using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters
{
	public class RecipeRewriter : BaseRewriter {

		private class RecipeSetResultVisitor : CSharpSyntaxVisitor {
			private readonly List<ILocalReferenceOperation> references = new();
			private readonly SemanticModel model;

			public RecipeSetResultVisitor(SemanticModel model) {
				this.model = model;
			}

			public static List<ILocalReferenceOperation> GetReferences(SemanticModel model, SyntaxNode node) {
				var v = new RecipeSetResultVisitor(model);
				v.DefaultVisit(node);
				return v.references;
			}

			public override void DefaultVisit(SyntaxNode node) {
				foreach (var c in node.ChildNodes())
					Visit(c);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
				if (node.Parent is ExpressionStatementSyntax &&
					node.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax target, Name.Identifier.Text: "SetResult" } &&
					model.GetOperation(target) is ILocalReferenceOperation local && IsRecipe(local.Type)) {
						references.Add(local);
				}

				base.VisitInvocationExpression(node);
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax node) { }
			public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) { }
			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) { }
			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) { }

			private bool IsRecipe(ITypeSymbol type) {
				if (type.ToString() == "Terraria.Recipe")
					return true;

				if (type.BaseType == null)
					return false;

				// the BaseType in the SemanticModel for classes extending Recipe is object because Recipe is sealed
				if (type.BaseType.ToString() != "object")
					return IsRecipe(type.BaseType);

				if (type.DeclaringSyntaxReferences.Length != 1 || type.DeclaringSyntaxReferences[0].GetSyntax() is not ClassDeclarationSyntax decl ||
					decl.BaseList is not BaseListSyntax baseList || baseList.Types.Count == 0)
					return false;

				var baseTypeSyntax = baseList.Types[0].Type;
				var sm = model.Compilation.GetSemanticModel(decl.SyntaxTree);
				var t = sm.GetTypeInfo(baseTypeSyntax);
				return t.Type != null && IsRecipe(t.Type);
			}
		}

		public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
			var refs = RecipeSetResultVisitor.GetReferences(model, node);
			if (refs.Count == 0)
				return base.VisitMethodDeclaration(node);

			var cfg = ControlFlowGraph.Create((IMethodBodyOperation)model.GetOperation(node));
			var thisType = model.GetDeclaredSymbol(node) is IMethodSymbol mSym && !mSym.IsStatic && mSym.ContainingType.InheritsFrom("Terraria.ModLoader.ModItem");
			var localReferenceAnalysis = LocalReferenceAnalysis.Analyze(cfg);
			foreach (var r in refs) {
				var setResultNode = r.Parent.Syntax as InvocationExpressionSyntax;
				var setResultStatement = setResultNode.Parent;

				var assign = localReferenceAnalysis[r.Syntax];
				if (assign.Length == 1) {
					var creation = assign[0].Value.Syntax;
					RegisterAction(creation, n => AddResultToCreateRecipe(n, setResultNode.ArgumentList, thisType));
					RegisterAction(GoodCommentableParent(creation), n => n.WithTrailingCommentsFrom(setResultStatement));
					RegisterAction(setResultStatement, n => null);
				}
				else {
					RegisterAction(setResultStatement, n => n.WithBlockComment("Pass result to CreateRecipe."));
				}
			}

			return base.VisitMethodDeclaration(node);
		}

		private SyntaxNode AddResultToCreateRecipe(SyntaxNode node, ArgumentListSyntax resultArgs, bool isModItem) {
			if (isModItem &&
				!(node is not InvocationExpressionSyntax invoke || invoke.ArgumentList.Arguments.Count > 0) &&
				!(invoke.Expression is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "Mod" }, Name: IdentifierNameSyntax { Identifier.Text: "CreateRecipe" } }) &&
				resultArgs.Arguments[0].Expression is ThisExpressionSyntax) {

				return invoke.WithExpression(IdentifierName("CreateRecipe")).WithArgumentList(invoke.ArgumentList.WithArguments(resultArgs.Arguments.RemoveAt(0)));
			}

			var itemExpr = resultArgs.Arguments[0].Expression;
			if (model.GetOperation(itemExpr) is IOperation { Type: ITypeSymbol type } && type.InheritsFrom("Terraria.ModLoader.ModItem")) {
				resultArgs = resultArgs.ReplaceNode(itemExpr, MemberAccessExpression(itemExpr, "Type"));
			}

			return AppendArgs(node, resultArgs);
		}

		public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
			var origNode = node;

			node = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);
			if (node.Type is IdentifierNameSyntax { Identifier.Text: "Recipe" } && node.ArgumentList.Arguments.Count > 0) {
				var args = node.ArgumentList.Arguments;
				var target = args[0].Expression;
				args = args.RemoveAt(0);

				// this converts to the old `mod.CreateRecipe` style, it gets factored to Recipe.Create or ModItem.CreateRecipe later
				bool isCallOnThis = target is ThisExpressionSyntax && model.GetEnclosingSymbol(origNode.SpanStart) is IMethodSymbol mSym && !mSym.IsStatic && mSym.ContainingType.InheritsFrom("Terraria.ModLoader.Mod");
				var expr = MemberAccessExpression(target, "CreateRecipe");
				return InvocationExpression(isCallOnThis ? expr.Name : expr, node.ArgumentList.WithArguments(args));
			}

			return node;
		}

		private static SyntaxNode AppendArgs(SyntaxNode n, ArgumentListSyntax extraArgs) {
			return n switch {
				ObjectCreationExpressionSyntax newObj => newObj.WithArgumentList(newObj.ArgumentList.Concat(extraArgs)),
				InvocationExpressionSyntax invoke => invoke.WithArgumentList(invoke.ArgumentList.Concat(extraArgs)),
				_ => n
			};
		}

		private static SyntaxNode GoodCommentableParent(SyntaxNode node) => node.Parent switch {
			AssignmentExpressionSyntax { Parent: ExpressionStatementSyntax stmt } => stmt,
			EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax stmt } } } => stmt,
			_ => node
		};
	}
}
