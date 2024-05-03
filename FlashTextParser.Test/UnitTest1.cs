using NUnit.Framework;
using FlashTextParser.Interfaces;

namespace FlashTextParser.Test
{
    [TestFixture]
    public class TextSanitizerTests
    {
        private IBannedWordController _bannedWordController;

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void SanitizeText_EmptyText_ReturnsEmpty()
        {
            // Arrange
            string inputText = "";

            // Act
            var result = _bannedWordController.SanitizeText(inputText);

            // Assert
            Assert.AreEqual("", result.Value);
        }

        [Test]
        public void SanitizeText_NoBannedWords_ReturnsOriginalText()
        {
            // Arrange
            string inputText = "This is a clean text.";

            // Act
            var result = _bannedWordController.SanitizeText(inputText);

            // Assert
            Assert.AreEqual(inputText, result.Value);
        }

        [Test]
        public void SanitizeText_BannedWord_ReturnsTextWithBannedWordReplaced()
        {
            // Arrange
            string inputText = "This is a test word.";
            string expected = "This is a **** word.";

            // Act
            var result = _bannedWordController.SanitizeText(inputText);

            // Assert
            Assert.AreEqual(expected, result.Value);
        }

        
    }
}