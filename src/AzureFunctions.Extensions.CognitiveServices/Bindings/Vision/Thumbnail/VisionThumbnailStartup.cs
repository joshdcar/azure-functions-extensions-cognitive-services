using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(VisionThumbnailWebJobsStartup))]

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{

    public class VisionThumbnailWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddVisionThumbnail();
        }
    }


}

