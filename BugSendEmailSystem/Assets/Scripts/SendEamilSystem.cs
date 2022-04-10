using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

public class SendEamilSystem
{

    private List<string> AlreadySendMessage = new List<string>();//已经发送的错误日志

    private List<string> ReadySendMessage = new List<string>();//准备发送的错误日志

    private bool isSending = false;
    private string SendingErr;
    private string bugfilepath;
    private Thread currentThread=null;
    public void Init()
    {
        isSending = false;
        
        Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
        {
            if (type == LogType.Error|| type== LogType.Assert
                || type == LogType.Exception)
            {
                string err = $"{condition} \n {stackTrace}";
                if (AlreadySendMessage.Contains(err))
                {
                    Debug.Log("该错误日志已经发送");
                    return;
                }
                ReadySendMessage.Add(err);
                
            }

// 线程中发送邮件，避免影响主线程

        };
    }
    

    public void UpdateData()
    {
        if (isSending)
        {
            return;
        }

        if (ReadySendMessage.Count>0)
        {
            SendingErr=ReadySendMessage[0];
            Debug.Log("发送错误日志"+SendingErr);
            ReadySendMessage.RemoveAt(0);
            isSending = true;
            AlreadySendMessage.Add(SendingErr);
            Test_SendBug.Inst.StartCoroutine(JieTu());
        }
    }

    IEnumerator JieTu()
    {

        yield return new WaitForEndOfFrame();

        Texture2D t = new Texture2D(Screen.width,Screen.height, TextureFormat.RGB24, false)
            ;
        t.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);

        byte[] bytes = t.EncodeToPNG();
        t.Compress(false);
        t.Apply();

        string filePath = Application.streamingAssetsPath+"/Bug";
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        var now = System.DateTime.Now;
        bugfilepath = filePath + $"/{now.Month}.{now.Day}.{now.Hour}.{now.Minute}.{now.Second}.png";
        File.WriteAllBytes(bugfilepath, bytes);
        
        
        string deviceinfo = $"operatingSystem=={SystemInfo.operatingSystem}   " +
                            $"processorType=={SystemInfo.processorType}   " +
                            $"graphicsDeviceVersion=={SystemInfo.graphicsDeviceVersion}   " +
                            $"systemMemorySize=={SystemInfo.systemMemorySize}   ";

        SendingErr=deviceinfo + " \n" + SendingErr;
        
        currentThread = new Thread(SendEmail);
        currentThread.Start();
        while (currentThread.IsAlive)
        {
            yield return new WaitForSeconds(0.2f);
        }
        if (currentThread!=null
            && currentThread.IsAlive)
        {
            currentThread.Abort();
        }
        isSending = false;
    }
    
    
    
    // 发送邮件
    void SendEmail() {
 
        // 新建一个发件体
        EmailFormat emailFormat = new EmailFormat();
 
        // 邮件账户和授权码，授权平台，端口（可不要）
        emailFormat.From = "11111111@qq.com";//替换自己要发送的QQ邮箱
        emailFormat.AuthorizationCode = "fugrgdrhbfdddhcj";//对应邮箱 开启smtp服务 并且获取code码  自行百度如何开启获取
        emailFormat.SmtpClient = "smtp.qq.com";

     

        var now = System.DateTime.Now;
        // 邮件主题和内容
        emailFormat.Subject = $"steam用户名  月{now.Month} 日{now.Day} 时{now.Hour} 分{now.Minute}";
        emailFormat.Body = SendingErr;
 
        
        // 添加附件，可多个
        emailFormat.Attachments.Add(bugfilepath);
        // 添加收件人，可多个
        emailFormat.To.Add("22222222@qq.com");//bug发送的指定邮箱地址


        try
        {
            // 发送邮件
            SendEamilSystem.SendEmail(emailFormat);
        }
        catch (Exception e)
        {
            
        }


    }
    
    

    /// <summary>
    /// 发送邮件
    /// </summary>
    /// <param name="emailFormat">邮件的格式化内容</param>
    /// <returns>true 发送成功</returns>
    public static void SendEmail(EmailFormat emailFormat) {
        if (emailFormat == null)
        {
            return;
        }
 
        if (emailFormat.To.Count==0 || emailFormat.To == null) {
            return;
        }
 
        // 创建邮件信息，设置发件人
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(emailFormat.From);
 
        // 添加收件方（可以是列表）
        foreach (string item in emailFormat.To)
        {
            mail.To.Add(item);
 
        }
 
        // 设置邮件主题和内容
        mail.Subject = emailFormat.Subject;
        mail.Body = emailFormat.Body;
 
        // 设置邮件的附件（可以是多个）
        if (emailFormat.Attachments !=null && emailFormat.Attachments.Count >0) {
            foreach (string item in emailFormat.Attachments)
            {
 
                mail.Attachments.Add(new Attachment(item));
            }
        }
 
        // 绑定开通 Smtp 服务的邮箱 Credentials登陆SMTP服务器的身份验证
        SmtpClient smtpClient = new SmtpClient(emailFormat.SmtpClient);
        smtpClient.Credentials = new System.Net.NetworkCredential(emailFormat.From, emailFormat.AuthorizationCode) as System.Net.ICredentialsByHost;
        smtpClient.EnableSsl = true;
 
        // 设置相关回调
        System.Net.ServicePointManager.ServerCertificateValidationCallback =
            delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors
            )
            {
                return true;
            };
 
        // 发送邮件
        smtpClient.Send(mail);


        ;
    }

}

public class EmailFormat
{
    // 账号
    public string From { get; set; }
    public string AuthorizationCode { get; set; }

    public string SmtpClient { get; set; }

    // 邮件主题
    public string Subject { get; set; }

    // 邮件内容
    public string Body { get; set; }

    // 邮件附件
    public List<string> Attachments { get; set; }

    // 收件人
    public List<string> To { get; set; }

    public EmailFormat()
    {
        Attachments = new List<string>();
        To = new List<string>();
    }
}
