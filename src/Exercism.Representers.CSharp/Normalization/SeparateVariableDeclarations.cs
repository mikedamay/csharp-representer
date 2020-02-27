using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.Representers.CSharp.Normalization
{
    internal class SeparateVariableDeclarations : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            if (node.Variables.Count > 1 &&
                node.Parent is LocalDeclarationStatementSyntax localDeclarationStatement &&
                localDeclarationStatement.Parent is BlockSyntax block)
            {
                var exitingIndex = block.Statements.IndexOf(localDeclarationStatement);
                var updatedStatements = block.Statements.RemoveAt(exitingIndex);

                for (var i = node.Variables.Count - 1; i >= 0; i--)
                    updatedStatements = updatedStatements.Insert(i, LocalDeclarationStatement(
                        node.WithVariables(SingletonSeparatedList(node.Variables[i]))));
                
                return base.VisitBlock(block);
            }
            
            return base.VisitVariableDeclaration(node);
        }
    }
}