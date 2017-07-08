// Copyright (c) Tunnel Vision Laboratories, LLC. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace NOpenCL.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.MSBuild;
    using Microsoft.CodeAnalysis.Text;

    public class OpenCLCodeGenerator
    {
        private readonly Workspace _workspace;
        private readonly Solution _solution;

        private INamedTypeSymbol _kernelAttribute;

        private OpenCLCodeGenerator(Workspace workspace, Solution solution)
        {
            _workspace = workspace;
            _solution = solution;
        }

        public static async Task<OpenCLCodeGenerator> CreateAsync(string solutionFilePath, CancellationToken cancellationToken)
        {
            var (workspace, solution) = await OpenSolutionAsync(solutionFilePath, cancellationToken).ConfigureAwait(false);
            return new OpenCLCodeGenerator(workspace, solution);
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
                        await GenerateCodeForTypeAsync(compilation, typeSymbol, cancellationToken).ConfigureAwait(false);
                    }

                    break;

                default:
                    throw new InvalidOperationException("Unreachable");
                }
            }
        }

        private async Task<ImmutableArray<(string, SourceText)>> GenerateCodeForTypeAsync(Compilation compilation, ITypeSymbol typeSymbol, CancellationToken cancellationToken)
        {
            var kernels = typeSymbol.GetMembers()
                .Where(member => member.GetAttributes().Any(attribute => attribute.AttributeClass.Equals(_kernelAttribute)))
                .ToImmutableArray();
            if (kernels.IsEmpty)
            {
                return ImmutableArray<(string, SourceText)>.Empty;
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

            var builder = ImmutableArray.CreateBuilder<(string, SourceText)>();
            builder.Add((typeSymbol.Name + ".g.cl", SourceText.From(visitor.EmbeddedResource, Encoding.UTF8)));

            // Generate the harness
            var harness = SyntaxFactory.ClassDeclaration(typeSymbol.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.IdentifierName(nameof(IDisposable))));

            NameSyntax namespaceName = SyntaxFactory.IdentifierName("TODO");
            string embeddedResource = namespaceName.ToString() + "." + builder[0].Item1;
            harness = harness.AddMembers(
                SyntaxFactory
                    .FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                        .AddVariables(SyntaxFactory.VariableDeclarator("Source").WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(embeddedResource))))))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ConstKeyword)),
                SyntaxFactory
                    .FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("Program"))
                            .AddVariables(SyntaxFactory.VariableDeclarator("_program")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                SyntaxFactory
                    .FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("Kernel"))
                            .AddVariables(SyntaxFactory.VariableDeclarator("_kernel")))
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                SyntaxFactory.ConstructorDeclaration(typeSymbol.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("context")).WithType(SyntaxFactory.IdentifierName("Context")))
                    .AddBodyStatements(
                        SyntaxFactory.IfStatement(
                            SyntaxFactory.BinaryExpression(
                                SyntaxKind.EqualsExpression,
                                SyntaxFactory.IdentifierName("context"),
                                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)),
                            SyntaxFactory.ThrowStatement(
                                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(nameof(ArgumentNullException)))
                                    .WithArgumentList(SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SeparatedList(
                                        new[]
                                        {
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.InvocationExpression(
                                                    SyntaxFactory.IdentifierName("nameof"),
                                                    SyntaxFactory.ArgumentList(
                                                        SyntaxFactory.SeparatedList(
                                                            new[] { SyntaxFactory.Argument(SyntaxFactory.IdentifierName("context")) })))),
                                        }))))),
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.IdentifierName("_program"),
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("context"),
                                    SyntaxFactory.IdentifierName("CreateProgramWithSource")),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(SyntaxFactory.IdentifierName("Source"))))))),
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_program"),
                                SyntaxFactory.IdentifierName("Build"))))));

            foreach (var kernel in kernels)
            {
                if (kernel.DeclaringSyntaxReferences.Length != 1)
                    continue;

                var syntax = await kernel.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
                if (!(syntax is MethodDeclarationSyntax methodDeclaration))
                    continue;

                var semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
                harness = harness.AddMembers(GenerateHarness(semanticModel, methodDeclaration, cancellationToken));
            }

            var compilationUnit = SyntaxFactory.CompilationUnit().AddMembers(
                SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName("TODO"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(System))),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(InAttribute).Namespace)),
                        SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName(nameof(NOpenCL))),
                        SyntaxFactory.UsingDirective(
                            SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("Buffer")),
                            SyntaxFactory.ParseName("NOpenCL.Buffer")))
                    .AddMembers(harness));
            var text = Formatter.Format(compilationUnit, _workspace).ToFullString();
            return builder.ToImmutable();
        }

        private MemberDeclarationSyntax GenerateHarness(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var harnessMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.IdentifierName("EventTask"), methodDeclaration.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

            var block = SyntaxFactory.Block();
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                var harnessParameter = SyntaxFactory.Parameter(parameter.Identifier);
                if (parameter.Type is GenericNameSyntax genericName
                    && genericName.Identifier.ValueText == "Pointer"
                    && genericName.TypeArgumentList.Arguments.Count == 1)
                {
                    harnessParameter = harnessParameter.WithType(SyntaxFactory.IdentifierName("Buffer"));
                }
                else
                {
                    harnessParameter = harnessParameter.WithType(parameter.Type);
                }

                harnessMethod = harnessMethod.AddParameterListParameters(harnessParameter);

                ExpressionSyntax argumentExpression = SyntaxFactory.IdentifierName(parameter.Identifier.Text);
                if (semanticModel.GetTypeInfo(parameter.Type, cancellationToken).Type is INamedTypeSymbol namedType)
                {
                    switch (namedType.SpecialType)
                    {
                    case SpecialType.System_UInt32:
                        // Have to cast to int for SetValue
                        argumentExpression = SyntaxFactory.CheckedExpression(
                            SyntaxKind.UncheckedExpression,
                            SyntaxFactory.CastExpression(
                                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
                                argumentExpression));
                        break;

                    default:
                        break;
                    }
                }

                block = block.AddStatements(SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.ParseExpression($"kernel.Arguments[{block.Statements.Count}].SetValue"),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(argumentExpression))))));
            }

            harnessMethod = harnessMethod.AddBodyStatements(
                SyntaxFactory.UsingStatement(block).WithDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("var"),
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator("kernel").WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.IdentifierName("_program"),
                                        SyntaxFactory.IdentifierName("CreateKernel")),
                                    SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(
                                        SyntaxFactory.ParseExpression($"nameof({methodDeclaration.Identifier.Text})")))))))))));
            return harnessMethod;
        }

        private static async Task<(Workspace, Solution)> OpenSolutionAsync(string solutionFilePath, CancellationToken cancellationToken)
        {
            var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionFilePath, cancellationToken).ConfigureAwait(false);
            return (workspace, solution);
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
