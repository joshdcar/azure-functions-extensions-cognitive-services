 # <img align="left" height="100" src="https://raw.githubusercontent.com/joshdcar/azure-functions-extensions-cognitive-services/master/logo.png"> &nbsp;  Azure Function Extensions for Cognitive Services (Preview)

Azure Function Extensions for Cognitive Services is a set of custom Azure Function bindings for Azure's Cognitive Services. 

### Features:

 - Supports a subset of the Computer Vision API including Image Analysis, Optical Character Recognition (OCR), Handwriting, Celebrities & Landmarks, and Thumbnail generation.
 - Cognitive Service API calls with a couple lines of code
 - Support for automatic retry policies for throttled requests
 - Support for automatic image resize for files larger then 4mb (Cognitive Services Limit) ***
 - Support for Key Vault storage of Cognitive Services Keys
 - Exception, Warning, and request time metrics logging

*** Automatic resize is currently disabled due to binding conflicts with Azure Functions 2.0 and dependencies of the ImageSharp library used for resizing. We hope to resolve this prior to GA.

Added support for the remainder of the Vision offerings and additional support for Language and Knowledge will be coming very soon. More details on the various Cognitive Services offerings can be found at [https://azure.microsoft.com/en-us/services/cognitive-services/](https://azure.microsoft.com/en-us/services/cognitive-services/)

### Getting Started Video

[![Alt text](https://img.youtube.com/vi/GlCuPuj0Nk4/0.jpg)](https://www.youtube.com/watch?v=GlCuPuj0Nk4)

### Current Version

Latest Version: 1.0.0-preview1 

Current Azure Function Dependency:  Microsoft.Azure.Webjobs.Extensions (3.0.0-beta5)

**Warning**: There are a lot of previews at play with these bindings. The Bindings themselves are in preview. Azure Functions 2.0 is in preview. Some Cognitive Services are in preview.  ImageSharp, the library used for native .net core image resizing is also in preview.   

These bindings currently **only support Azure Functions 2.0** . There are no current plans for 1.x support. 

## QuickStart

### Prerequisites
You must have a valid Azure Cognitive Services Vision License Key and Url. You can obtain one with a Azure Subscription. If you do not have a subscription you can sign up for a free trial at [https://azure.microsoft.com/en-us/try/cognitive-services/](https://azure.microsoft.com/en-us/try/cognitive-services/)

### Packages

Nuget Package Available at AzureFunctions.Extensions.CognitiveServices (https://www.nuget.org/packages/AzureFunctions.Extensions.CognitiveServices)

### Language Support

This library has only been tested with C#. I do plan to ensure support for other Language options
with Azure Functions such as Javascript.

### Configuration

##### Appsettings

The following default appsetting values are used if no argument is provided:

- VisionKey : The API Key for your Vision subscription
- VisionUrl : The API Url for your Vision subscription

Attribute Properties such as VisionUrl and VisionKey support the Azure Functions
AutoBinding feature by using the %appsettingskey% value.

##### KeyVault

You can optionally specify the SecureVisionKey and provide the path to your key vault secret (directly or via appsettings).

For example: https://xxxxx.vault.azure.net/secrets/VisionApiKey/XXXXXXXXXXXXX

Note: The current implementation assumes you are using a Managed Service Identity and does not support 
providing direct client id and secrets for Azure AD Applications.

### General Usage Guidelines

Attributes are currently bound to their corresponding client class.  All attributes share a common set of properties
that can be set at the attribute level including Key, Url, and AutoResize**. Each binding also has options and properties
that are unique to that particular Cognitive Services. Additionally attributes values can be customized
and/or overwritten at the request level as well.  This can be advantages if you want additional logic to decide on request
details vs the limited number of assignment and logical operation options available at the attribute level.  All assignments at the request
level will override any values set at at the attribute level.

Binding example with VisionAnalysis Attribute and VisionAnalysisClient binding:

```
[VisionAnalysis(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionAnalysisClient visionclient,

```


Request example w/ VisionAnalysisRequest:

```
    var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));
```

Request example w/ additional and overriding property values:

```
    //Instantiate a new request
    var request = new VisionAnalysisRequest(storageBlob);
    request.AutoResize = true;

    //Specify an alternate url\key - usefull if you have multiple accounts use for different clients\scenerios
    request.Url = "http://xxxxxxx";
    request.Key = "XXXXXXXX";

    //Set unique analysis options for the request
    request.Options = VisionAnalysisOptions.Categories | VisionAnalysisOptions.Description | VisionAnalysisOptions.Tags;

    var result = await visionclient.AnalyzeAsync(request);

```

** AutoResize is currently disabled due to beta assembly binding issues.

### Current Azure Function Extensions for Cognitive Services Bindings

#### VisionAnalysis
```
public static async Task Run(
		   [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionAnalysis(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionAnalysisClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDescribe
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionDescribe(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionDescribeClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.DescribeAsync(new VisionDescribeRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDomain (Celebrity)
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionDomain(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionDomainClient visionclient
           string name,
           TraceWriter log)
        {
            var request = new VisionDomainRequest(storageBlob) { Domain = VisionDomainOptions.Celebrity };
            var result = await visionclient.AnalyzeCelebrityAsync(request);
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDomain (Landmark)
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionDomain(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionDomainClient visionclient
           string name,
           TraceWriter log)
        {
            var request = new VisionDomainRequest(storageBlob) { Domain = VisionDomainOptions.Landscape };
            var result = await visionclient.AnalyzeLandmarkAsync(request);
            log.Info($"Analysis Results:{result.ToString()}");
        }
```
#### VisionHandwriting
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionHandwriting(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionHandwritingClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.HandwritingAsync(new VisionHandwritingRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```
#### VisionOcr
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
            [VisionOcr(Key = "%VisionKey%", Url = "%VisionUrl%")]VisionOcrClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.OCRAsync(new VisionOcrRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```
#### VisionThumbnail
```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionThumbnail(Key = "%VisionKey%",
                          Url = "%VisionUrl%",
                          AutoResize = true,
                          Height ="100",
                          Width = "100",
                          SmartCropping =true)]VisionThumbnailClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.ThumbnailAsync(new VisionThumbnailRequest(storageBlob));
            log.Info($"Image thumbnail generated");
        }
```

