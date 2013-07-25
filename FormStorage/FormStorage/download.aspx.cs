using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;


namespace FormStorage
{
    public partial class Download : System.Web.UI.Page
    {
        HttpRequest request = HttpContext.Current.Request;

        protected void Page_Load(object sender, EventArgs e)
        {
            //(string keywords, string formAlias, string occurring, int maxResults=25

            FormStorageWebService service = new FormStorageWebService();
            service.GetSubmissions(request.Params["keywords"], request.Params["formAlias"], request.Params["occurring"], Convert.ToInt32(request.Params["maxResults"]));

            string attachment = "attachment; filename=" + request.Params["formAlias"] + ".xls";
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.ClearHeaders();
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.AddHeader("content-disposition", attachment);
            HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            HttpContext.Current.Response.AddHeader("Pragma", "public");

            StringBuilder sb = new StringBuilder();

            //headers
            FormSchema formSchema = new FormSchema(request.Params["formAlias"], false);

            sb.Append("Id\t");
            sb.Append("Date/Time\t");
            sb.Append("IP\t");

            foreach (string formField in formSchema.FormFields)
            {
                sb.Append(HttpUtility.UrlDecode(FormStorageCore.GetDictionaryItem(formField)) + "\t");
            }
            sb.Append(Environment.NewLine);

            //data
            foreach (ResultRow row in service.Submissions)
            {
                sb.Append(row.ID + "\t");
                sb.Append(row.dateTime + "\t");
                sb.Append(row.IP + "\t");
                foreach (string value in row.values)
                {
                    //strip out html breaks
                    sb.Append(value.Replace("<br/>", " ") + "\t");
                }

                sb.Append(Environment.NewLine);
            }

            HttpContext.Current.Response.Write(sb.ToString());
            HttpContext.Current.Response.End();
        }
    }
}