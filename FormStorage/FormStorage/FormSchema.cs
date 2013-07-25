using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace FormStorage
{
    public class FormSchema
    {
        private int DBformID = 0;
        private List<string> formFields = new List<string>();
        private string alias = "";

        public int ID
        {
            get { return DBformID; }
        }

        public string Alias
        {
            get { return alias; }
        }

        public List<string> FormFields
        {
            get { return formFields; }
        }

        public FormSchema(string alias, bool createNewIfNotFound = true)
        {
            SetFormByAlias(alias, createNewIfNotFound);
            
            //get the appSettings
            formFields.AddRange(System.Web.Configuration.WebConfigurationManager.AppSettings["FormStorage:"+alias].Split(','));
        }        

        private void SetFormByAlias(string alias, bool createNewIfNotFound)
        {
            this.alias = alias;
            IRecordsReader reader = FormStorageCore.SqlHelper.ExecuteReader("SELECT formID FROM FormStorageForms WHERE alias = @alias", FormStorageCore.SqlHelper.CreateParameter("@alias", alias));

            if (reader.HasRecords)
            {
                reader.Read();
                DBformID = reader.Get<int>("formID");
            }

            //creates new form in DB if not found
            if (DBformID == 0 && createNewIfNotFound)
            {
                //throw new Exception("Form not found in the FormStorage table.");
                IParameter[] parameters = new IParameter[1];
                parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@alias", alias);

                //could throw unique constraint
                try
                {
                    DBformID = FormStorageCore.SqlHelper.ExecuteScalar<int>(@"
                        INSERT 
                        INTO FormStorageForms
                        (alias) 
                        VALUES 
                        (@alias);
                        SELECT SCOPE_IDENTITY() AS formID;
                    ", parameters);
                }
                catch (Exception e)
                {

                }
            }
        }        
        
        public int CreateSubmission(Dictionary<string, string> dataDictionary)
        {
            IParameter[] parameters = new IParameter[2];
            parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@DBformID", DBformID);
            parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@ipAddress", FormStorageCore.GetUserIP());

            //create submission
            int submissionID = FormStorageCore.SqlHelper.ExecuteScalar<int>("INSERT INTO FormStorageSubmissions (formID, IP, datetime) VALUES (@DBformID, @ipAddress, GETDATE());SELECT SCOPE_IDENTITY() AS submissionID;", parameters);
            //HttpContext.Current.Response.Write("Submission ID=>" + submissionID + "<br/>");

            if (submissionID == 0)
            {
                throw new Exception("Could not create a record in table FormStorageSubmissions.");
            }

            parameters = new IParameter[3];
            parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@submissionID", submissionID);

            foreach (string formField in formFields)
            {
                try
                {
                    parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@fieldAlias", formField);
                    parameters[2] = FormStorageCore.SqlHelper.CreateParameter("@value", HttpUtility.HtmlEncode(dataDictionary[formField]));
                    int entryID = FormStorageCore.SqlHelper.ExecuteScalar<int>("INSERT INTO FormStorageEntries (submissionID, fieldAlias, value) VALUES (@submissionID, @fieldAlias, @value);SELECT SCOPE_IDENTITY() AS entryID;", parameters);
                }
                catch (Exception e2)
                {
                    //HttpContext.Current.Response.Write(e2.Message);
                }
            }

            return submissionID;
        }        
    }  
}