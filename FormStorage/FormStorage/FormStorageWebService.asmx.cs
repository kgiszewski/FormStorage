using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;

using umbraco.DataLayer;
using umbraco.BusinessLogic;

namespace FormStorage
{
    /// <summary>
    /// Summary description for FormStorageWebService
    /// </summary>
    [WebService(Namespace = "http://foobar.com/")]

    [System.Web.Script.Services.ScriptService]
    public class FormStorageWebService : System.Web.Services.WebService
    {
        private JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
        private Dictionary<string, string> returnValue = new Dictionary<string, string>();
        private enum status { SUCCESS, ERROR };

        private List<ResultRow> submissionResult;

        public List<ResultRow> Submissions
        {
            get { return submissionResult; }
        }

        [WebMethod]
        public Dictionary<string, string> DeleteSubmission(int submissionID)
        {
            FormStorageCore.Authorize();

            IParameter[] parameters = new IParameter[1];
            parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@submissionID", submissionID);

            try
            {
                int entriesDeleted = FormStorageCore.SqlHelper.ExecuteNonQuery(@"
                    DELETE
                    FROM FormStorageEntries
                    WHERE submissionID=@submissionID
                ", parameters);
            }
            catch (Exception e)
            {
                returnValue.Add("status", status.ERROR.ToString());
                returnValue.Add("message", "An error has occurred. " + e.Message);
            }

            try
            {
                int submissionsDeleted = FormStorageCore.SqlHelper.ExecuteNonQuery(@"
                    DELETE
                    FROM FormStorageSubmissions
                    WHERE submissionID=@submissionID
                ", parameters);
            }
            catch (Exception e)
            {
                returnValue.Add("status", status.ERROR.ToString());
                returnValue.Add("message", "An error has occurred. " + e.Message);
            }

            returnValue.Add("status", status.SUCCESS.ToString());

            return returnValue;
        }

        [WebMethod]
        public Dictionary<string, string> GetSubmissions(string keywords, string formAlias, string occurring, int maxResults = 25)
        {
            FormStorageCore.Authorize();

            List<Entry> entries = new List<Entry>();

            string[] keywordList = new string[3] { "", "", "" };

            int index = 0;

            foreach (string keyword in HttpUtility.UrlDecode(keywords).Split(' '))
            {
                keywordList[index] = keyword;
                index++;
            }

            string begDate = "";
            string endDate = "";
            string occuringSQL = "";

            if (occurring != "")
            {
                begDate = DateTime.Now.ToString("yyyy-MM-dd");
                endDate = DateTime.Now.AddDays(Convert.ToInt32(occurring)).ToString("yyyy-MM-dd");

                occuringSQL = " AND [datetime]<='" + begDate + " 23:59:59'" + " AND [datetime]>='" + endDate + " 00:00:00'";
            }

            try
            {
                IParameter[] parameters = new IParameter[4];
                parameters[0] = FormStorageCore.SqlHelper.CreateParameter("@alias", formAlias);
                parameters[1] = FormStorageCore.SqlHelper.CreateParameter("@key1", "%" + keywordList[0] + "%");
                parameters[2] = FormStorageCore.SqlHelper.CreateParameter("@key2", "%" + keywordList[1] + "%");
                parameters[3] = FormStorageCore.SqlHelper.CreateParameter("@key3", "%" + keywordList[2] + "%");

                IRecordsReader reader = FormStorageCore.SqlHelper.ExecuteReader(@"
                    SELECT 
                        fs.submissionID, fs.formID, IP, [datetime], entryID, value, fieldAlias
                    FROM FormStorageSubmissions fs
                    LEFT JOIN 
                        FormStorageForms sf on sf.formID=fs.formID
                    LEFT JOIN 
                        FormStorageEntries fe on fe.submissionID=fs.submissionID
	                WHERE sf.alias=@alias
                      AND fs.submissionID IN (
                            SELECT submissionID
                            FROM FormStorageEntries
                            WHERE value LIKE @key1
                          )

                      AND fs.submissionID IN (
                            SELECT submissionID
                            FROM FormStorageEntries
                            WHERE value LIKE @key2
                          )                          
	
                      AND fs.submissionID IN (
                            SELECT submissionID
                            FROM FormStorageEntries
                            WHERE value LIKE @key3
                          )
                    " + occuringSQL + @"
	                ORDER BY submissionID DESC
                ", parameters);

                int submissionID = 0;

                Entry thisEntry = new Entry();//ini

                while (reader.Read())
                {
                    int recordSubmissionID = reader.Get<int>("submissionID");

                    //create a new Entry if a new submissionID
                    if (submissionID != recordSubmissionID)
                    {
                        if (entries.Count() == maxResults)
                        {
                            break;
                        }

                        thisEntry = new Entry()
                        {
                            ID = reader.Get<int>("submissionID"),
                            dateTime = reader.Get<DateTime>("datetime").ToUniversalTime().ToString(),
                            IP = reader.Get<string>("IP")
                        };
                        entries.Add(thisEntry);
                        thisEntry.fields.Add(new SavedValue() { value = reader.Get<string>("value"), fieldAlias = reader.Get<string>("fieldAlias") });

                        submissionID = recordSubmissionID;
                    }
                    else
                    {
                        //otherwise add in the fields only
                        thisEntry.fields.Add(new SavedValue() { value = reader.Get<string>("value"), fieldAlias = reader.Get<string>("fieldAlias") });
                    }
                }

                reader.Dispose();
                FormStorageCore.SqlHelper.Dispose();

                //get the form schema for headers
                FormSchema formSchema = new FormSchema(formAlias, false);

                List<ResultRow> finalResults = new List<ResultRow>();
                foreach (Entry entry in entries)
                {
                    ResultRow resultRow = new ResultRow()
                    {
                        ID = entry.ID,
                        dateTime = entry.dateTime,
                        IP = entry.IP,
                        values = new List<string>()
                    };

                    //match up the fields
                    foreach (string fieldAlias in formSchema.FormFields)
                    {
                        bool foundField = false;
                        foreach (SavedValue thisSavedValue in entry.fields)
                        {
                            if (thisSavedValue.fieldAlias == fieldAlias)
                            {
                                resultRow.values.Add(thisSavedValue.value.Replace(Environment.NewLine, "<br/>"));
                                foundField = true;
                            }
                        }
                        if (foundField == false)
                        {
                            resultRow.values.Add("");
                        }
                    }

                    finalResults.Add(resultRow);
                }

                //save the results to a class member
                submissionResult = finalResults;

                returnValue.Add("status", status.SUCCESS.ToString());
                returnValue.Add("entries", jsonSerializer.Serialize(finalResults));

                return returnValue;
            }
            catch (Exception e)
            {
                returnValue.Add("status", status.ERROR.ToString());
                returnValue.Add("message", e.Message);

                Log.Add(LogTypes.Custom, 0, e.Message + "<br/>\n" + e.StackTrace);

                return returnValue;
            }
        }
    }

    public class Entry
    {
        public int ID = 0;
        public string dateTime = "";
        public string IP = "";
        public List<SavedValue> fields = new List<SavedValue>();
    }

    public class SavedValue
    {
        public string value = "";
        public string fieldAlias = "";
    }

    public class ResultRow
    {
        public int ID = 0;
        public string dateTime = "";
        public string IP = "";
        public List<string> values = new List<string>();
    }
}
