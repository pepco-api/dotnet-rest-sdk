using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using System.Web;
using System.Xml;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace dotnet_rest_sdk
{
    public class Pasargad
    {
        public static string PrivateKey;
        const string URL_GET_TOKEN = "https://pep.shaparak.ir/Api/v1/Payment/GetToken";
        const string URL_PAYMENT_GATEWAY = "https://pep.shaparak.ir/payment.aspx";
        const string URL_CHECK_TRANSACTION = "https://pep.shaparak.ir/Api/v1/Payment/CheckTransactionResult";
        const string URL_VERIFY_PAYMENT = "https://pep.shaparak.ir/Api/v1/Payment/VerifyPayment";
        const string URL_REFUND = "https://pep.shaparak.ir/Api/v1/Payment/RefundPayment";

        public string MerchantCode, TerminalCode, RedirectAddress, XmlFile, Action;
        public Pasargad(string MerchantCode, string TerminalCode, string RedirectAddress, string XmlFile, string Action = "1003")
        {
            this.MerchantCode = MerchantCode;
            this.TerminalCode = TerminalCode;
            this.RedirectAddress = RedirectAddress;
            this.XmlFile = XmlFile;
            this.Action = Action;
            if (string.IsNullOrEmpty(PrivateKey))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(XmlFile);
                    PrivateKey = doc.OuterXml;
                }
                catch(Exception EX)
                {
                  
                }
            }
        }
        public string InvoiceNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string Amount { get; set; }
        public string TransactionReferenceId { get; set; }
        public string Timestamp { get { return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); } }

        public string redirect()
        {
            object obj = new { MerchantCode = MerchantCode, TerminalCode = TerminalCode, InvoiceNumber = InvoiceNumber, InvoiceDate = InvoiceDate, Amount = Amount, Timestamp = Timestamp, RedirectAddress = RedirectAddress, Action = Action };
            string content = mainMethod(URL_GET_TOKEN, obj);

            if (content.Contains("Token"))
            {
                string[] token = content.Split((new Char[] { ':', '"', '\n' }), ',');
                return URL_PAYMENT_GATEWAY + "?n=" + token[4];
            }
            else
                return content;
        }
        public JObject checkTransaction()
        {
            object obj = new { TransactionReferenceId = TransactionReferenceId, MerchantCode = MerchantCode, TerminalCode = TerminalCode, InvoiceNumber = InvoiceNumber, InvoiceDate = InvoiceDate };
            string str = mainMethod(URL_CHECK_TRANSACTION, obj);
            JObject json = JObject.Parse(str);
            return json;
        }
        public JObject verifyPayment()
        {
            object obj = new { MerchantCode = MerchantCode, TerminalCode = TerminalCode, InvoiceNumber = InvoiceNumber, InvoiceDate = InvoiceDate, Amount = Amount, Timestamp = Timestamp };
            string str = mainMethod(URL_VERIFY_PAYMENT, obj);
            JObject json = JObject.Parse(str);
            return json;
        }
        public JObject refundPayment()
        {
            object obj = new { MerchantCode = MerchantCode, TerminalCode = TerminalCode, InvoiceNumber = InvoiceNumber, InvoiceDate = InvoiceDate, Amount = Amount, Timestamp = Timestamp };
            string str = mainMethod(URL_REFUND, obj);
            JObject json = JObject.Parse(str);
            return json;
        }
        public string getSign(string inputData)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(PrivateKey);
            byte[] signMain = rsa.SignData(Encoding.UTF8.GetBytes(inputData), new SHA1CryptoServiceProvider());
            string sign = Convert.ToBase64String(signMain);
            return sign;
        }
        private string mainMethod(string baseUrl, object inputForSign)
        {
            try
            {
                RestClient client = new RestClient(baseUrl);
                RestRequest request = new RestRequest(Method.POST);
                object obj = inputForSign;
                string output = JsonConvert.SerializeObject(obj);
                string sign = getSign(output);
                request.AddHeader("Sign", sign);
                request.AddJsonBody(obj);
                var response = client.Execute(request);
                string content = response.Content;
                return content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
