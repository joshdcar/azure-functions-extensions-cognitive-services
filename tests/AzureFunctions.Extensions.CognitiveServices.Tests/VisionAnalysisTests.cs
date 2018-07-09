using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Services.Models;
using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using AzureFunctions.Extensions.CognitiveServices.Config;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class VisionAnalysisTests
    {

        private static VisionAnalysisModel visionAnalysisUrlResult;
        private static VisionAnalysisModel visionAnalysisImageBytesResult;
        private static VisionAnalysisModel visionAnalysisImageBytesResizeResult;

        [Fact]
        public static async Task TestVisionAnalysisWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithUrl");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithImageBytes");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionAnalysisModel>(MockResults.VisionAnalysisResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>(client, "VisionFunctions.VisionAnalysisWithTooBigImageBytesWithResize");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionAnalysisImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionAnalysisImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>
                        (client, "VisionFunctions.VisionAnalysisWithTooBigImageBytes"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }


        [Fact]
        public static async Task TestVisionAnalysisMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionAnalysisBinding>
                        (client, "VisionFunctions.VisionAnalysisMissingFile"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);

        }

        private class VisionFunctions
        {

            public async Task VisionAnalysisWithUrl(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();
                request.ImageUrl = "http://www.blah";

                var result = await client.AnalyzeAsync(request);

                visionAnalysisUrlResult = result;
            }

            public async Task VisionAnalysisWithImageBytes(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionAnalysisClient client)
            {
                var request = new VisionAnalysisRequest();
                request.ImageBytes = MockResults.SamplePhoto;

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResult = result;
            }

            public async Task VisionAnalysisWithTooBigImageBytes(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%" , AutoResize = false)]
                 VisionAnalysisClient client)
            {
                
                 var request = new VisionAnalysisRequest();

                 request.ImageBytes = MockResults.SamplePhotoTooBig;

                 var result = await client.AnalyzeAsync(request);

            }

            public async Task VisionAnalysisWithTooBigImageBytesWithResize(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey = "%VisionKey%", AutoResize = true)]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.AnalyzeAsync(request);

                visionAnalysisImageBytesResizeResult = result;

            }

            public async Task VisionAnalysisMissingFile(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);

            }

            public async Task VisionAnalysisKeyvault(
                [VisionAnalysis(VisionUrl = "%VisionUrl%", VisionKey ="[VisionKey]")]
                 VisionAnalysisClient client)
            {

                var request = new VisionAnalysisRequest();

                var result = await client.AnalyzeAsync(request);

            }
        }
    }

   
}
