using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
 
    public class Word
    {

        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Line
    {

        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("words")]
        public IList<Word> Words { get; set; }
    }

    public class Region
    {

        [JsonProperty("boundingBox")]
        public string BoundingBox { get; set; }

        [JsonProperty("lines")]
        public IList<Line> Lines { get; set; }
    }

    public class VisionOcrModel
    {

        [JsonProperty("textAngle")]
        public double TextAngle { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("regions")]
        public IList<Region> Regions { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
