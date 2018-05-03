using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public class VisionErrorModel
    {
        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }
}
