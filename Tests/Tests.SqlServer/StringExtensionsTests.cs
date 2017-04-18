using NUnit.Framework;
using SqlServer;

namespace Tests.SqlServer {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void WhenStringIsSingleQuotedThenDoubleQuotedShouldReplaceItByADoubleQuoted() {
            var singleQuoted = "description's";
            Assert.That(singleQuoted.DoubleQuoted(), Is.EqualTo("description''s"));
        }

        [Test]
        public void WhenStringIsAlreadyDoubleQuotedThenDoubleQuotedDontChangeTheInputString() {
            var singleQuoted = "description''s";
            Assert.That(singleQuoted.DoubleQuoted(), Is.EqualTo("description''s"));
        }

        [TestCase("'''")]
        [TestCase("''''")]
        [TestCase("'''''")]
        [TestCase("''''''")]
        public void WhenStringIsThreeOrMoreQuotesThenDoubleQuotedReturnsInputStringWithOnlyTwoQuotes(string quotes) {
            var singleQuoted = $"description{quotes}s";
            Assert.That(singleQuoted.DoubleQuoted(), Is.EqualTo("description''s"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void WhenStringIsNullOrEmptyThenDoubleQuotedReturnAnEmptyString(string nullOrEmptyInput) {
            Assert.That(nullOrEmptyInput.DoubleQuoted(), Is.Empty);
        }

    }
}