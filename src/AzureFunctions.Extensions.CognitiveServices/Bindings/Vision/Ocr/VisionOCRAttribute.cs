using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionOcrAttribute : VisionAttributeBase
    {

        public bool? DetectOrientation { get; set; } 

    }
}
