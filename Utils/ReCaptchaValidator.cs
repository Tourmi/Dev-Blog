using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Dev_Blog.Utils
{
    public static class ReCaptchaValidator
    {
        /// <summary>
        /// Verifies the given response
        /// </summary>
        /// <param name="secretKey">The secret key to use for the API</param>
        /// <param name="gRecaptchaResponse">The reponse to verify</param>
        /// <returns></returns>
        public static bool ReCaptchaPassed(string secretKey, string gRecaptchaResponse)
        {
            HttpClient httpClient = new HttpClient();

            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={gRecaptchaResponse}").Result;

            if (res.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);

            if (JSONdata.success != "true")
            {
                return false;
            }

            return true;
        }
    }
}
