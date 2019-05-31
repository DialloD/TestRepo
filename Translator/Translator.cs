using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Translator
{
    internal class Translator
    {
        // This sample uses the Cognitive Services subscription key for all services. To learn more about
        // authentication options, see: https://docs.microsoft.com/azure/cognitive-services/authentication.
        private const string COGNITIVE_SERVICES_KEY = "YOUR_COG_SERVICES_KEY";
        // Endpoints for Translator Text and Bing Spell Check
        public static readonly string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";
        private const string BING_SPELL_CHECK_API_ENDPOINT = "https://westus.api.cognitive.microsoft.com/bing/v7.0/spellcheck/";
        // An array of language codes
        private string[] languageCodes;

        // Dictionary to map language codes from friendly name (sorted case-insensitively on language name)
        private SortedDictionary<string, string> languageCodesAndTitles =
            new SortedDictionary<string, string>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        // ***** GET TRANSLATABLE LANGUAGE CODES
        public void GetLanguagesForTranslate()
        {
            // Send request to get supported language codes
            string uri = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "languages") + "&scope=translation";
            WebRequest WebRequest = WebRequest.Create(uri);
            WebRequest.Headers.Add("Accept-Language", "en");
            WebResponse response = null;
            // Read and parse the JSON response
            response = WebRequest.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), UnicodeEncoding.UTF8))
            {
                Dictionary<string, Dictionary<string, Dictionary<string, string>>> result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(reader.ReadToEnd());
                Dictionary<string, Dictionary<string, string>> languages = result["translation"];

                languageCodes = languages.Keys.ToArray();
                foreach (KeyValuePair<string, Dictionary<string, string>> kv in languages)
                {
                    languageCodesAndTitles.Add(kv.Value["name"], kv.Key);
                }
            }
        }


        // ***** DETECT LANGUAGE OF TEXT TO BE TRANSLATED
        public string DetectLanguage(string text)
        {
            string detectUri = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "detect");

            // Create request to Detect languages with Translator Text
            HttpWebRequest detectLanguageWebRequest = (HttpWebRequest)WebRequest.Create(detectUri);
            detectLanguageWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
            detectLanguageWebRequest.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
            detectLanguageWebRequest.ContentType = "app/json; charset=utf-8";
            detectLanguageWebRequest.Method = "POST";

            // Send request
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonText = serializer.Serialize(text);

            string body = "[{ \"Text\": " + jsonText + " }]";
            byte[] data = Encoding.UTF8.GetBytes(body);

            detectLanguageWebRequest.ContentLength = data.Length;

            using (Stream requestStream = detectLanguageWebRequest.GetRequestStream())
                requestStream.Write(data, 0, data.Length);

            HttpWebResponse response = (HttpWebResponse)detectLanguageWebRequest.GetResponse();

            // Read and parse JSON response
            Stream responseStream = response.GetResponseStream();
            string jsonString = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
            dynamic jsonResponse = serializer.DeserializeObject(jsonString);

            // Fish out the detected language code
            dynamic languageInfo = jsonResponse[0];
            if (languageInfo["score"] > (decimal)0.5)
            {
                Console.WriteLine(languageInfo["language"]);
                return languageInfo["language"];
            }
            else
                return "Unable to confidently detect input language.";
        }
        // NOTE:
        // In the following sections, we'll add code below this.


    }
}
