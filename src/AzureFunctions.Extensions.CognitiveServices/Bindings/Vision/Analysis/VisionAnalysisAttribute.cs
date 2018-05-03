using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class VisionAnalysisAttribute : VisionAttributeBase
    {

    }
}
