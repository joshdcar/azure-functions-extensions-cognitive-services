using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Handwriting
{
    public class VisionHandwritingBinding : IExtensionConfigProvider, IVisionBinding
    {

        public ICognitiveServicesClient Client { get; set; }

        internal ILoggerFactory _loggerFactory;
        internal ILogger _log;


        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            _loggerFactory = context.Config.LoggerFactory ?? throw new ArgumentNullException("Logger Missing");

            var visionRule = context.AddBindingRule<VisionHandwritingAttribute>();

            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionHandwritingModel>(GetVisionHandwritingModel);

            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.Url)
             .BindToInput<VisionHandwritingModel>(GetVisionHandwritingModel);

            visionRule.When(nameof(VisionHandwritingAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionHandwritingClient>(attr => new VisionHandwritingClient(this, attr, _loggerFactory));

        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private VisionHandwritingModel GetVisionHandwritingModel(VisionHandwritingAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionHandwritingClient(this, attribute, _loggerFactory);

            VisionHandwritingRequest request = new VisionHandwritingRequest();

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

            var result = client.HandwritingAsync(request);
            result.Wait();

            return result.Result;

        }



    }
}
