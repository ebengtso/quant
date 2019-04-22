using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Algorithm;
using RestSharp;

namespace AlgorithmUtils.Utilities
{
    public class Utils
    {
        public static string emailAccount ="";
        public static string emailPassword ="";
        public static string SlackURL = "";

        public static readonly long Identifier = (long)DateTime.Now.ToUniversalTime().Subtract(
    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    ).TotalMilliseconds;
        public static readonly string ID = "AlgoLucy";

        public static int ToUnixTimestamp(DateTime dateTime)
        {
            return (int)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                     new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;

        }
        public static string ToJSONString(object o)
        {
            return JObject.FromObject(o).ToString(Formatting.None);
        }

        public static int[] shiftRight(int[] arr)
        {
            int[] value = new int[arr.Length];

            for (int i = 1; i < arr.Length; i++)
            {
                value[i] = arr[i - 1];
            }

            value[0] = arr[value.Length - 1];

            return value;
        }

        public static void SaveFile(string filename, String content)
        {
            System.IO.File.WriteAllText(filename, content);
        }

        public static void WriteToFile(string filename, String content, bool appendExists = true)
        {
            TextWriter tw = new StreamWriter(filename, appendExists ? File.Exists(filename) : false);
            tw.WriteLine(content);
            tw.Flush();
            tw.Close();
        }

        public static void WriteToFile(string directory, string filename, String content, bool appendExists = true)
        {
            System.IO.Directory.CreateDirectory(directory);
            TextWriter tw = new StreamWriter(directory+'/'+filename, appendExists ? File.Exists(directory + '/' + filename) : false);
            tw.WriteLine(content);
            tw.Flush();
            tw.Close();
        }
        public static void SendMail(QCAlgorithm algorithm, string AlgorithmId, string title, string body, string url, string id)
        {
            if (!algorithm.LiveMode)
            {
                return;
            }
            try
            {
                SendSlack(AlgorithmId, title, body);
                // Command line argument must the the SMTP host.
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(Utils.emailAccount, Utils.emailPassword);

                MailMessage mm = new MailMessage(Utils.emailAccount, Utils.emailAccount, string.Format("{0}-{1} {2}", AlgorithmId, Identifier,title), body);
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

               
                try
                {
                    WebClient webClient = new WebClient();
                    byte[] data = webClient.DownloadData(url);


                    System.IO.MemoryStream ms = new System.IO.MemoryStream(data);
                    System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Image.Jpeg);
                    System.Net.Mail.Attachment attach = new System.Net.Mail.Attachment(ms, ct);
                    attach.ContentDisposition.FileName = string.Format("{0}.jpg", id);
                    mm.Attachments.Add(attach);
                    client.Send(mm);
                    ms.Close();
                }
                catch (Exception) { client.Send(mm); }
                 
            }
            catch (Exception) { }
        }
        public static void SendMail(QCAlgorithm algorithm, string AlgorithmId, string title, string body, bool force = false)
        {
            if (!algorithm.LiveMode && !force)
            {
                return;
            }
            try
            {
                SendSlack(AlgorithmId, title, body);

                // Command line argument must the the SMTP host.
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(Utils.emailAccount, Utils.emailPassword);

                MailMessage mm = new MailMessage(Utils.emailAccount, Utils.emailAccount, string.Format("{0}-{1} {2}", AlgorithmId, Identifier, title), body);
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                client.Send(mm);
            }
            catch (Exception) { }

        }


        public static void SendSlack(string AlgorithmId, string title, string body)
        {

            RestClient client = new RestClient
            {
                Timeout = 120000,
                BaseUrl = new Uri(string.Format(SlackURL))
            };
            JObject obj = new JObject();
            obj.Add("text", string.Format("{0}-{1} {2} {3}", AlgorithmId, Identifier, title, body));


            var request = new RestRequest
            {
                Method = RestSharp.Method.POST
            };
            request.AddParameter("application/json", obj.ToString(), ParameterType.RequestBody);
            request.AddHeader("content-type", "application/json");
            client.Execute(request);
        }
    }


}
