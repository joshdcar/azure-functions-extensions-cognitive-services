using Microsoft.Azure.WebJobs;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{
    public static class VisionDescribeExtension
    {
        public static IWebJobsBuilder AddVisionDescribe(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionDescribeBinding>();
            return builder;
        }

    }

}
