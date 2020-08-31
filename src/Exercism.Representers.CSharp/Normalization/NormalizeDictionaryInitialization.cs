using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Serilog;

namespace Exercism.Representers.CSharp.Normalization
{
    public class NormalizeDictionaryInitialization : CSharpSyntaxRewriter
    {
        private class InvalidKindException : Exception
        {
            public SyntaxKind Kind;
        }

        private readonly SemanticModel semanticModel;

        public NormalizeDictionaryInitialization(SemanticModel semanticModel)
        {
            if (semanticModel is null)
            {
                Log.Error(
                    $"{nameof(NormalizeDictionaryInitialization)}: semantic model is null - this normalization will fail");
            }

            this.semanticModel = semanticModel;
        }

        public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
        {
            SyntaxNode DefaultVisit() => base.VisitInitializerExpression(node);

            if (semanticModel is null)
            {
                return DefaultVisit();
            }

            try
            {
                if (semanticModel.GetOperation(node) is IObjectOrCollectionInitializerOperation initializerOperation)
                {
                    if (initializerOperation.Parent?.Type?.Name != "Dictionary")
                    {
                        if (initializerOperation.Parent?.Type?.Name is null)
                        {
                            Log.Error(
                                $"{nameof(NormalizeDictionaryInitialization)}: unable to retrieve the type information for {nameof(initializerOperation)}");
                        }

                        return DefaultVisit();
                        // presumably a list or some object or other
                    }

                    var initializerNodeDetails
                        = node.Kind() switch
                        {
                            SyntaxKind.CollectionInitializerExpression
                            => ExtractInitializersWithCollectionSyntax(initializerOperation),
                            SyntaxKind.ObjectInitializerExpression
                            => ExtractInitializersWithObjectSyntax(initializerOperation),
                            _ 
                            => throw new InvalidKindException{Kind = node.Kind()}
                        };

                    if (!initializerNodeDetails.Success)
                    {
                        Log.Error(
                            $"{nameof(NormalizeDictionaryInitialization)}: dictionary initialization found with incorrect number of valid arguments (!=2)");
                        return DefaultVisit();
                    }

                    var explodedInitializerSyntaxNodes = initializerNodeDetails.InitializerSyntaxNodes
                        .Select(initializer => new KeyValuePair<SyntaxNode, SyntaxNode>(
                            this.Visit(initializer.Key),
                            this.Visit(initializer.Value))
                        );

                    var dictionaryAsText = BuildDictionaryAsText(explodedInitializerSyntaxNodes);
                    var dictionarySyntaxTree = CSharpSyntaxTree.ParseText(dictionaryAsText);
                    var replacementNode = dictionarySyntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .FirstOrDefault(n => n is InitializerExpressionSyntax);
                    if (replacementNode == default(SyntaxNode))
                    {
                        Log.Error(
                            $"{nameof(NormalizeDictionaryInitialization)}: failed to find a {nameof(InitializerExpressionSyntax)} node in generated dictionary fragment");
                        return DefaultVisit();
                    }

                    return replacementNode;
                }
            }
            catch (InvalidKindException ike)
            {
                Log.Error(
                    $"{nameof(NormalizeDictionaryInitialization)}: {nameof(InitializerExpressionSyntax)} found with unexpected Kind {ike.Kind}");
            }
            catch (Exception e)
            {
                Log.Error(e, $"{nameof(NormalizeDictionaryInitialization)}: unknown error");
            }

            return DefaultVisit();
        }

        private (bool Success, List<KeyValuePair<SyntaxNode, SyntaxNode>> InitializerSyntaxNodes)
            ExtractInitializersWithObjectSyntax(IObjectOrCollectionInitializerOperation initializerOperation)
        {
            try
            {
                var initializerSyntaxNodes = initializerOperation.Initializers
                    .Select(initializer => new
                    {
                        arg0 = initializer.Descendants().FirstOrDefault(d => d is IArgumentOperation),
                        arg1 = (initializer as IAssignmentOperation)?.Value
                    })
                    .Select(p => (p.arg0?.Syntax is null || p.arg1?.Syntax is null)
                        ? throw new NullReferenceException()
                        : new KeyValuePair<SyntaxNode, SyntaxNode>(p.arg0.Syntax, p.arg1.Syntax))
                    .ToList();
                return (true, initializerSyntaxNodes);
            }
            catch (NullReferenceException)
            {
                return (false, null);
            }
        }

        private (bool Success, List<KeyValuePair<SyntaxNode, SyntaxNode>> InitializerSyntaxNodes)
            ExtractInitializersWithCollectionSyntax(IObjectOrCollectionInitializerOperation initializerOperation)
        {
            try
            {
                var initializerSyntaxNodes = initializerOperation.Initializers
                    .Select(initializer => (initializer as IInvocationOperation)?.Arguments)
                    .Select(args =>
                        (args?.Length != 2 || args?[0]?.Syntax is null || args?[1]?.Syntax is null)
                            ? throw new NullReferenceException()
                            : new KeyValuePair<SyntaxNode, SyntaxNode>(args?[0]?.Syntax, args?[1]?.Syntax)
                    ).ToList();
                return (true, initializerSyntaxNodes);
            }
            catch (NullReferenceException)
            {
                return (false, null);
            }
        }

        private string BuildDictionaryAsText(IEnumerable<KeyValuePair<SyntaxNode, SyntaxNode>> initializerArgNodes)
        {
            var sb = new StringBuilder();
            sb.AppendLine("var dict = new Dictionary{");
            foreach (var initializerArg in initializerArgNodes)
            {
                sb.AppendLine($"{{{initializerArg.Key.GetText()}, {initializerArg.Value.GetText()}}},");
            }

            sb.AppendLine("};");
            return sb.ToString();
        }
    }
}