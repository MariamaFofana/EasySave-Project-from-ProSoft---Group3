using EasySave.Services;
using EasySave.UnitTests.Fixtures;
using EasySave.UnitTests.Helpers;
using System.IO;
using Xunit;

namespace EasySave.UnitTests.Services
{
    public class EncryptionServiceTests : IClassFixture<TemporaryDirectoryFixture>
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public EncryptionServiceTests(TemporaryDirectoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ShouldEncrypt_Should_ReturnFalse_When_No_Extensions_Are_Configured()
        {
            // Arrange
            EncryptionService.Configure("fakePath.exe", "key", new string[0]);

            // Act
            bool result = EncryptionService.ShouldEncrypt("document.txt");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Configure_Should_Normalize_Extensions_Correctly()
        {
            // Arrange
            EncryptionService.Configure("fakePath.exe", "key", new[] { "txt", ".pdf", " JPG " });

            // Act
            bool txtResult = EncryptionService.ShouldEncrypt("file.txt");
            bool pdfResult = EncryptionService.ShouldEncrypt("file.pdf");
            bool jpgResult = EncryptionService.ShouldEncrypt("image.jpg");
            bool otherResult = EncryptionService.ShouldEncrypt("notes.docx");

            // Assert
            Assert.True(txtResult);
            Assert.True(pdfResult);
            Assert.True(jpgResult);
            Assert.False(otherResult);
        }

        [Fact]
        public void EncryptIfNeeded_Should_ReturnZero_When_File_Should_Not_Be_Encrypted()
        {
            // Arrange
            EncryptionService.Configure("fakePath.exe", "key", new[] { ".pdf" });

            string testFilePath = Path.Combine(TestPaths.SourceDirectory, "sample.txt");
            File.WriteAllText(testFilePath, "hello");

            // Act
            int result = EncryptionService.EncryptIfNeeded(testFilePath);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void EncryptIfNeeded_Should_ReturnMinusOne_When_CryptoSoft_Path_Is_Invalid()
        {
            // Arrange
            EncryptionService.Configure(@"C:\invalid\CryptoSoft.exe", "key", new[] { ".txt" });

            string testFilePath = Path.Combine(TestPaths.SourceDirectory, "sample.txt");
            File.WriteAllText(testFilePath, "hello");

            // Act
            int result = EncryptionService.EncryptIfNeeded(testFilePath);

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void IsConfigured_Should_ReturnFalse_When_Path_Is_Empty()
        {
            // Arrange
            EncryptionService.Configure("", "key", new[] { ".txt" });

            // Act
            bool result = EncryptionService.IsConfigured;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsConfigured_Should_ReturnFalse_When_No_Extensions_Are_Configured()
        {
            // Arrange
            EncryptionService.Configure("fakePath.exe", "key", new string[0]);

            // Act
            bool result = EncryptionService.IsConfigured;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsConfigured_Should_ReturnTrue_When_Path_And_Extensions_Are_Set()
        {
            // Arrange
            EncryptionService.Configure("fakePath.exe", "key", new[] { ".txt" });

            // Act
            bool result = EncryptionService.IsConfigured;

            // Assert
            Assert.True(result);
        }
    }
}