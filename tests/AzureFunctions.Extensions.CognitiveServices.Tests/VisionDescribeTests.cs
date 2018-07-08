using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Analysis;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Describe;
using AzureFunctions.Extensions.CognitiveServices.Bindings.Vision.Thumbnail;
using AzureFunctions.Extensions.CognitiveServices.Config;
using AzureFunctions.Extensions.CognitiveServices.Services;
using AzureFunctions.Extensions.CognitiveServices.Tests.Resources;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureFunctions.Extensions.CognitiveServices.Tests
{
    public class VisionDescribeTests
    {
        private static VisionDescribeModel visionDescribeUrlResult;
        private static VisionDescribeModel visionDescribeImageBytesResult;
        private static VisionDescribeModel visionDescribeImageBytesResizeResult;


        [Fact]
        public static async Task TestVisionAnalysisWithUrl()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithUrl");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeUrlResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeWithImageBytes()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithImageBytes");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeWithImageWithResize()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();
            var mockResult = JsonConvert.DeserializeObject<VisionDescribeModel>(MockResults.VisionDescribeResults);

            await TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>(client, "VisionFunctions.VisionDescribeWithTooBigImageBytesWithResize");

            var expectedResult = JsonConvert.SerializeObject(mockResult);
            var actualResult = JsonConvert.SerializeObject(visionDescribeImageBytesResizeResult);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public static async Task TestVisionDescribeImageBytesTooLarge()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            string exceptionMessage = "or smaller for the cognitive service vision API";

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>
                        (client, "VisionFunctions.VisionDescribeWithTooBigImageBytes"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(exceptionMessage);

        }


        [Fact]
        public static async Task TestVisionDescribeMissingFile()
        {
            ICognitiveServicesClient client = new TestCognitiveServicesClient();

            var exception = await Record.ExceptionAsync(() => TestHelper.ExecuteFunction<VisionFunctions, VisionDescribeBinding>
                        (client, "VisionFunctions.VisionDescribeMissingFile"));

            exception.Should().NotBeNull();
            exception.InnerException.Should().NotBeNull();
            exception.InnerException.Should().BeOfType<ArgumentException>();
            exception.InnerException.Message.Should().Contain(VisionExceptionMessages.FileMissing);

        }

        private class VisionFunctions
        {

            public async Task VisionDescribeWithUrl(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();
                request.ImageUrl = "http://www.blah";

                var result = await client.DescribeAsync(request);

                visionDescribeUrlResult = result;
            }

            public async Task VisionDescribeWithImageBytes(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionDescribeClient client)
            {
                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);

                visionDescribeImageBytesResult = result;
            }

            public async Task VisionDescribeWithTooBigImageBytes(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%", AutoResize=false)]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();
                request.AutoResize = false;
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.DescribeAsync(request);

            }

            public async Task VisionDescribeWithTooBigImageBytesWithResize(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey = "%VisionKey%", AutoResize = true)]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();
                request.ImageBytes = MockResults.SamplePhotoTooBig;

                var result = await client.DescribeAsync(request);

                visionDescribeImageBytesResizeResult = result;

            }

            public async Task VisionDescribeMissingFile(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey ="%VisionKey%")]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);

            }

            public async Task VisionDescribeKeyvault(
                [VisionDescribe(VisionUrl = "%VisionUrl%", VisionKey ="[VisionKey]")]
                 VisionDescribeClient client)
            {

                var request = new VisionDescribeRequest();

                var result = await client.DescribeAsync(request);

            }
        }
    }
}
