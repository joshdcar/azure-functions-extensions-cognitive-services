using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail
{
    public class VisionThumbnailBinding : IExtensionConfigProvider, IVisionBinding
    {

        public ICognitiveServicesClient Client { get; set; }

        internal ILoggerFactory _loggerFactory;
        internal ILogger _log;


        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            _loggerFactory = context.Config.LoggerFactory ?? throw new ArgumentNullException("Logger Missing");

            var visionRule = context.AddBindingRule<VisionThumbnailAttribute>();

            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<Byte[]>(GetVisionDescribeModel);

            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.Url)
                .BindToInput<Byte[]>(GetVisionDescribeModel);

            visionRule.When(nameof(VisionThumbnailAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionThumbnailClient>(attr => new VisionThumbnailClient(this, attr, _loggerFactory));


        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private Byte[] GetVisionDescribeModel(VisionThumbnailAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionThumbnailClient(this, attribute, _loggerFactory);

            VisionThumbnailRequest request = new VisionThumbnailRequest();

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

            var result = client.ThumbnailAsync(request);
            result.Wait();

            return result.Result;

        }
    }
}

