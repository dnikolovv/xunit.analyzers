using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Xunit.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AssertTrueOrFalseWithLinqAllShouldBeConvertedToDoesNotContainOrContains : XunitDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Descriptors.X2021_AssertTrueOrFalseWithLinqAllShouldBeConvertedToDoesNotContainOrContains);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, XunitContext xunitContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(context =>
            {
                var invocationExpression = (InvocationExpressionSyntax)context.Node;
                var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;

                var calledMethodName = memberAccessExpression?.Name.ToString();

                if (calledMethodName != "True" && calledMethodName != "False")
                    return;

                var memberSymbol = context
                    .SemanticModel
                    .GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

                if (!memberSymbol?.ToString().StartsWith("Xunit.Assert") ?? true)
                    return;

                var argumentList = invocationExpression.ArgumentList;

                if (argumentList?.Arguments.Count < 1)
                    return;

                if (ArgumentExpressionEndsWith("All", argumentList))
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Descriptors.X2021_AssertTrueOrFalseWithLinqAllShouldBeConvertedToDoesNotContainOrContains,
                        invocationExpression.GetLocation()));
                }

            }, ImmutableArray.Create(SyntaxKind.InvocationExpression));
        }

        private static bool ArgumentExpressionEndsWith(string lastMethodCallInExpression, ArgumentListSyntax argumentList) =>
            argumentList.Arguments[0].Expression is InvocationExpressionSyntax argumentInvocationExpression &&
            argumentInvocationExpression.Expression is MemberAccessExpressionSyntax argumentMemberAccessExpression &&
            argumentMemberAccessExpression.Name.ToString() == lastMethodCallInExpression;
    }
}
