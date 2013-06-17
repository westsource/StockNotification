using System.Net.Mail;
using System.Net.Mime;

namespace StockNotification.WinService.Common
{
    class MailSender
    {
        private readonly MailMessage mail;
        private readonly string password;//发件人密码 
        private readonly string user;

        /// <summary>  
        /// 处审核后类的实例  
        /// </summary>  
        /// <param name="to">收件人地址</param>  
        /// <param name="from">发件人地址</param>  
        /// <param name="body">邮件正文</param>  
        /// <param name="subject">邮件的主题</param>  
        /// <param name="sendAccount">发件人账号</param>
        /// <param name="sendPassword">发件人密码</param>  
        public MailSender(string to, string from, string body, string subject, string sendAccount, string sendPassword)
        {
            mail = new MailMessage();
            mail.To.Add(to);
            mail.From = new MailAddress(from);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.Priority = MailPriority.Normal;
            this.password = sendPassword;
            this.user = sendAccount;
        }
        /// <summary>  
        /// 添加附件  
        /// </summary>  
        public void Attachments(string path)  
        {  
            string[] pathes = path.Split(',');  
            for (int i = 0; i < pathes.Length; i++)  
            {  
                var data = new Attachment(pathes[i], MediaTypeNames.Application.Octet);//实例化附件  
                var disposition = data.ContentDisposition;  
                disposition.CreationDate = System.IO.File.GetCreationTime(pathes[i]);//获取附件的创建日期  
                disposition.ModificationDate = System.IO.File.GetLastWriteTime(pathes[i]);//获取附件的修改日期  
                disposition.ReadDate = System.IO.File.GetLastAccessTime(pathes[i]);//获取附件的读取日期  
                mail.Attachments.Add(data);//添加到附件中  
            }  
        }
        /// <summary>  
        /// 异步发送邮件  
        /// </summary>  
        /// <param name="CompletedMethod"></param>  
        public void SendAsync(SendCompletedEventHandler CompletedMethod)
        {
            if (mail != null)
            {
                var smtpClient = new SmtpClient();
                smtpClient.Credentials = new System.Net.NetworkCredential(user, password);//设置发件人身份的票据  
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Host = "smtp." + mail.From.Host;
                smtpClient.SendCompleted += CompletedMethod;//注册异步发送邮件完成时的事件  
                smtpClient.SendAsync(mail, mail.Body);
            }
        }
        /// <summary>  
        /// 发送邮件  
        /// </summary>  
        public void Send()
        {
            if (mail != null)
            {
                var smtpClient = new SmtpClient();
                smtpClient.Credentials = new System.Net.NetworkCredential(user, password);//设置发件人身份的票据  
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Host = "smtp." + mail.From.Host;
                smtpClient.Send(mail);
            }
        }  
    }

}
