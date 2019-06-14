using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    public class AssertTrueOrFalseWithLinqAllShouldBeConvertedToDoesNotContainOrContainsTests
    {
        private readonly DiagnosticAnalyzer analyzer = new AssertTrueOrFalseWithLinqAllShouldBeConvertedToDoesNotContainOrContains();

        private static string Template(string expression) => $@"using System.Linq;
using System.Collections.Generic;
using Xunit;
class TestClass
{{
    void TestMethod()
    {{
        var collection = new List<string>();
        {expression};
    }}
}}";

        [Fact]
        public async Task FindsWarningForAssertTrueAll()
        {
            var sourceCode = Template("Assert.True(collection.All(x => string.IsNullOrEmpty(x)))");
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, sourceCode);

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.True(...) or Assert.False(...) together with LINQ's All(...). Use Assert.DoesNotContain(...) or Assert.Contains(...) instead.", d.GetMessage());
                Assert.Equal("xUnit2021", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }

        [Fact]
        public async Task FindsWarningForAssertFalseAll()
        {
            var sourceCode = Template("Assert.False(collection.All(x => string.IsNullOrEmpty(x)))");
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, sourceCode);

            Assert.Collection(diagnostics, d =>
            {
                Assert.Equal("Do not use Assert.True(...) or Assert.False(...) together with LINQ's All(...). Use Assert.DoesNotContain(...) or Assert.Contains(...) instead.", d.GetMessage());
                Assert.Equal("xUnit2021", d.Id);
                Assert.Equal(DiagnosticSeverity.Info, d.Severity);
            });
        }

        [Fact]
        public async Task DoesntFindWarningForAssertTrueBooleanExpression()
        {
            var sourceCode = Template("Assert.True(120 == 120)");
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, sourceCode);

            Assert.Empty(diagnostics);
        }

        [Fact]
        public async Task DoesntFindWarningForAssertFalseBooleanExpression()
        {
            var sourceCode = Template("Assert.False(120 == 120)");
            var diagnostics = await CodeAnalyzerHelper.GetDiagnosticsAsync(analyzer, sourceCode);

            Assert.Empty(diagnostics);
        }
    }
}
