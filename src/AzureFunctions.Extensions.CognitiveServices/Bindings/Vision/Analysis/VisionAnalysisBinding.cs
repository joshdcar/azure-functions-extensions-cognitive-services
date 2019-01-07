using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;


namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis
{
    [Extension("VisionAnalysis")]
    public class VisionAnalysisBinding : IExtensionConfigProvider, IVisionBinding
    {

        public ICognitiveServicesClient Client {get;set;}

        internal ILoggerFactory _loggerFactory;

        public VisionAnalysisBinding(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            var visionAnalysisRule = context.AddBindingRule<VisionAnalysisAttribute>();

            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionAnalysisModel>(GetVisionAnalysisModel);

            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.Url)
             .BindToInput<VisionAnalysisModel>(GetVisionAnalysisModel);

            visionAnalysisRule.When(nameof(VisionAnalysisAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionAnalysisClient>(attr => new VisionAnalysisClient(this, attr, _loggerFactory));

        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private VisionAnalysisModel GetVisionAnalysisModel(VisionAnalysisAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionAnalysisClient(this, attribute, _loggerFactory);

            VisionAnalysisRequest request = new VisionAnalysisRequest();

            if (attribute.ImageSource == ImageSource.BlobStorage)
            {
                var fileTask = StorageServices.GetFileBytes(attribute.BlobStoragePath, attribute.BlobStorageAccount);
                fileTask.Wait();

                request.ImageBytes = fileTask.Result;

            } else
            {
                request.ImageUrl = attribute.ImageUrl;
            }

            var result = client.AnalyzeAsync(request);
            result.Wait();

            return result.Result;

        }



    }
}
