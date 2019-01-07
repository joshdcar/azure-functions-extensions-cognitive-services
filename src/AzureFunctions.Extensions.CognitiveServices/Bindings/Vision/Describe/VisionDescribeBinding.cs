using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe
{

    [Extension("VisionDescribe")]
    public class VisionDescribeBinding : IExtensionConfigProvider, IVisionBinding
    {

        public ICognitiveServicesClient Client { get; set; }

        internal ILoggerFactory _loggerFactory;

        public VisionDescribeBinding(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

            var visionDescribeRule = context.AddBindingRule<VisionDescribeAttribute>();

            visionDescribeRule.When(nameof(VisionDescribeAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionDescribeModel>(GetVisionDescribeModel);

            visionDescribeRule.When(nameof(VisionDescribeAttribute.ImageSource), ImageSource.Url)
             .BindToInput<VisionDescribeModel>(GetVisionDescribeModel);

            visionDescribeRule.When(nameof(VisionDescribeAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionDescribeClient>(attr => new VisionDescribeClient(this, attr, _loggerFactory));


        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private VisionDescribeModel GetVisionDescribeModel(VisionDescribeAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionDescribeClient(this, attribute, _loggerFactory);

            VisionDescribeRequest request = new VisionDescribeRequest();

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

            var result = client.DescribeAsync(request);
            result.Wait();

            return result.Result;

        }
    }
}
