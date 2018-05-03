using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    public class Landmark
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }

    public class Result
    {

        [JsonProperty("landmarks")]
        public IList<Landmark> Landmarks { get; set; }
    }

    public class LandmarkMetadata
    {

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }
    }

    public class VisionDomainLandmarkModel
    {

        [JsonProperty("result")]
        public Result Result { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("metadata")]
        public LandmarkMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }




}
