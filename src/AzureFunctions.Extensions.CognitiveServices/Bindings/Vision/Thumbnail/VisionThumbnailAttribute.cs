using AzureFunctions.Extensions.CognitiveServices.Config;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionThumbnailAttribute : VisionAttributeBase
    {
        [AutoResolve()]
        public string Width { get; set; }

        [AutoResolve()]
        public string Height { get; set; }

        public bool SmartCropping { get; set; }
    }
}

