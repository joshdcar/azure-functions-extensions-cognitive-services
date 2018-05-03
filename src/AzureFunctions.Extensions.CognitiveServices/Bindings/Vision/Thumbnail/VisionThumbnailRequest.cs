using AzureFunctions.Extensions.CognitiveServices.Config;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    public class VisionThumbnailRequest : VisionRequestBase
    {

        public VisionThumbnailRequest() { }

        public VisionThumbnailRequest(Stream image) : base(image) { }

        public VisionThumbnailRequest(byte[] image) : base(image) { }

        public VisionThumbnailRequest(string imageUrl) : base(imageUrl) { }


        [AutoResolve()]
        [Required(ErrorMessage = VisionExceptionMessages.WidthMissing)]
        [Range(1, 1024, ErrorMessage = VisionExceptionMessages.ImageSizeOutOfRange)]
        public string Width { get; set; }

        [AutoResolve()]
        [Required(ErrorMessage = VisionExceptionMessages.HeightMissing)]
        [Range(1, 1024, ErrorMessage = VisionExceptionMessages.ImageSizeOutOfRange)]
        public string Height { get; set; }

        public bool SmartCropping { get; set; }
    }
}
