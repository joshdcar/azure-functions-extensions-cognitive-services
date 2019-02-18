 # <img align="left" height="100" src="https://raw.githubusercontent.com/joshdcar/azure-functions-extensions-cognitive-services/master/logo.png"> &nbsp;  Azure Function Extensions for Cognitive Services (Preview)

Azure Function Extensions for Cognitive Services is a set of custom Azure Function bindings for Azure's Cognitive Services. 

### Features:

 - Supports a subset of the Computer Vision API including 
   - Image Analysis
   - Optical Character Recognition (OCR)
   - Handwriting, Celebrities & Landmarks, 
   - Thumbnail generation.
 - Cognitive Service API calls with a couple lines of code
 - Support for automatic retry policies for throttled requests
 - Support for automatic image resize for files larger then 4mb (Cognitive Services Limit)
 - ~~Support for Key Vault storage of Cognitive Services Keys~~ Removed in Preview 4 as this is now supported in the Azure Functions Platform
 - Exception, Warning, and request time metrics logging

Additional support for the remainder of the Vision offerings and additional support for Language and Knowledge will be coming very soon. 

More details on the various Cognitive Services offerings can be found at [https://azure.microsoft.com/en-us/services/cognitive-services/](https://azure.microsoft.com/en-us/services/cognitive-services/)

### Current Version

Latest Version: 1.0.0-preview4 

This release introduced support for Azure Functions 2.0 GA 

Current Azure Function Dependency:  Microsoft.Azure.Webjobs.Extensions (3.0.1)

**Note**: Some Cognitive Services are in preview.  ImageSharp, the library used for native .net core image resizing is also in preview.   

These bindings currently **only support Azure Functions 2.0** . There are no current plans for 1.x support. 

## QuickStart

### Prerequisites
You must have a valid Azure Cognitive Services Vision License Key and Url. You can obtain one with a Azure Subscription. If you do not have a subscription you can sign up for a free trial at [https://azure.microsoft.com/en-us/try/cognitive-services/](https://azure.microsoft.com/en-us/try/cognitive-services/)

### Packages

Nuget Package Available at AzureFunctions.Extensions.CognitiveServices (https://www.nuget.org/packages/AzureFunctions.Extensions.CognitiveServices)

### Language Support

This library has only been tested with C#. Other language support will be validated\added soon. 

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

These extensions support numerouse combinations of bindings for each service available in cognitive services. The available binding and their associated behaviors depend 
on the Image Source provided within the binding attribute. 

The following bindings are supported.

| Image Source | Model Binding | Client Binding |
| ------------ | :-----------: | :------------: |
| Client (default)       |               |        x       |
| BlobStorage  |       X       |        x       |
| Url          |       X       |        x       |


#### Model Class Binding

Model Binding is the fastest and simples way to automatically analyze a given image. It requires and image source
to be available at the time of binding so a given cognitive services query can be immediatly executed. Because of this
it only supports BlobStorage and Url sources.  Blob Storage has the additional requirement of a connection and path. 


```
...
[VisionAnalysis(BlobStorageConnection = "storageaccount",
                BlobStoragePath = "analysismodel/{name}",
                ImageSource = ImageSource.BlobStorage)]VisionAnalysisModel result,
string name
...
```

It is often advantageous to us these extensions in combination with a blob trigger where the name parameter
is provided by the Blob Trigger and the path and connection is already known.

```
 [BlobTrigger("analysismodel/{name}")]Stream storageBlob,
 [VisionAnalysis(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionAnalysisModel result,
 string name,

```

Each Vision Binding has one or more corresponding models  available:

| Vision Binding   | Model(s)      |
| ------------      | :----------- |
| VisionAnalysis    | VisionAnalysisModel         |   
| VisionDescribe    | VisionDescribeModel             |       
| VisionDomain      | VisionDomainCelebrityModel, VisionDomainLandmarkModel              |   
| VisionHandwriting | VisionHandwritingModel              | 
| VisionOcr         | VisionOcrModel              | 
| VisionThumbnail   | Byte[]              | 

#### Client Binding

If your image source meets one of the following criteria then working with a Vision Client may be more usefull:

- Your image is not available in Blob Storage or in a publically accessible URL
- Your image is not available at binding time
- Your image is in an external source that requires a seperate request
- You want to modify your image (beyond resize) before processing
- You do not want to use the default processing options for a given request. 

Due to the unique requirements of each cognitive service each vision binding has a dedicated vision client and vision request object.

The following client and requests classes are available for each binding

| Vision Binding   | Client      | Request   |
| ------------      | :----------- |:----------- |
| VisionAnalysis    | VisionAnalysisClient      |   VisionAnalysisRequest | 
| VisionDescribe    | VisionDescribeClient      |   VisionDescribeRequest |  
| VisionDomain      | VisionDomainClient        |   VisionDomainRequest |  
| VisionHandwriting | VisionHandwritingClient   |   VisionHandwritingRequest |  
| VisionOcr         | VisionOcrClient           |   VisionOcrRequest |  
| VisionThumbnail   | VisionThumbnailClient     |   VisionThumbnailRequest |  


Binding example with VisionAnalysis Attribute and VisionAnalysisClient binding:

```
...
[VisionAnalysis()]VisionAnalysisClient visionclient,
... {

    byte[] fileBytes = FetchMyFile();

    //Instantiate a new request
    var request = new VisionAnalysisRequest(fileBytes);
    request.AutoResize = true;

    //Set unique analysis options for the request
    request.Options = VisionAnalysisOptions.Categories | VisionAnalysisOptions.Description | VisionAnalysisOptions.Tags;

    //Make the request
    var result = await visionclient.AnalyzeAsync(request);

}

```


### Current Azure Function Extensions for Cognitive Services Bindings

#### VisionAnalysis

##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionAnalysis(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionAnalysisModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example

```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionAnalysis()]VisionAnalysisClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.AnalyzeAsync(new VisionAnalysisRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDescribe


##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionDescribe(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionDescribeModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example

```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionDescribe()]VisionDescribeClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.DescribeAsync(new VisionDescribeRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDomain (Celebrity)


##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionDomain(BlobStorageAccount = "StorageAccount",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionDomainCelebrityModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example


```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionDomain()]VisionDomainClient visionclient
           string name,
           TraceWriter log)
        {
            var request = new VisionDomainRequest(storageBlob) { Domain = VisionDomainOptions.Celebrity };
            var result = await visionclient.AnalyzeCelebrityAsync(request);
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

#### VisionDomain (Landmark)

##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionDomain(BlobStorageConnection = "%storageaccount%",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionDomainLandmarkModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example


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

##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionHandwriting(BlobStorageConnection = "%storageaccount%",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionHandwritingModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example


```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
           [VisionHandwriting()]VisionHandwritingClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.HandwritingAsync(new VisionHandwritingRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```
#### VisionOcr


##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionOcr(BlobStorageConnection = "%storageaccount%",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]VisionOcrModel result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results:{result.ToString()}");
        }
```

##### Client Binding Example


```
public static async Task Run(
           [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
            [VisionOcr()]VisionOcrClient visionclient,
           string name,
           TraceWriter log)
        {
            var result = await visionclient.OCRAsync(new VisionOcrRequest(storageBlob));
            log.Info($"Analysis Results:{result.ToString()}");
        }
```
#### VisionThumbnail

##### Model Binding Example
```
public static async Task Run(
    [BlobTrigger("visionrequest/{name}")]Stream storageBlob,
    [VisionThumbnail(BlobStorageConnection = "%storageaccount%",
                           BlobStoragePath = "analysismodel/{name}",
                           ImageSource = ImageSource.BlobStorage)]Byte[] result,
           string name,
           TraceWriter log)
        {
            log.Info($"Analysis Results: {result.length}");
        }
```

##### Client Binding Example



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

