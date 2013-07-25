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

        private JavaScriptSerializer jsonSerializer;
        private Options savedOptions, renderingOptions;

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
        }

        private void BuildSettingsTable()
        {
            HtmlGenericControl tr, th, td;
            TextBox alias;

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