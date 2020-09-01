using Xunit;

namespace Exercism.Representers.CSharp.IntegrationTests
{
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
