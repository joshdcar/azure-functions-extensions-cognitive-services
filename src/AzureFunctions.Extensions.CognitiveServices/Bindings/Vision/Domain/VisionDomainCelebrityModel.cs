using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    public class FaceRectangle
    {

        [JsonProperty("top")]
        public int Top { get; set; }

        [JsonProperty("left")]
        public int Left { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class Celebrity
    {

        [JsonProperty("faceRectangle")]
        public FaceRectangle FaceRectangle { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class CelebrityResult
    {

        [JsonProperty("celebrities")]
        public IList<Celebrity> Celebrities { get; set; }
    }

    public class CelebrityMetadata
    {

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public class VisionDomainCelebrityModel
    {

        [JsonProperty("result")]
        public CelebrityResult Result { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public CelebrityMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}




