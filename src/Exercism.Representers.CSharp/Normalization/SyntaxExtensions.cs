using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exercism.Representers.CSharp.Normalization
{
    internal static class SyntaxExtensions
    {
        public static string IdentifierName(this CSharpSyntaxNode member) =>
            member switch
            {
                null => "null",
                ClassDeclarationSyntax classDeclaration => classDeclaration.Identifier.Text,
                MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.Text,
                VariableDeclaratorSyntax variableDeclarator => variableDeclarator.Identifier.Text,
                IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
                _ => member.ToString()
            };
    }
}