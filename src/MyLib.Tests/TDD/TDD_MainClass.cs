using Xunit;

namespace MyLib.Tests.TDD
{
    [Trait("Category", "TDD")]
    public class TDD_MainClass
    {
        [Fact]
        public void Foo_Should_ReturnTrue()
        {
            IMainClass mainClass = new MainClass();
            var actualResult = mainClass.Foo();
            Assert.True(actualResult);
        }
    }
}
