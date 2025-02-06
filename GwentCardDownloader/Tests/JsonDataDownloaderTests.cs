using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;
using GwentCardDownloader.Models;
using GwentCardDownloader.Services;

namespace GwentCardDownloader.Tests
{
    public class JsonDataDownloaderTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly JsonDataDownloader _jsonDataDownloader;

        public JsonDataDownloaderTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _jsonDataDownloader = new JsonDataDownloader("https://gwent.one/en/cards/");
        }

        [Fact]
        public async Task DownloadCardDataAsync_ShouldReturnCardData_WhenJsonDataIsPresent()
        {
            // Arrange
            var htmlContent = @"
                <html>
                    <head></head>
                    <body>
                        <script>
                            var cardData = [{""Id"":""123456"",""Name"":""Test Card"",""Faction"":""Neutral"",""Type"":""Unit"",""Rarity"":""Common"",""Power"":5,""Provisions"":6,""Categories"":[""Soldier""],""Keywords"":[""Deploy""],""Effects"":[""Damage""],""SupportedArchetypes"":[""Aristocrat""],""Ability"":""Test Ability"",""ArtistName"":""Test Artist"",""FlavorText"":""Test Flavor"",""Set"":""Test Set"",""ReleaseDate"":""2022-01-01T00:00:00Z"",""IsAvailable"":true,""Status"":""Active"",""Version"":""1.0.0"",""LastModified"":""2022-01-01T00:00:00Z"",""PatchNumber"":""1.0""}];
                        </script>
                    </body>
                </html>";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            // Act
            var result = await _jsonDataDownloader.DownloadCardDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("123456", result[0].Id);
            Assert.Equal("Test Card", result[0].Name);
        }

        [Fact]
        public async Task DownloadCardDataAsync_ShouldThrowException_WhenJsonDataIsMissing()
        {
            // Arrange
            var htmlContent = @"
                <html>
                    <head></head>
                    <body>
                        <script>
                            var otherData = [];
                        </script>
                    </body>
                </html>";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _jsonDataDownloader.DownloadCardDataAsync());
        }

        [Fact]
        public async Task DownloadCardDataAsync_ShouldThrowException_WhenJsonDataIsMalformed()
        {
            // Arrange
            var htmlContent = @"
                <html>
                    <head></head>
                    <body>
                        <script>
                            var cardData = [{""Id"":""123456"",""Name"":""Test Card"",""Faction"":""Neutral"",""Type"":""Unit"",""Rarity"":""Common"",""Power"":5,""Provisions"":6,""Categories"":[""Soldier""],""Keywords"":[""Deploy""],""Effects"":[""Damage""],""SupportedArchetypes"":[""Aristocrat""],""Ability"":""Test Ability"",""ArtistName"":""Test Artist"",""FlavorText"":""Test Flavor"",""Set"":""Test Set"",""ReleaseDate"":""2022-01-01T00:00:00Z"",""IsAvailable"":true,""Status"":""Active"",""Version"":""1.0.0"",""LastModified"":""2022-01-01T00:00:00Z"",""PatchNumber"":""1.0""}];
                        </script>
                    </body>
                </html>";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => _jsonDataDownloader.DownloadCardDataAsync());
        }
    }
}
