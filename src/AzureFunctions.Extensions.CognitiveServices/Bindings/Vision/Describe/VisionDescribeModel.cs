using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public class VisionDescribeModel
    {

        [JsonProperty(PropertyName = "description")]
        public VisionDescribeDescription Description { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public VisionDescribeMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class VisionDescribeDescription
    {
        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty(PropertyName = "captions")]
        public IEnumerable<VisionDescribeCaption> Captions { get; set; }
    }

    public class VisionDescribeCaption
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }
    }

    public class VisionDescribeMetadata
    {
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "format")]
        public string Format { get; set; }
    }
}
