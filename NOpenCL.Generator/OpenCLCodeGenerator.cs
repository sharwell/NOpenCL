// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.CodeAnalysis.Text;

    public class OpenCLCodeGenerator
    {
        private readonly Solution _solution;

        private INamedTypeSymbol _kernelAttribute;

        private OpenCLCodeGenerator(Solution solution)
        {
            _solution = solution;
        }

        public static async Task<OpenCLCodeGenerator> CreateAsync(string solutionFilePath, CancellationToken cancellationToken)
        {
            var solution = await OpenSolutionAsync(solutionFilePath, cancellationToken).ConfigureAwait(false);
            return new OpenCLCodeGenerator(solution);
        }

        public Task GenerateCodeForProjectAsync(string projectFilePath, CancellationToken cancellationToken)
        {
            var project = _solution.Projects.Single(p => p.FilePath.Equals(projectFilePath));
            return GenerateCodeAsync(project, cancellationToken);
        }

        private async Task GenerateCodeAsync(Project project, CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
            _kernelAttribute = compilation.GetTypeByMetadataName("NOpenCL.Test.Intel.__kernel");

            var workList = new Queue<INamespaceOrTypeSymbol>();
            workList.Enqueue(compilation.GlobalNamespace);
            while (workList.Count > 0)
            {
                switch (workList.Dequeue())
                {
                case INamespaceSymbol namespaceSymbol:
                    foreach (var type in namespaceSymbol.GetTypeMembers())
                        workList.Enqueue(type);

                    foreach (var ns in namespaceSymbol.GetNamespaceMembers())
                        workList.Enqueue(ns);

                    break;

                case ITypeSymbol typeSymbol:
                    if (typeSymbol.ContainingAssembly.Equals(compilation.Assembly))
                    {
                        await GenerateCodeForTypeAsync(typeSymbol, cancellationToken).ConfigureAwait(false);
                    }

                    break;

                default:
                    throw new InvalidOperationException("Unreachable");
                }
            }
        }

        private async Task GenerateCodeForTypeAsync(ITypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            var kernels = typeSymbol.GetMembers()
                .Where(member => member.GetAttributes().Any(attribute => attribute.AttributeClass.Equals(_kernelAttribute)))
                .ToImmutableArray();
            if (kernels.IsEmpty)
            {
                return;
            }

            if (typeSymbol.TypeKind != TypeKind.Class || !typeSymbol.IsStatic)
                throw new NotSupportedException("OpenCL kernels must be defined in a static class.");

            // Generate OpenCL code for the type
            var visitor = new CodeGeneratorVisitor();
            var declaringSyntaxReferences = typeSymbol.DeclaringSyntaxReferences;
            foreach (var syntaxReference in declaringSyntaxReferences)
            {
                var syntax = await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                visitor.Visit(syntax);
            }

            Console.WriteLine(visitor.EmbeddedResource);

            // Generate the harness
            foreach (var kernel in kernels)
            {
            }
        }

        private static async Task<Solution> OpenSolutionAsync(string solutionFilePath, CancellationToken cancellationToken)
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionFilePath, cancellationToken).ConfigureAwait(false);
            return solution;
        }

        private class CodeGeneratorVisitor : CSharpSyntaxWalker
        {
            private readonly StringBuilder _builder;

            public CodeGeneratorVisitor()
                : base(SyntaxWalkerDepth.Token)
            {
                _builder = new StringBuilder();
            }

            public string EmbeddedResource => _builder.ToString();

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // Visit the members but not the declaration elements themselves
                foreach (var member in node.Members)
                    Visit(member);
            }

            public override void VisitAttributeList(AttributeListSyntax node)
            {
                _builder.Append(node.GetLeadingTrivia().ToFullString());

                foreach (var attribute in node.Attributes)
                {
                    Visit(attribute);
                }

                _builder.Append(node.GetTrailingTrivia().ToFullString());
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (node.Modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    foreach (var variable in node.Declaration.Variables)
                    {
                        _builder.Append(node.GetLeadingTrivia().ToFullString());
                        _builder.Append("#define ");
                        VisitToken(variable.Identifier);
                        Visit(variable.Initializer.Value);
                        _builder.AppendLine();
                    }
                }
                else
                {
                    base.VisitFieldDeclaration(node);
                }
            }

            public override void VisitGenericName(GenericNameSyntax node)
            {
                if (node.Identifier.ValueText == "Pointer" && node.TypeArgumentList.Arguments.Count == 1)
                {
                    Visit(node.TypeArgumentList.Arguments[0]);
                    _builder.Append("*");
                    _builder.Append(node.GetTrailingTrivia().ToFullString());
                }
                else
                {
                    base.VisitGenericName(node);
                }
            }

            public override void VisitToken(SyntaxToken token)
            {
                switch (token.Kind())
                {
                case SyntaxKind.PublicKeyword:
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.InternalKeyword:
                case SyntaxKind.StaticKeyword:
                    if (token.Parent.IsKind(SyntaxKind.MethodDeclaration))
                    {
                        // Print the leading trivia if it's the first modifier
                        if (((MethodDeclarationSyntax)token.Parent).Modifiers.FirstOrDefault().Equals(token))
                            _builder.Append(token.LeadingTrivia.ToFullString());

                        return;
                    }

                    break;

                case SyntaxKind.IdentifierToken:
                    // Append ValueText instead of Text since some OpenCL names may appear escaped in C# code
                    _builder.Append(token.LeadingTrivia.ToFullString());
                    _builder.Append(token.ValueText);
                    _builder.Append(token.TrailingTrivia.ToFullString());
                    return;

                case SyntaxKind.OpenParenToken:
                case SyntaxKind.CloseParenToken:
                    if (token.Parent.IsKind(SyntaxKind.TupleExpression))
                    {
                        _builder.Append(token.LeadingTrivia.ToFullString());
                        _builder.Append(token.IsKind(SyntaxKind.OpenParenToken) ? "{" : "}");
                        _builder.Append(token.TrailingTrivia.ToFullString());
                        return;
                    }

                    break;

                default:
                    break;
                }

                _builder.Append(token.ToFullString());
            }
        }

        ////private class HarnessGeneratorVisitor : CSharpSyntaxWalker
        ////{
        ////    private readonly StringBuilder _builder;

        ////    public CodeGeneratorVisitor()
        ////        : base(SyntaxWalkerDepth.Token)
        ////    {
        ////        _builder = new StringBuilder();
        ////    }

        ////    public override void VisitToken(SyntaxToken token)
        ////    {
        ////        _builder.Append(token.ToFullString());
        ////    }
        ////}
    }
}
