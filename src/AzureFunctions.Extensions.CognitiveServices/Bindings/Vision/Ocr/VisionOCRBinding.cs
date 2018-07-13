using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Ocr
{
    public class VisionOcrBinding : IExtensionConfigProvider, IVisionBinding
    {

        public ICognitiveServicesClient Client { get; set; }

        internal ILoggerFactory _loggerFactory;
        internal ILogger _log;


        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            _loggerFactory = context.Config.LoggerFactory ?? throw new ArgumentNullException("Logger Missing");

            var visionRule = context.AddBindingRule<VisionOcrAttribute>();

            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionOcrModel>(GetVisionOcrModel);

            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.Url)
             .BindToInput<VisionOcrModel>(GetVisionOcrModel);

            visionRule.When(nameof(VisionOcrAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionOcrClient>(attr => new VisionOcrClient(this, attr, _loggerFactory));

        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private VisionOcrModel GetVisionOcrModel(VisionOcrAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionOcrClient(this, attribute, _loggerFactory);

            VisionOcrRequest request = new VisionOcrRequest();

            if (attribute.ImageSource == ImageSource.BlobStorage)
            {
                var fileTask = StorageServices.GetFileBytes(attribute.BlobStoragePath, attribute.BlobStorageAccount);
                fileTask.Wait();

                request.ImageBytes = fileTask.Result;

            }
            else
            {
                request.ImageUrl = attribute.ImageUrl;
            }

            var result = client.OCRAsync(request);
            result.Wait();

            return result.Result;

        }



    }
}