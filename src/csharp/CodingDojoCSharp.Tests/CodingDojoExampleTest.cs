namespace CodingDojoCSharp.Tests
{
    public class CodingDojoExampleTest
    {
        
        [Test]
        public void When_TestConditions_Then_Results()
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

        [TestCase(true)]
        public void When_TestCaseConditions_Then_Results(bool expectedValue)
        {
            //arrange
            var currentValue = false;
            var dojo = new CodingDojoExample();

            //act
            currentValue = dojo.Calculate();

            //assert
            Assert.That(currentValue, Is.EqualTo(expectedValue));

        }
    }
}