using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using Microsoft.Azure.WebJobs.Description;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings
{
    public abstract class VisionAttributeBase : Attribute
    {

        [AutoResolve(Default = "%VisionUrl%")]
        [Required(ErrorMessage = VisionExceptionMessages.SubscriptionUrlRequired)]
        public string Url { get; set; }

        [AutoResolve(Default = "%VisionKey%")]
        public string Key { get; set; }

        [AutoResolve()]
        public string SecureKey { get; set; }

        public bool AutoResize { get; set; }

        public string ImageUrl { get; set; }


    }
}
