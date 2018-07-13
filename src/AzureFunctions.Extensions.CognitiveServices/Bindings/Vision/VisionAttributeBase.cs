using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings
{

    /// <summary>
    /// Determines the source of the image being analyzed. Each Image Source
    /// has varied required properties.
    /// </summary>
    public enum ImageSource
    {
        /// <summary>
        /// Image source is a publically accessible url
        /// </summary>
        Url,
        /// <summary>
        /// Image source is from Blob Storage
        /// </summary>
        BlobStorage,
        /// <summary>
        /// Image source is specified within the client binding
        /// </summary>
        Client
    }

    public abstract class VisionAttributeBase : Attribute
    {

        /// <summary>
        /// Vision Service URL. Defaults to an appSettings of VisionUrl
        /// </summary>
        [AppSetting(Default = "VisionUrl")]
        public string VisionUrl { get; set; }

        /// <summary>
        /// Authentication Key for Vision Service. Defaults to an appsettings 
        /// of VisionKey
        /// </summary>
        [AppSetting(Default = "VisionKey")]
        public string VisionKey { get; set; }

        [AppSetting()]
        public string SecureKey { get; set; }

        /// <summary>
        /// The source of the image being analyzed.
        /// </summary>
        public ImageSource ImageSource { get; set; } = ImageSource.Client;

        /// <summary>
        /// Whether the image being analyzed will automatically
        /// downscale and resize to achieve the cognitive services
        /// file size limit.
        /// </summary>
        public bool AutoResize { get; set; } = true;

        /// <summary>
        /// Vision Service URL. Defaults to a appsetting of VisionStorage
        /// </summary>
        [AppSetting(Default = "StorageAccount")]
        public string BlobStorageAccount { get; set; }

        /// <summary>
        /// Path to the file being analyzed in BlobStorage. May be set
        /// at runtime with dynamic parameters
        /// </summary>
        [AutoResolve()]
        public string BlobStoragePath { get; set; }

        /// <summary>
        /// A Url to the image being analyzed. Must be accessible without authentication
        /// </summary>
        [AutoResolve()]
        public string ImageUrl { get; set; }


        internal void Validate()
        {

            if (string.IsNullOrEmpty(VisionUrl))
            {
                throw new ArgumentException($"A value for VisionUrl must be provided for a VisionAttribute.");
            }

            if (string.IsNullOrEmpty(VisionKey))
            {
                throw new ArgumentException($"A value for VisionKey must be provided for a VisionAttribute.");
            }

            switch (ImageSource)
            {
                case ImageSource.Client:
                    break;

                case ImageSource.BlobStorage:

                    if (string.IsNullOrEmpty(BlobStorageAccount))
                    {
                        throw new ArgumentException($"A value for BlobStorageConnection must be provided for an image source of BlobStorage");
                    }

                    if (string.IsNullOrEmpty(BlobStoragePath))
                    {
                        throw new ArgumentException($"A value for BlobStoragePath must be provided for an image source of BlobStorage");
                    }

                    break;

                case ImageSource.Url:

                    if (string.IsNullOrEmpty(ImageUrl))
                    {
                        throw new ArgumentException($"A value for ImageUrl must be provided for an image source of BlobStorage");
                    }

                    break;
            }

        }

    }
}

