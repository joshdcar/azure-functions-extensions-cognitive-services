
using Microsoft.Azure.WebJobs;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public static class VisionOcrExtension
    {
        public static IWebJobsBuilder AddVisionOcr(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionOcrBinding>();
            return builder;
        }

    }

}
