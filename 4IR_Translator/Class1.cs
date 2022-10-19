
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace _4IR_Translator
{
    public class SuccessfulRequestModel
    {
        public string original_text { get; set; }
        public string conversion_text { get; set; }
        public dynamic detail { get { return detail; } set { detail = null; } }

    }

    public class Translate : CodeActivity
    {
        static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        [RequiredArgument]
        [Category("Input")]
        [DisplayName("Source Language")]
        [Description("Language of sentence that is to be converted")]
        public InArgument<string> SrcLang { get; set; }

        [RequiredArgument]
        [Category("Input")]
        [DisplayName("Conversion Language")]
        [Description("Language sentence is to be converted to")]
        public InArgument<string> CnvrsnLang { get; set; }

        [RequiredArgument]
        [Category("Input")]
        [DisplayName("Sentence")]
        [Description("Text that is to be converted")]
        public InArgument<string> Sentence { get; set; }

        [RequiredArgument]
        [Category("Output")]
        [DisplayName("Response")]
        [Description("Response of API")]
        public OutArgument<string> Response { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            dynamic Result;
            SuccessfulRequestModel model;
            string srcLang = UppercaseFirst(SrcLang.Get(context));
            string cnvrsnLang = UppercaseFirst(CnvrsnLang.Get(context));
            string sentence = Sentence.Get(context);
            HttpStatusCode statusCode;

            //API Details
            string endPoint = "https://text-translation-fairseq-1.ai-sandbox.4th-ir.io/api/v1/sentence";
            string paramdEndPoint = endPoint + "?source_lang=" + srcLang + "&conversion_lang=" + cnvrsnLang;
            string PostData = "{" + String.Format("\"sentence\":\"{0}\"", sentence) + "}";

            WebRequest request = WebRequest.Create(paramdEndPoint);
            request.Method = "POST";
            request.ContentType = "application/json";

            //  try
            //  {
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(PostData);
                streamWriter.Flush();
                streamWriter.Close();

                var httpResponse = request.GetResponse();

                using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                   
                    statusCode = ((HttpWebResponse)httpResponse).StatusCode;
                    string text = reader.ReadToEnd();
                    Result = JsonSerializer.Deserialize<SuccessfulRequestModel>(text);
                }
            }

            switch (statusCode)
            {
                case (HttpStatusCode.OK):
                    Response.Set(context, Result.conversion_text);
                    break;

                case HttpStatusCode.BadRequest:
                    throw new Exception("Bad Request");
                    break;

                case HttpStatusCode.InternalServerError:
                    throw new Exception("Internal Server Error");
                    break;

                default:
                    Response.Set(context, "Didnt work");
                    break;
            }

        }
    }
}
