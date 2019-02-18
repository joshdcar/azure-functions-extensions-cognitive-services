using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(VisionOcrWebJobsStartup))]

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{

    public class VisionOcrWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddVisionOcr();
        }
    }


}