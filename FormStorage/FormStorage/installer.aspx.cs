using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using umbraco.BusinessLogic;

namespace FormStorage
{
    public partial class Installer : System.Web.UI.UserControl
    {
        private HtmlGenericControl messageList, li;

        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Add(LogTypes.Custom, 0, "Running FormStorage Installer...");

            messageList = new HtmlGenericControl("ul");
            wrapper.Controls.Add(messageList);

            AddTable("FormStorage", @"
                CREATE TABLE [dbo].[FormStorageForms](
	                [formID] [int] IDENTITY(1,1) NOT NULL,
	                [alias] [nvarchar](50) NOT NULL,
                 CONSTRAINT [PK_SimpleForms] PRIMARY KEY CLUSTERED 
                (
	                [formID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]
            ");           

            AddTable("FormStorageSubmissions", @"  
                CREATE TABLE [dbo].[FormStorageSubmissions](
	                [submissionID] [int] IDENTITY(1,1) NOT NULL,
	                [formID] [int] NOT NULL,
	                [IP] [nvarchar](50) NOT NULL,
	                [datetime] [datetime] NOT NULL,
                 CONSTRAINT [PK_SimpleFormSubmissions] PRIMARY KEY CLUSTERED 
                (
	                [submissionID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[FormStorageSubmissions]  WITH CHECK ADD  CONSTRAINT [FK_SimpleFormSubmissions_SimpleForms] FOREIGN KEY([formID])
                REFERENCES [dbo].[FormStorageForms] ([formID])

                ALTER TABLE [dbo].[FormStorageSubmissions] CHECK CONSTRAINT [FK_SimpleFormSubmissions_SimpleForms]                
            ");

            AddTable("FormStorageEntries", @" 
                CREATE TABLE [dbo].[FormStorageEntries](
	                [entryID] [int] IDENTITY(1,1) NOT NULL,
	                [submissionID] [int] NOT NULL,
	                [fieldAlias] [nvarchar](25) NOT NULL,
	                [value] [nvarchar](max) NOT NULL,
                 CONSTRAINT [PK_FormStorageEntries] PRIMARY KEY CLUSTERED 
                (
	                [entryID] ASC
                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                ) ON [PRIMARY]            
            ");
        }

        private void AddTable(string name, string SQL)
        {
            li = new HtmlGenericControl("li");
            messageList.Controls.Add(li);
            li.InnerHtml = "Adding Table '" + name + "'...";
            try
            {
                FormStorageCore.SqlHelper.ExecuteNonQuery(SQL);
            }
            catch (Exception e)
            {
                li = new HtmlGenericControl("li");
                messageList.Controls.Add(li);
                li.InnerHtml = "ERROR: Adding Table '" + name + "' " + e.Message;
                Log.Add(LogTypes.Custom, 0, "ERROR: Adding Table '" + name + "' " + e.Message);
            }
            li = new HtmlGenericControl("li");
            messageList.Controls.Add(li);
            li.InnerHtml = "'" + name + "' added successfully.";
        }
    }
}