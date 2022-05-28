using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Http
{
    public class HttpRequest
    {

        // unique identifier
        public int requestId;

        public async Task<string> GetResponseStatus(string BaseUri, HttpContent payload)
        {
            var httpClient = new HttpClient();

            var response = await httpClient.PostAsync(new Uri(BaseUri), payload);
            if (response.IsSuccessStatusCode)
                return "Success";
            else
                return "Fail";
        }

    }
}