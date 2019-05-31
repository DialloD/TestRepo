using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Translator
{
    internal class Program
    {
        // This sample uses the Cognitive Services subscription key for all services. To learn more about
        // authentication options, see: https://docs.microsoft.com/azure/cognitive-services/authentication.
        private const string COGNITIVE_SERVICES_KEY = "YOUR_COG_SERVICES_KEY";
        // Endpoints for Translator Text and Bing Spell Check
        public static readonly string TEXT_TRANSLATION_API_ENDPOINT = "https://api.cognitive.microsofttranslator.com/{0}?api-version=3.0";
        private const string BING_SPELL_CHECK_API_ENDPOINT = "https://westus.api.cognitive.microsoft.com/bing/v7.0/spellcheck/";
        // An array of language codes
        private readonly string[] languageCodes;

        // Dictionary to map language codes from friendly name (sorted case-insensitively on language name)
        private readonly SortedDictionary<string, string> languageCodesAndTitles =
            new SortedDictionary<string, string>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        private static void Main(string[] args)
        {
            string message = "This is some html text to <strong>translate</strong>!";
            string targetLanguage = "fr";
            string sourceLanguage = null; // automatically detected
            Google.Cloud.Translation.V2.TranslationClient client = Google.Cloud.Translation.V2.TranslationClient.Create();
            Google.Cloud.Translation.V2.TranslationResult response = client.TranslateHtml(message, targetLanguage, sourceLanguage);
            Console.WriteLine(response.TranslatedText);
            //GetTranslatorAsync().Wait();
        }

        public static async Task<string> Translate()
        {
            // send HTTP request to perform the translation
            string endpoint = string.Format(TEXT_TRANSLATION_API_ENDPOINT, "translate");
            string uri = string.Format(endpoint + "&from={0}&to={1}", "fr", "en");

            object[] body = new object[] { new { Text = "test traduction" } };
            string requestBody = JsonConvert.SerializeObject(body);

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "app/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", COGNITIVE_SERVICES_KEY);
                request.Headers.Add("Ocp-Apim-Subscription-Region", "westus");
                request.Headers.Add("X-ClientTraceId", Guid.NewGuid().ToString());

                HttpResponseMessage response = await client.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                List<Dictionary<string, List<Dictionary<string, string>>>> result = JsonConvert.DeserializeObject<List<Dictionary<string, List<Dictionary<string, string>>>>>(responseBody);
                string translation = result[0]["translations"][0]["text"];

                // Update the translation field
                //Console.WriteLine(translation);

                return translation;
            }
        }

        private static async Task GetTranslatorAsync() //method added by me.
        {
            string x = await Translate();
        }
    }
}
