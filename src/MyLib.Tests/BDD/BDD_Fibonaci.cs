using LightBDD.Framework;
using LightBDD.Framework.Scenarios;
using LightBDD.XUnit2;
using System;
using Xunit;

namespace MyLib.Tests.BDD
{
    [FeatureDescription(@"Fibonaci")]
    [Trait("Category", "BDD")]
    public class BDD_Fibonaci : FeatureFixture
    {
        private Fibonaci fibonaciCreator = new Fibonaci();
        private dynamic output = null;

        [Scenario]
        public void Get_fibonaci_series()
        {
            Runner.RunScenario(
                _ => Given_fibonaci_length(5),
                _ => Given_output_format(typeof(string)),
                _ => When_generate_fibonaci_series(),
                _ => Then_receive_the_fibonaci_series_base_on_output_format(typeof(string), "0,1,1,2,3"));
        }

        private void Given_fibonaci_length(int length)
        {
            fibonaciCreator.SetFibonaciLength(length);
        }

        private void Given_output_format(Type type)
        {
            fibonaciCreator.SetOutputFormat(type);
        }

        private void When_generate_fibonaci_series()
        {
            output = fibonaciCreator.Generate();
        }

        private void Then_receive_the_fibonaci_series_base_on_output_format(Type comparedType, string expectedResult)
        {
            var actualResult = Convert.ChangeType(output, comparedType);
            Assert.Equal(expectedResult, actualResult);
        }
    }
}