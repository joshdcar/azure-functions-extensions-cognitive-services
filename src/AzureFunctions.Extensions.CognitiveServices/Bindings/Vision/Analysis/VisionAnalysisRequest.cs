using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    [Flags]
    public enum VisionAnalysisOptions
    {
        All = 0,
        Categories = 1,
        Tags = 2,
        Description = 4,
        Faces = 8,
        ImageType = 16,
        Color = 32,
        Adult = 64,
        Celebrities = 128,
        Landmarks = 256
    }

    public class VisionAnalysisRequest : VisionRequestBase
    {
        public VisionAnalysisRequest() { }

        public VisionAnalysisRequest(Stream image) : base(image) { }

        public VisionAnalysisRequest(byte[] image) : base(image) { }

        public VisionAnalysisRequest(string imageUrl) : base(imageUrl) { }


        [JsonProperty("options")]
        public VisionAnalysisOptions Options { get; set; } = VisionAnalysisOptions.All;


    }
}
