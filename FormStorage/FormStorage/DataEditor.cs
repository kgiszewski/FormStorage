using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Xml;

namespace FormStorage
{
    /// <summary>
    /// This class is used for the actual datatype dataeditor, i.e. the control you will get in the content section of umbraco. 
    /// </summary>
    public class DataEditor : System.Web.UI.UpdatePanel, umbraco.interfaces.IDataEditor
    {
        private umbraco.interfaces.IData savedData;
        private Options savedOptions;
        private XmlDocument savedXML = new XmlDocument();
        private FormSchema formSchema;
        TextBox keywords, maxResults;
        DropDownList dateChoices;
        HtmlGenericControl h1, h2, div, label, input, button, table, thead, tbody, tr, th, td, urlTD, statusTD, wrapperDiv, submissions;
        
        public DataEditor(umbraco.interfaces.IData Data, Options Configuration)
        {
            //load the prevalues
            savedOptions = Configuration;

            //ini the savedData object
            savedData = Data;
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public Control Editor { get { return this; } }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            string css = string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" />", "/App_Plugins/FormStorage/FormStorage.css");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "FormStorageCSS", css, false);

            string js = string.Format("<script src=\"{0}\" ></script>", "/App_Plugins/FormStorage/FormStorage.js");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "FormStorageJS", js, false);    

            wrapperDiv = new HtmlGenericControl("div");
            wrapperDiv.Attributes["class"] = "FormStorageWrapperDiv";
            ContentTemplateContainer.Controls.Add(wrapperDiv);

            submissions = new HtmlGenericControl("div");
            submissions.Attributes["class"] = "FormStorageSubmissionsDiv";
            wrapperDiv.Controls.Add(submissions);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (!String.IsNullOrEmpty(savedOptions.alias))
            {
                buildControls();
            }
        }

        private void buildControls()
        {            
            formSchema = new FormSchema(savedOptions.alias);

            wrapperDiv.Attributes["alias"] = savedOptions.alias;

            BuildSubmissionsUI();
            BuildSubmissionsTable();
        }

        private void BuildSubmissionsUI()
        {
            //title
            h2 = new HtmlGenericControl("h2");
            submissions.Controls.Add(h2);
            h2.InnerHtml = FormStorageCore.GetDictionaryItem("Submissions");

            //UI
            div = new HtmlGenericControl("div");
            submissions.Controls.Add(div);
            div.Attributes["class"] = "resultsUI";

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = FormStorageCore.GetDictionaryItem("Keywords");

            keywords = new TextBox();
            keywords.Attributes["class"] = "keywords";
            div.Controls.Add(keywords);

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = FormStorageCore.GetDictionaryItem("Max Results");

            maxResults = new TextBox();
            maxResults.Attributes["class"] = "maxResults";
            maxResults.Text = "25";
            div.Controls.Add(maxResults);

            label = new HtmlGenericControl("label");
            div.Controls.Add(label);
            label.InnerHtml = FormStorageCore.GetDictionaryItem("Occurring");

            dateChoices = new DropDownList();
            div.Controls.Add(dateChoices);
            dateChoices.Attributes["class"] = "occurring";
            if (dateChoices.Items.Count == 0)
            {
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("Ever"), ""));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("Today"), "0"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("Yesterday and Today"), "-1"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 7 Days"), "-7"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 30 Days"), "-30"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 60 Days"), "-60"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 90 Days"), "-90"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 180 Days"), "-180"));
                dateChoices.Items.Add(new ListItem(FormStorageCore.GetDictionaryItem("In the Last 365 Days"), "-365"));
            }

            input = new HtmlGenericControl("input");
            div.Controls.Add(input);
            input.Attributes["type"] = "button";
            input.Attributes["class"] = "search";
            input.Attributes["value"] = FormStorageCore.GetDictionaryItem("Search");

            input = new HtmlGenericControl("input");
            div.Controls.Add(input);
            input.Attributes["type"] = "button";
            input.Attributes["class"] = "download";
            input.Attributes["value"] = FormStorageCore.GetDictionaryItem("Download");
        }

        private void BuildSubmissionsTable()
        {
            //results
            div = new HtmlGenericControl("div");
            submissions.Controls.Add(div);
            div.Attributes["class"] = "resultsDiv";

            //table
            table = new HtmlGenericControl("table");
            div.Controls.Add(table);

            thead = new HtmlGenericControl("thead");
            table.Controls.Add(thead);

            //build the column headers
            tr = new HtmlGenericControl("tr");
            thead.Controls.Add(tr);

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "<a href='#'>Date/Time</a>";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "<a href='#'>IP</a>";

            foreach (string thisFormField in formSchema.FormFields)
            {
                th = new HtmlGenericControl("th");
                tr.Controls.Add(th);
                th.InnerHtml = "<a href='#'>" + HttpUtility.UrlDecode(FormStorageCore.GetDictionaryItem(thisFormField)) + "</a>";
            }

            tbody = new HtmlGenericControl("tbody");
            table.Controls.Add(tbody);

            tr = new HtmlGenericControl("tr");
            tbody.Controls.Add(tr);

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["colspan"] = (formSchema.FormFields.Count + 2).ToString();
            td.InnerHtml = FormStorageCore.GetDictionaryItem("Click search for results.");
        }

        public void Save()
        {
            
        }
    }
}