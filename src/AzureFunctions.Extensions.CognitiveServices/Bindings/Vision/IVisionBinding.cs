using AzureFunctions.Extensions.CognitiveServices.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunctions.Extensions.CognitiveServices.Bindings.Vision
{
    public interface IVisionBinding
    {
        ICognitiveServicesClient Client { get; set; }

    }
}
