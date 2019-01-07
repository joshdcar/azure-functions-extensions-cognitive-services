using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using Microsoft.Azure.WebJobs;
using System;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    public static class VisionThumbnailExtension
    {
        public static IWebJobsBuilder AddVisionThumbnail(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionThumbnailBinding>();
            return builder;
        }

    }

}
