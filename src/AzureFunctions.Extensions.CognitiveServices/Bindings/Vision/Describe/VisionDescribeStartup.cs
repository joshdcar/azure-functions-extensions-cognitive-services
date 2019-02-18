using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(VisionDescribeWebJobsStartup))]

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{

    public class VisionDescribeWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddVisionDescribe();
        }
    }


}