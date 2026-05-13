using EasySave.Services;
using System;
using System.IO;
using Xunit;

namespace EasySave.IntegrationTests.Encryption
{
    public class EncryptionIntegrationTests : IDisposable
    {
        private readonly string _baseTestDirectory;

        public EncryptionIntegrationTests()
        {
            _baseTestDirectory = Path.Combine(Path.GetTempPath(), "EasySaveIntegrationTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_baseTestDirectory);
        }

        [Fact]
        public void EncryptIfNeeded_Should_Modify_File_And_Return_Positive_Time_When_CryptoSoft_Is_Available()
        {
            // Arrange
            string cryptoSoftPath = GetCryptoSoftPath();
            Assert.True(File.Exists(cryptoSoftPath), $"CryptoSoft.exe not found at: {cryptoSoftPath}");

            string filePath = Path.Combine(_baseTestDirectory, "sample.txt");
            string originalContent = "Hello EasySave integration test!";
            File.WriteAllText(filePath, originalContent);

            EncryptionService.Configure(
                cryptoSoftPath,
                "EasySaveKey",
                new[] { ".txt" }
            );

            // Act
            int result = EncryptionService.EncryptIfNeeded(filePath);

            // Assert
            Assert.True(result > 0, $"Expected encryption time > 0, but got {result}");

            string encryptedContent = File.ReadAllText(filePath);
            Assert.NotEqual(originalContent, encryptedContent);
        }

        private static string GetCryptoSoftPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string candidate1 = Path.Combine(
                baseDir, "..", "..", "..", "..", "..",
                "CryptoSoft", "bin", "Debug", "net8.0", "CryptoSoft.exe");

            string candidate2 = Path.Combine(
                baseDir, "..", "..", "..", "..", "..",
                "CryptoSoft", "bin", "Release", "net8.0", "CryptoSoft.exe");

            candidate1 = Path.GetFullPath(candidate1);
            candidate2 = Path.GetFullPath(candidate2);

            if (File.Exists(candidate1))
                return candidate1;

            if (File.Exists(candidate2))
                return candidate2;

            return candidate1;
        }
        [Fact]
        public void EncryptIfNeeded_Should_Not_Modify_File_And_Return_Zero_When_Extension_Is_Not_Allowed()
        {
            // Arrange
            string cryptoSoftPath = GetCryptoSoftPath();
            Assert.True(File.Exists(cryptoSoftPath), $"CryptoSoft.exe not found at: {cryptoSoftPath}");

            string filePath = Path.Combine(_baseTestDirectory, "sample.pdf");
            string originalContent = "This file should not be encrypted.";
            File.WriteAllText(filePath, originalContent);

            EncryptionService.Configure(
                cryptoSoftPath,
                "EasySaveKey",
                new[] { ".txt" } // .pdf is not allowed
            );

            // Act
            int result = EncryptionService.EncryptIfNeeded(filePath);

            // Assert
            Assert.Equal(0, result);

            string finalContent = File.ReadAllText(filePath);
            Assert.Equal(originalContent, finalContent);
        }
        [Fact]
        public void EncryptIfNeeded_Should_Return_Negative_And_Not_Modify_File_When_CryptoSoft_Path_Is_Invalid()
        {
            // Arrange
            string filePath = Path.Combine(_baseTestDirectory, "sample.txt");
            string originalContent = "This file should remain unchanged if encryption fails.";
            File.WriteAllText(filePath, originalContent);

            EncryptionService.Configure(
                @"C:\invalid\CryptoSoft.exe",
                "EasySaveKey",
                new[] { ".txt" }
            );

            // Act
            int result = EncryptionService.EncryptIfNeeded(filePath);

            // Assert
            Assert.True(result < 0, $"Expected a negative error code, but got {result}");

            string finalContent = File.ReadAllText(filePath);
            Assert.Equal(originalContent, finalContent);
        }
        public void Dispose()
        {
            if (Directory.Exists(_baseTestDirectory))
            {
                Directory.Delete(_baseTestDirectory, true);
            }
        }
    }
}