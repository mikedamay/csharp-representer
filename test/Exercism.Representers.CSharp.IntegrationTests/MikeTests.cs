using Xunit;

namespace Exercism.Representers.CSharp.IntegrationTests
{
    // dotnet test --logger "trx;LogfileName=test_results.xml" --results-directory ./
    public partial class SolutionRepresenterTests
    {
        [Fact]
        public void Single()
        {
            SolutionIsRepresentedCorrectly(new TestSolution("Fake"
                // , "./Solutions/ReadOnly/Fields"));
                , "./Solutions/DictionaryInitializations/ComplexObjectInit"));
        }
    }
}
