using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using umbraco.BusinessLogic;

namespace FormStorage
{

    public class DefaultData : umbraco.cms.businesslogic.datatype.DefaultData
    {

        public static string defaultXML = "<formStorage/>";

        public DefaultData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) { }

        public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument data)
        {

            XmlDocument xd = new XmlDocument();
            try
            {
                xd.LoadXml(this.Value.ToString());
            }
            catch (Exception e)
            {
                xd.LoadXml(defaultXML);
            }

            return data.ImportNode(xd.DocumentElement, true);
        }
    }
}

