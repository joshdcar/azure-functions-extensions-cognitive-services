using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using Microsoft.Azure.WebJobs;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    public static class VisionHandwritingExtension
    {
        public static IWebJobsBuilder AddVisionHandwriting(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionHandwritingBinding>();
            return builder;
        }

    }

}
