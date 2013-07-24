﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

using umbraco.BusinessLogic;
using umbraco.DataLayer;

namespace FormStorage
{
    public class FormStorageCore
    {
        public static void Authorize()
        {
            if (umbraco.BusinessLogic.User.GetCurrent() == null)
            {
                HttpContext.Current.Response.StatusCode = 403;
                HttpContext.Current.Response.End();
            }
        }

        public static string GetUserIP()
        {
            string ipList = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
        }

        public static ISqlHelper SqlHelper
        {
            get
            {
                return Application.SqlHelper;
            }
        }

        public void SendMail(string to, string from, string subject, string body, bool html)
        {
            MailMessage message = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(from),
                Subject = subject,
                IsBodyHtml = html
            };

            message.To.Add(to);

            message.Body = body;
            SmtpClient smtp = new System.Net.Mail.SmtpClient();
            smtp.Send(message);
        }
    }
}