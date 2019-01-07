using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(VisionAnalysisWebJobsStartup))]

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    public class VisionAnalysisWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddVisionAnalysis();
        }
    }


}