using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{

    public class Word
    {

        [JsonProperty("boundingBox")]
        public IList<int> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public class Line
    {

        [JsonProperty("boundingBox")]
        public IList<int> BoundingBox { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("words")]
        public IList<Word> Words { get; set; }
    }

    public class RecognitionResult
    {

        [JsonProperty("lines")]
        public IList<Line> Lines { get; set; }
    }

    public class VisionHandwritingModel
    {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("succeeded")]
        public bool Succeeded { get; set; }

        [JsonProperty("failed")]
        public bool Failed { get; set; }

        [JsonProperty("finished")]
        public bool Finished { get; set; }

        [JsonProperty("recognitionResult")]
        public RecognitionResult RecognitionResult { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
