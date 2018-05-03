using AzureFunctions.Extensions.CognitiveServices.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public abstract class VisionRequestBase
    {
        public VisionRequestBase() { }

        public VisionRequestBase(Stream image)
        {
            this.ImageStream = image;
        }

        public VisionRequestBase(byte[] image)
        {
            this.ImageBytes = image;
        }
        public VisionRequestBase(string imageUrl)
        {
            this.ImageUrl = ImageUrl;
        }

        public Byte[] ImageBytes { get; set; }

        public Stream ImageStream
        {
            set
            {
                using (BinaryReader reader = new BinaryReader(value))
                {
                    this.ImageBytes = reader.ReadBytes((int)value.Length);
                }
            }
        }

        [JsonProperty("autoResizePhoto")]
        public bool AutoResize { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("secureKey")]
        public string SecureKey { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; } = "en";

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        public bool Oversized
        {
            get
            {
                var maxFileSize = VisionConfiguration.MaximumFileSize * 1024f * 1024f;

                if (ImageBytes.Length > maxFileSize)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsUrlImageSource
        {
            get
            {
                if (ImageBytes == null || ImageBytes.Length == 0)
                {

                    bool validUrl = Uri.TryCreate(ImageUrl, UriKind.Absolute, out Uri uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                    if (validUrl)
                    {
                        return true;
                    }

                }

                return false;
            }
        }

    }
}
