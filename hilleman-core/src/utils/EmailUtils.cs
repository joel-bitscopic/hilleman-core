using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using com.bitscopic.hilleman.core.domain;

namespace com.bitscopic.hilleman.core.utils
{
    public static class EmailUtils
    {

        public static void sendEmail(String from, String to, String subject, String body, byte[] attachment)
        {
            IList<byte[]> attachments = new List<byte[]>();
            attachments.Add(attachment);
            sendEmail(from, to, subject, body, attachments);
        }

        public static void sendEmail(String from, String to, String subject, String body, IList<byte[]> attachments)
        {
            SmtpClient smtp = new SmtpClient(MyConfigurationManager.getValue("SmtpHost"), Int32.Parse(MyConfigurationManager.getValue("SmtpPort")));
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new System.Net.NetworkCredential(MyConfigurationManager.getValue("SmtpUsername"), MyConfigurationManager.getValue("SmtpPassword"));
            smtp.Timeout = 5000;
            
            MailMessage msg = new MailMessage(from, to, subject, body);

            if (attachments != null && attachments.Count > 0 && attachments[0] != null)
            {
                foreach (byte[] currentAttachment in attachments)
                {
                    MemoryStream ms = new MemoryStream(currentAttachment);
                    msg.Attachments.Add(new Attachment(ms, "invite.ics", "text/calendar"));
                }
            }

            smtp.Send(msg);
        }
    }
}