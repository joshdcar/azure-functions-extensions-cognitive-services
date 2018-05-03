using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public class VisionDescribeRequest : VisionRequestBase
    {
        public VisionDescribeRequest() { }

        public VisionDescribeRequest(Stream image) : base(image) { }

        public VisionDescribeRequest(byte[] image) : base(image) { }

        public VisionDescribeRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("maxCandidates")]
        public int MaxCandidates { get; set; } = 1;
    }
}
