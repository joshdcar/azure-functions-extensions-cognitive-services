using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Services.Models
{
    public class ServiceResultModel
    {

        public int HttpStatusCode { get; set; }

        public string Contents { get; set; }

        public byte[] Binary { get; set; }

        public HttpResponseHeaders Headers { get; set; }

    }
}
