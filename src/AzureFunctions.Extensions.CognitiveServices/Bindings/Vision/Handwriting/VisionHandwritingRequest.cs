using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    public class VisionHandwritingRequest : VisionRequestBase
    {

        public VisionHandwritingRequest() { }

        public VisionHandwritingRequest(Stream image) : base(image) { }

        public VisionHandwritingRequest(byte[] image) : base(image) { }

        public VisionHandwritingRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("handwriting")]
        public bool Handwriting { get; set; } = true;

    }


    
}
