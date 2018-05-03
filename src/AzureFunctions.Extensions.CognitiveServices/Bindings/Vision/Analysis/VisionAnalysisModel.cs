using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    public class VisionAnalysisModel
    {
        [JsonProperty(PropertyName = "categories")]
        public IEnumerable<VisionCategory> Categories { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<VisionTag> Tags { get; set; }

        [JsonProperty(PropertyName = "description")]
        public VisionDescription Description { get; set; }

        [JsonProperty(PropertyName = "faces")]
        public IEnumerable<VisionFace> Faces { get; set; }

        [JsonProperty(PropertyName = "color")]
        public VisionColor Color { get; set; }

        [JsonProperty(PropertyName = "imageType")]
        public VisionImageType ImageType { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public VisionMetadata Metadata { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

    public class VisionCategory
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }

        [JsonProperty(PropertyName = "detail")]
        public VisionDetail Detail { get; set; }
    }

    public class VisionDetail
    {
        [JsonProperty(PropertyName = "landmarks")]
        public IEnumerable<VisionLandmark> Landmarks { get; set; }

        [JsonProperty(PropertyName = "celebrities")]
        public IEnumerable<VisionCelebrity> Celebrities { get; set; }
    }

    public class VisionLandmark
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }
    }

    public class VisionCelebrity
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }

        [JsonProperty(PropertyName = "faceRectangle")]
        public VisionFaceRectangle FaceRectangle { get; set; }
    }

    public class VisionTag
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }
    }

    public class VisionDescription
    {
        [JsonProperty(PropertyName = "tags")]
        public IEnumerable<string> Tags { get; set; }

        [JsonProperty(PropertyName = "captions")]
        public IEnumerable<VisionCaption> Captions { get; set; }

    }

    public class VisionCaption
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "confidence")]
        public double Confidence { get; set; }
    }

    public class VisionFace
    {
        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string Gender { get; set; }

        [JsonProperty(PropertyName = "faceRectangle")]
        public VisionFaceRectangle FaceRectangle { get; set; }
    }

    public class VisionFaceRectangle
    {
        [JsonProperty(PropertyName = "top")]
        public int Top { get; set; }

        [JsonProperty(PropertyName = "left")]
        public int Left { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "length")]
        public int Length { get; set; }
    }

    public class VisionColor
    {
        [JsonProperty(PropertyName = "dominantColorForeground")]
        public string DominantColorForeground { get; set; }

        [JsonProperty(PropertyName = "dominantColorBackground")]
        public string DominantColorBackground { get; set; }

        [JsonProperty(PropertyName = "dominantColors")]
        public IEnumerable<string> DominantColors { get; set; }

        [JsonProperty(PropertyName = "accentColor")]
        public string AccentColor { get; set; }

        [JsonProperty(PropertyName = "isBwImg")]
        public bool IsBwImg { get; set; }
    }

    public class VisionImageType
    {
        [JsonProperty(PropertyName = "clipArtType")]
        public int ClipArtType { get; set; }

        [JsonProperty(PropertyName = "lineDrawingType")]
        public int LineDrawingType { get; set; }
    }

    public class VisionMetadata
    {
        [JsonProperty(PropertyName = "height")]
        public int Height { get; set; }

        [JsonProperty(PropertyName = "width")]
        public int Width { get; set; }

        [JsonProperty(PropertyName = "format")]
        public string Format { get; set; }
    }
}
