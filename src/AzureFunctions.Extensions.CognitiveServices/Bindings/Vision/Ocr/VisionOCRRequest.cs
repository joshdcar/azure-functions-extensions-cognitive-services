using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public class VisionOcrRequest : VisionRequestBase
    {

        public VisionOcrRequest() { }

        public VisionOcrRequest(Stream image) : base(image) { }

        public VisionOcrRequest(byte[] image) : base(image) { }

        public VisionOcrRequest(string imageUrl) : base(imageUrl) { }

        [JsonProperty("detectOrientation")]
        public bool DetectOrientation { get; set; } = false;

    }
}
