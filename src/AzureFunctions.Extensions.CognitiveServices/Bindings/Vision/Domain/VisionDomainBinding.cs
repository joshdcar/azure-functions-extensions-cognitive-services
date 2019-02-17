﻿using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Domain
{

    [Extension("VisionDomain")]
    public class VisionDomainBinding : IExtensionConfigProvider, IVisionBinding
    {
        internal ILoggerFactory _loggerFactory;

        public VisionDomainBinding(ILoggerFactory loggerFactory, ICognitiveServicesClient client)
        {
            _loggerFactory = loggerFactory;
            this.Client = client;
        }

        public ICognitiveServicesClient Client { get; set; }

        public void Initialize(ExtensionConfigContext context)
        {

            LoadClient();

    
            var visionDomainRule = context.AddBindingRule<VisionDomainAttribute>();

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionDomainLandmarkModel>(GetVisionLandmarkModel);

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Url)
                .BindToInput<VisionDomainLandmarkModel>(GetVisionLandmarkModel);

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.BlobStorage)
                .BindToInput<VisionDomainCelebrityModel>(GetVisionCelebrityModel);

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Url)
             .BindToInput<VisionDomainCelebrityModel>(GetVisionCelebrityModel);

            visionDomainRule.When(nameof(VisionDomainAttribute.ImageSource), ImageSource.Client)
                .BindToInput<VisionDomainClient>(attr => new VisionDomainClient(this, attr, _loggerFactory));

        }

        private void LoadClient()
        {
            if (Client == null)
            {
                Client = new CognitiveServicesClient(new RetryPolicy(), _loggerFactory);
            }
        }

        private VisionDomainCelebrityModel GetVisionCelebrityModel(VisionDomainAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionDomainClient(this, attribute, _loggerFactory);
            var request = BuildRequest(attribute);
           
            var result = client.AnalyzeCelebrityAsync(request);
            result.Wait();

            return result.Result;

        }

        private VisionDomainLandmarkModel GetVisionLandmarkModel(VisionDomainAttribute attribute)
        {

            if (attribute.ImageSource == Bindings.ImageSource.Client)
            {
                throw new ArgumentException($"ImageSource of Client does not support binding to vision models. Use Url or BlobStorage instead. ");
            }

            attribute.Validate();

            var client = new VisionDomainClient(this, attribute, _loggerFactory);
            var request = BuildRequest(attribute);

            var result = client.AnalyzeLandmarkAsync(request);
            result.Wait();

            return result.Result;

        }

        private VisionDomainRequest BuildRequest(VisionDomainAttribute attribute)
        {
            VisionDomainRequest request = new VisionDomainRequest();

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

            return request;

        }
    }
}
