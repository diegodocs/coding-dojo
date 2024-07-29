namespace CodingDojoCSharp.Tests
{
    public class CodingDojoExampleTest
    {
        
        [Test]
        public void When_Conditions_Then_Results()
        {
            //arrange
            var expectedValue = true;
            var currentValue = false;
            var dojo = new CodingDojoExample();

            //act
            currentValue = dojo.Calculate();

            //assert
            Assert.That(currentValue, Is.EqualTo(expectedValue));

        }
    }
}