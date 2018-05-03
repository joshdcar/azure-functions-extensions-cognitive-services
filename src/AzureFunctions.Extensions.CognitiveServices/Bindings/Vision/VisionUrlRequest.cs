using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public class VisionUrlRequest
    {

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
