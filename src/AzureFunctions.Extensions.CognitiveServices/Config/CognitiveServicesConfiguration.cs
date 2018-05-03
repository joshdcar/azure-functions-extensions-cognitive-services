using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Config
{
    public class CognitiveServicesConfiguration : IExtensionConfigProvider
    {
        internal ICognitiveServicesClient Client { get; set; }

        internal ILoggerFactory _loggerFactory;
        internal ILogger _log;

        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            _loggerFactory = context.Config.LoggerFactory ?? throw new ArgumentNullException("Logger Missing");

            //Vision Analysis Bindings
            var analysisBinding = context.AddBindingRule<VisionAnalysisAttribute>();

            analysisBinding.BindToInput<VisionAnalysisClient>(attr => new VisionAnalysisClient(this, attr, _loggerFactory));

                
            context.AddBindingRule<VisionDescribeAttribute>()
                .BindToInput<VisionDescribeClient>(attr => new VisionDescribeClient(this, attr, _loggerFactory));

            context.AddBindingRule<VisionThumbnailAttribute>()
               .BindToInput<VisionThumbnailClient>(attr => new VisionThumbnailClient(this, attr, _loggerFactory));

            context.AddBindingRule<VisionOcrAttribute>()
              .BindToInput<VisionOcrClient>(attr => new VisionOcrClient(this, attr, _loggerFactory));

            context.AddBindingRule<VisionHandwritingAttribute>()
              .BindToInput<VisionHandwritingClient>(attr => new VisionHandwritingClient(this, attr, _loggerFactory));

            context.AddBindingRule<VisionDomainAttribute>()
              .BindToInput<VisionDomainClient>(attr => new VisionDomainClient(this, attr, _loggerFactory));

        }

        private void LoadClient()
        {
            if(Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(),_loggerFactory);
            }
        }
    }
}
