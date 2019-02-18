using Microsoft.Azure.WebJobs;
using System;


namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    public static class VisionAnalysisExtension
    {
        public static IWebJobsBuilder AddVisionAnalysis(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<VisionAnalysisBinding>();
            return builder;
        }

    }

}
