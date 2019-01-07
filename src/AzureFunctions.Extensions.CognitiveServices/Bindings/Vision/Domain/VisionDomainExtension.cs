using Microsoft.Azure.WebJobs;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{
    public static class VisionDomainExtension
    {
        public static IWebJobsBuilder AddVisionDomain(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionDomainBinding>();
            return builder;
        }

    }

}
