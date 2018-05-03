using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Config
{
    public static class VisionConfiguration
    {
        public static readonly int MaximumFileSize = 4;
        public static readonly int DefaultResizeQuality = 70;
    }

    public static class VisionExceptionMessages
    {
        public const string CognitiveServicesException = "The following exception was returned from Cognitive Services. Code: {0} Message: {1}";
        public const string FileMissing = "Image File Missing";
        public const string FileTooLarge = "Files must be {0} mb or smaller for the cognitive service vision API. Your file size is {1} bytes.";
        public const string FileTooLargeAfterResize = "Files must be {0} mb or smaller for the cognitive service vision API. After an autoresize attempt your file is still too large at {1} bytes.";
        public const string ImageSizeOutOfRange = "Must be between 1 and 1024. Recommended minimum of 50.";
        public const string WidthMissing = "Width is required.";
        public const string HeightMissing = "Height is required.";
        public const string SubscriptionUrlRequired = "The url for Cognitive Services endpoint is missing. This can be found in your Azure Subscription.";
        public const string SubscriptionKeyMissing = "The key for the Cognitive Services subscription is missing.This can be found in your Azure Subscription.";
        public const string KeyMissing = "The cognitives services key is missing. You must set the Key or SecureKey property.";
        public const string InvalidDomainName = "The provided domain option {0} is invalid. Domain must be Celebrity or Landmark. Use Domain constants for better accuracy.";
        public const string KeyvaultException = "An exception occured while attempting to obtain a keyvault value: {0}";
    }
}
