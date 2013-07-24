using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Linq;

using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.DataLayer;
using umbraco.interfaces;

using System.Web.Script.Serialization;

namespace FormStorage
{
    public class PrevalueEditor : System.Web.UI.UpdatePanel, IDataPrevalue
    {
        // referenced datatype
        private umbraco.cms.businesslogic.datatype.BaseDataType _datatype;

        private TextBox saveBox;
        private HtmlGenericControl wrapperDiv = new HtmlGenericControl("div");
        private HtmlGenericControl settingsTable = new HtmlGenericControl("table");
        private HtmlGenericControl fieldsTable = new HtmlGenericControl("table");

        private JavaScriptSerializer jsonSerializer;
        private Options savedOptions, renderingOptions;

        private FormSchema formSchema;

        public PrevalueEditor(umbraco.cms.businesslogic.datatype.BaseDataType DataType)
        {
            _datatype = DataType;
            jsonSerializer = new JavaScriptSerializer();
            savedOptions = Configuration;
        }

        public Control Editor
        {
            get
            {
                return this;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            saveBox = new TextBox();
            saveBox.TextMode = TextBoxMode.MultiLine;
            saveBox.CssClass = "saveBox";
            ContentTemplateContainer.Controls.Add(saveBox);

            wrapperDiv.Attributes["class"] = "wrapperDiv";
            ContentTemplateContainer.Controls.Add(wrapperDiv);

            string css = string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" />", "/App_Plugins/FormStorage/Prevalue.css");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "FormStoragePrevalueCSS", css, false);

            string js = string.Format("<script src=\"{0}\" ></script>", "/App_Plugins/FormStorage/Prevalue.js");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "FormStoragePrevalueJS", js, false);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            buildControls();
        }

        private void buildControls()
        {
            //test for postback, decide to use db or saveBox for rendering
            if (Page.IsPostBack)
            {
                //test for saveBox having a value, default if not
                if (saveBox.Text != "")
                {
                    renderingOptions = jsonSerializer.Deserialize<Options>(saveBox.Text);
                }
                else
                {
                    renderingOptions = new Options();
                }
            }
            else
            {
                renderingOptions = savedOptions;
            }


            BuildSettingsTable();

            if(!String.IsNullOrEmpty(renderingOptions.alias)){
                BuildFieldsTable();
            }
        }

        private void BuildSettingsTable()
        {
            HtmlGenericControl tr, th, td;
            TextBox name, alias;

            settingsTable.Attributes["class"] = "settingsTable";
            wrapperDiv.Controls.Add(settingsTable);
            
            //alias
            tr = new HtmlGenericControl("tr");
            settingsTable.Controls.Add(tr);

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "Alias";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            alias = new TextBox();
            alias.Attributes["class"] = "alias";
            td.Controls.Add(alias);
            alias.Text = renderingOptions.alias;

            //name
            if (!String.IsNullOrEmpty(renderingOptions.alias))
            {
                formSchema = new FormSchema(renderingOptions.alias);

                tr = new HtmlGenericControl("tr");
                settingsTable.Controls.Add(tr);

                th = new HtmlGenericControl("th");
                tr.Controls.Add(th);
                th.InnerHtml = "Name";

                td = new HtmlGenericControl("td");
                tr.Controls.Add(td);
                name = new TextBox();
                name.Attributes["class"] = "name";
                td.Controls.Add(name);
                name.Text = renderingOptions.name;
            }
        }

        private void BuildFieldsTable()
        {
            HtmlGenericControl tr, th, thead, tbody;

            fieldsTable.Attributes["class"] = "fieldsTable";
            wrapperDiv.Controls.Add(fieldsTable);

            thead = new HtmlGenericControl("thead");
            fieldsTable.Controls.Add(thead);

            tr = new HtmlGenericControl("tr");
            thead.Controls.Add(tr);

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "Name";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "Alias";

            th = new HtmlGenericControl("th");
            tr.Controls.Add(th);
            th.InnerHtml = "";

            tbody = new HtmlGenericControl("tbody");
            fieldsTable.Controls.Add(tbody);

            //list each form field            
            var sortedFields = formSchema.FormFields.OrderBy(o => o.sortOrder);

            foreach (FormField thisField in sortedFields)
            {
                AddField(thisField, tbody);
            }

            //add in a blank
            AddField(new FormField(), tbody);
        }

        private void AddField(FormField field, HtmlGenericControl tbody)
        {
            HtmlGenericControl tr, td;

            tr = new HtmlGenericControl("tr");
            tr.Attributes["fieldID"] = field.ID.ToString();
            tbody.Controls.Add(tr); 

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["class"] = "fieldControls";
            td.InnerHtml = "<img class='sortFieldsHandle' src='/App_Plugins/FormStorage/images/sort.png'/><img class='add' src='/App_Plugins/FormStorage/images/plus.png'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.InnerHtml = "<input type='text' class='name' value='" + HttpUtility.UrlDecode(field.name) + "'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.InnerHtml = "<input type='text' class='alias' value='" + HttpUtility.UrlDecode(field.alias) + "'/>";

            td = new HtmlGenericControl("td");
            tr.Controls.Add(td);
            td.Attributes["class"] = "fieldControls2";
            td.InnerHtml = "<img class='remove' src='/App_Plugins/FormStorage/images/minus.png'/>";
        }

        public void Save()
        {

            //save settings
            _datatype.DBType = (umbraco.cms.businesslogic.datatype.DBTypes)Enum.Parse(typeof(umbraco.cms.businesslogic.datatype.DBTypes), DBTypes.Ntext.ToString(), true);

            FormStorageCore.SqlHelper.ExecuteNonQuery("delete from cmsDataTypePreValues where datatypenodeid = @dtdefid", FormStorageCore.SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId));
            FormStorageCore.SqlHelper.ExecuteNonQuery("insert into cmsDataTypePreValues (datatypenodeid,[value],sortorder,alias) values (@dtdefid,@value,0,'')", FormStorageCore.SqlHelper.CreateParameter("@dtdefid", _datatype.DataTypeDefinitionId), FormStorageCore.SqlHelper.CreateParameter("@value", saveBox.Text));

            //save formschema
        }

        public Options Configuration
        {
            get
            {
                string dbValue = "";
                try
                {
                    object conf = FormStorageCore.SqlHelper.ExecuteScalar<object>("select value from cmsDataTypePreValues where datatypenodeid = @datatypenodeid", FormStorageCore.SqlHelper.CreateParameter("@datatypenodeid", _datatype.DataTypeDefinitionId));
                    dbValue = conf.ToString();
                }
                catch (Exception e)
                {
                }

                if (dbValue.ToString() != "")
                {
                    return jsonSerializer.Deserialize<Options>(dbValue.ToString());
                }
                else
                {
                    return new Options();
                }
            }
        }       
    }
}