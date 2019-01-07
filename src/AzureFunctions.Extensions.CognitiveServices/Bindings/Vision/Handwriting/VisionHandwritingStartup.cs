using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(VisionHandwritingWebJobsStartup))]

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{

    public class VisionHandwritingWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddVisionHandwriting();
        }
    }


}