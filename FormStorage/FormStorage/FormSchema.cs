using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

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
            //try
            //{
                SetFormByAlias(alias, createNewIfNotFound);

                //get the appSettings
                formFields.AddRange(System.Web.Configuration.WebConfigurationManager.AppSettings["FormStorage:" + alias].Split(','));
            //}
            //catch { }
        }        

        private void SetFormByAlias(string alias, bool createNewIfNotFound)
        {
            this.alias = alias;
            IRecordsReader reader = FormStorageCore.SqlHelper.ExecuteReader("SELECT formID FROM FormStorageForms WHERE alias = @alias", FormStorageCore.SqlHelper.CreateParameter("@alias", alias));
            
            while(reader.Read()){
                DBformID = reader.Get<int>("formID");
            }

            //creates new form in DB if not found
            if (DBformID == 0 && createNewIfNotFound)
            {
                //throw new Exception("Form not found in the FormStorage table.");
                IParameter[] parameters = new IParameter[1];
                parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@alias", alias);

                //could throw unique constraint
                //try
                //{
                    FormStorageCore.SqlHelper.ExecuteNonQuery(@"
                        INSERT 
                        INTO FormStorageForms
                        (alias) 
                        VALUES 
                        (@alias);
                    ", parameters);

                    IParameter[] parameters2 = new IParameter[1];
                    parameters2[0] = FormStorageCore.SqlHelper.CreateParameter("@alias", alias);

                    DBformID = FormStorageCore.SqlHelper.ExecuteScalar<int>(@"
                        SELECT formID FROM FormStorageForms WHERE alias=@alias;
                    ", parameters2);
                //}
                //catch (Exception e)
                //{

                //}

                if (DBformID == 0)
                {
                    throw new Exception("FormID is still zero");
                }
            }
        }        
        
        public int CreateSubmission(Dictionary<string, string> dataDictionary)
        {
            string ipAddy = FormStorageCore.GetUserIP();

            string sSourceData=ipAddy+DateTime.Now.Ticks.ToString();
            byte[] tmpSource;
            byte[] hash;
            tmpSource = Encoding.UTF8.GetBytes(sSourceData);
            hash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
            string md5 = Convert.ToBase64String(hash);

            IParameter[] parameters = new IParameter[3];
            parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@DBformID", DBformID);
            parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@ipAddress", ipAddy);
            parameters[2] = FormStorageCore.SqlHelper.CreateParameter("@hash", md5);

            //create submission
            FormStorageCore.SqlHelper.ExecuteNonQuery(@"
                INSERT 
                INTO FormStorageSubmissions (formID, IP, datetime, hash) 
                VALUES 
                (@DBformID, @ipAddress, GETDATE(), @hash);"
            , parameters);

            IParameter[] parameters2 = new IParameter[1];
            parameters2[0] = FormStorageCore.SqlHelper.CreateParameter("@hash", md5);

            int submissionID=FormStorageCore.SqlHelper.ExecuteScalar<int>(@"
                SELECT submissionID FROM FormStorageSubmissions WHERE hash=@hash
            ", parameters2);
            //HttpContext.Current.Response.Write("Submission ID=>" + submissionID + "<br/>");

            if (submissionID == 0)
            {
                throw new Exception("Could not create a record in table FormStorageSubmissions.");
            }

            parameters = new IParameter[3];
            parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@submissionID", submissionID);

            foreach (string formField in formFields)
            {
               //try
                //{
                    parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@fieldAlias", formField);
                    parameters[2] = FormStorageCore.SqlHelper.CreateParameter("@value", HttpUtility.HtmlEncode(dataDictionary[formField]));
                    int entryID = FormStorageCore.SqlHelper.ExecuteScalar<int>(@"
                        INSERT 
                        INTO FormStorageEntries 
                        (submissionID, fieldAlias, value) 
                        VALUES 
                        (@submissionID, @fieldAlias, @value);
                    ", parameters);
                //}
                //catch (Exception e2)
                //{
                    //HttpContext.Current.Response.Write(e2.Message);
                //}
            }

            return submissionID;
        }        
    }  
}