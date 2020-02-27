using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Exercism.Representers.CSharp.Normalization
{
    internal class ReorderMembers : CSharpSyntaxRewriter
    {
        public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node) =>
            base.VisitCompilationUnit(
                node.WithMembers(
                    List(OrderMembers(node))));

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) =>
            base.VisitClassDeclaration(
                node.WithMembers(List(node.Members
                    .OrderBy(member => member.Kind())
                    .ThenBy(member => member.IdentifierName()))));

        private static IEnumerable<MemberDeclarationSyntax> OrderMembers(CompilationUnitSyntax node) =>
            node.Members
                .OrderBy(member => member.Kind())
                .ThenBy(member => member.IdentifierName());
    }
}