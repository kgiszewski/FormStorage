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
        private List<FormField> formFields = new List<FormField>();
        private string formName = "";
        private string alias = "";

        public int ID
        {
            get { return DBformID; }
        }

        public string Name
        {
            get { return formName; }
        }

        public string Alias
        {
            get { return alias; }
        }

        public FormSchema(string alias, bool createNewIfNotFound = true)
        {
            SetFormByAlias(alias, createNewIfNotFound);
            SetFormFieldsByFormId(DBformID);
        }

        public List<FormField> FormFields
        {
            get { return formFields; }
        }

        private void SetFormByAlias(string alias, bool createNewIfNotFound)
        {
            this.alias = alias;
            IRecordsReader reader = FormStorageCore.SqlHelper.ExecuteReader("SELECT formID, name FROM FormStorageForms WHERE alias = @alias", FormStorageCore.SqlHelper.CreateParameter("@alias", alias));

            if (reader.HasRecords)
            {
                reader.Read();
                DBformID = reader.Get<int>("formID");
                formName = reader.Get<string>("name");
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

        private void SetFormFieldsByFormId(int formID)
        {
            IRecordsReader reader = FormStorageCore.SqlHelper.ExecuteReader("SELECT * FROM FormStorageFields WHERE formID = @formID", FormStorageCore.SqlHelper.CreateParameter("@formID", formID));

            if (!reader.HasRecords)
            {
                //throw new Exception("Fields not found for the given form ID.");
            }

            while (reader.Read())
            {
                formFields.Add(new FormField() { ID = reader.Get<int>("fieldID"), name = reader.Get<string>("name"), alias = reader.Get<string>("alias"), sortOrder = reader.Get<int>("sortOrder") });
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

            foreach (FormField formField in formFields)
            {
                try
                {
                    parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@fieldID", formField.ID);
                    parameters[2] = FormStorageCore.SqlHelper.CreateParameter("@value", HttpUtility.HtmlEncode(dataDictionary[formField.alias]));
                    int entryID = FormStorageCore.SqlHelper.ExecuteScalar<int>("INSERT INTO FormStorageEntries (submissionID, fieldID, value) VALUES (@submissionID, @fieldID, @value);SELECT SCOPE_IDENTITY() AS entryID;", parameters);
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