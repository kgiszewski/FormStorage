using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
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
        private TextBox saveBox;
        private HtmlGenericControl wrapperDiv;
        public string CurrentData = "";

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

            string css = string.Format("<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" />", "/App_Plugins/InlineImagePicker/InlineImagePicker.css");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "InlineImagePickerCSS", css, false);

            string js = string.Format("<script src=\"{0}\" ></script>", "/App_Plugins/InlineImagePicker/InlineImagePicker.js");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "InlineImagePickerJS", js, false);

            string js2 = string.Format("<script src=\"{0}\" ></script>", "/App_Plugins/InlineImagePicker/sort.js");
            ScriptManager.RegisterClientScriptBlock(Page, typeof(DataEditor), "InlineImagePickerSortJS", js2, false);

            saveBox = new TextBox();
            saveBox.TextMode = TextBoxMode.MultiLine;
            saveBox.CssClass = "InlineImagePickerSaveBox";
            ContentTemplateContainer.Controls.Add(saveBox);

            wrapperDiv = new HtmlGenericControl("div");
            wrapperDiv.Attributes["class"] = "InlineImagePickerWrapperDiv";
            ContentTemplateContainer.Controls.Add(wrapperDiv);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
           

        }

        private void buildControls()
        {
            string data;

            //get the data based on action
            if (Page.IsPostBack)
            {
                data = saveBox.Text;
            }
            else
            {
                data = savedData.Value.ToString();
            }

            //this overrides everything
            if (CurrentData != "")
            {
                data = CurrentData;
            }

            //load the data into an xml doc
            XmlDocument xd = new XmlDocument();

            try
            {
                xd.LoadXml(data);
            }
            catch (Exception e)
            {
                xd.LoadXml(DefaultData.defaultXML);
            }

            XmlNode selectedDataXML = xd.SelectSingleNode("//image");             
            
        }      

        public void Save()
        {
            savedData.Value = saveBox.Text;
        }
    }
}