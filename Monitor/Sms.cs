using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

namespace Monitor
{
        public class Sms
        {
                public Sms()
                {
                        XmlElement root = Tool.GetXML(@"Config/SMS.xml");
                        Url = root.SelectSingleNode("//URL").InnerText;
                        Appkey = root.SelectSingleNode("//APPKEY").InnerText;
                        Secret = root.SelectSingleNode("//SECRET").InnerText;
                        SmsFreeSignName = root.SelectSingleNode("//SMSFREESIGNNAME").InnerText;
                        //RecNum = root.SelectSingleNode("//RECNUM").InnerText;
                        RecNum = Tool.GetConfig("Telephones");
                        SmsTemplateCode = root.SelectSingleNode("//SMSTEMPLATECODE").InnerText;
                }
                public string Url
                {
                        get;
                        set;
                }
                public string Appkey
                {
                        get;
                        set;
                }
                public string Secret
                {
                        get;
                        set;
                }
                public string SmsFreeSignName
                {
                        get;
                        set;
                }
                public string RecNum
                {
                        get;
                        set;
                }
                public string SmsTemplateCode
                {
                        get;
                        set;
                }

                public void SendSms(string address, string info)
                {
                        try
                        {
                                ITopClient client = new DefaultTopClient(Url, Appkey, Secret);
                                AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
                                req.Extend = "";
                                req.SmsType = "normal";
                                req.SmsFreeSignName = SmsFreeSignName;
                                string msg = string.Format("time:'{0}',address:'{1}',info:'{2}'", DateTime.Now.ToString("HH:mm:ss"), address, info);
                                req.SmsParam = "{" + msg + "}";
                                req.RecNum = RecNum;
                                req.SmsTemplateCode = SmsTemplateCode;
                                AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);
                        }
                        catch
                        {
                                MsgBox.Show("请检查短信报警设置情况！", "发送短信失败", MsgBox.Buttons.OK, MsgBox.Icon.Error, MsgBox.AnimateStyle.FadeIn);
                        }
                }
        }
}
