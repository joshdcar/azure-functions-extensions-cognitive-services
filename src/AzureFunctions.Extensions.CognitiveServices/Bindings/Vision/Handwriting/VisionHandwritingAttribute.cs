using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]

    public class VisionHandwritingAttribute : VisionAttributeBase
    {

        public bool? Handwriting { get; set; } = true;
    }
}
