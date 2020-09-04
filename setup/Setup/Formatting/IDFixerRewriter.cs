using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Simplification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Terraria.ModLoader.Setup;
//using Terraria.ModLoader.Setup;

namespace Terraria.ModLoader.Setup.Formatting
{
	public class IDFixerRewriter : CSharpSyntaxRewriter
	{
		class FieldToIDTypeBinding
		{
			// Example: Item - createTile - TileID
			internal string fullyQualifiedClassName;
			internal string className;
			internal string field;
			internal string idType;
			internal IdDictionary idDictionary;

			//public FieldToIDTypeBinding(string fullName, string field, string idType, IdDictionary idDictionary) {
			//	this.fullyQualifiedClassName = fullName;
			//	this.className = fullName.Substring(fullName.LastIndexOf(".") + 1);
			//	this.field = field;
			//	this.idType = idType;
			//	this.idDictionary = idDictionary;
			//}

			public FieldToIDTypeBinding(string fullName, string field, string idType) {
				this.fullyQualifiedClassName = fullName;
				this.className = fullName.Substring(fullName.LastIndexOf(".") + 1);
				this.field = field;
				this.idType = idType;
#if DEBUG
				if(IDFixerTask.idDictionaries.ContainsKey(idType))
#endif
					this.idDictionary = IDFixerTask.idDictionaries[idType];
			}

			public override string ToString() => $"{fullyQualifiedClassName} {field} {idType}";
		}

		class MethodParameterToIDTypeBinding
		{
			internal string fullyQualifiedMethodName;
			internal string methodName;
			internal string fullMethodWithParameters;
			internal string[] parameterNames;
			internal int parameterIndex;
			internal string idType;
			internal IdDictionary idDictionary;

			public MethodParameterToIDTypeBinding(string fullyQualifiedMethodName, string fullMethodWithParameters, string[] parameterNames, int parameterIndex, string idType/*, IdDictionary idDictionary*/) {
				this.fullyQualifiedMethodName = fullyQualifiedMethodName;
				this.methodName = fullyQualifiedMethodName.Substring(fullyQualifiedMethodName.LastIndexOf(".") + 1);
				this.fullMethodWithParameters = fullMethodWithParameters;
				this.parameterNames = parameterNames;
				this.parameterIndex = parameterIndex;
				this.idType = idType;
#if DEBUG
				if (IDFixerTask.idDictionaries.ContainsKey(idType))
#endif
					this.idDictionary = IDFixerTask.idDictionaries[idType];
			}

			public override string ToString() => $"{fullMethodWithParameters} {parameterIndex} {idType}";
		}

		internal static int totalFilesChanged;
		internal static int totalChanges;
		private readonly SemanticModel SemanticModel;
		private List<FieldToIDTypeBinding> FieldToIDTypeBindings;
		private List<MethodParameterToIDTypeBinding> MethodParameterToIDTypeBindings;
		public IDFixerRewriter(SemanticModel semanticModel) {
			SemanticModel = semanticModel;
			
			FieldToIDTypeBindings = new List<FieldToIDTypeBinding>();
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "createTile", "TileID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "createWall", "WallID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "type", "ItemID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "shoot", "ProjectileID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "makeNPC", "NPCID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "useStyle", "ItemUseStyleID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "holdStyle", "ItemHoldStyleID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Tile", "type", "TileID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Tile", "wall", "WallID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.NPC", "type", "NPCID"));
			FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.NPC", "catchItem", "ItemID"));
			// doesn't exist at this stage: 
			//FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Main", "netMode", "NetmodeID"));
			//FieldToIDTypeBindings.Add(new FieldToIDTypeBinding("Terraria.Item", "rare", "ItemRarityID", IDFixerTask.idDictionaries["ItemRarityID"]));

			MethodParameterToIDTypeBindings = new List<MethodParameterToIDTypeBinding>();
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.SetDefaults", "Terraria.Item.SetDefaults(int)", new string[] { "Int32" }, 0, "ItemID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.SetDefaults", "Terraria.Item.SetDefaults(int, bool)", new string[] { "Int32", "Boolean" }, 0, "ItemID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.NewItem", "Terraria.Item.NewItem(int, int, int, int, int, int, bool, int, bool, bool)", new string[] { "Int32", "Int32", "Int32", "Int32", "Int32", "Int32", "Boolean", "Int32", "Boolean", "Boolean" }, 4, "ItemID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.NewItem", "Terraria.Item.NewItem(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, bool, int, bool, bool)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Boolean", "Int32", "Boolean", "Boolean" }, 2, "ItemID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Item.NewItem", "Terraria.Item.NewItem(Microsoft.Xna.Framework.Vector2, int, int, int, int, bool, int, bool, bool)", new string[] { "Vector2", "Int32", "Int32", "Int32", "Int32", "Boolean", "Int32", "Boolean", "Boolean" }, 3, "ItemID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(float, float, float, float, int, int, float, int, float, float)", new string[] { "Single", "Single", "Single", "Single", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 4, "ProjectileID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectile", "Terraria.Projectile.NewProjectile(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Projectile.NewProjectileDirect", "Terraria.Projectile.NewProjectileDirect(Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float)", new string[] { "Vector2", "Vector2", "Int32", "Int32", "Single", "Int32", "Single", "Single" }, 2, "ProjectileID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.NetMessage.SendData", "Terraria.NetMessage.SendData(int, int, int, Terraria.Localization.NetworkText, int, float, float, float, int, int, int)", new string[] { "Int32", "Int32", "Int32", "NetworkText", "Int32", "Single", "Single", "Single", "Int32", "Int32", "Int32" }, 0, "MessageID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Audio.SoundEngine.PlaySound", "Terraria.Audio.SoundEngine.PlaySound(int, Microsoft.Xna.Framework.Vector2, int)", new string[] { "Int32", "Vector2", "Int32" }, 0, "SoundID"));
			MethodParameterToIDTypeBindings.Add(new MethodParameterToIDTypeBinding("Terraria.Audio.SoundEngine.PlaySound", "Terraria.Audio.SoundEngine.PlaySound(int, int, int, int, float, float)", new string[] { "Int32", "Int32", "Int32", "Int32", "Single", "Single" }, 0, "SoundID"));

			// TODO: Need to better support optional parameters maybe?
			// TODO: other side of method parameter: instead of calls to the method, code inside the method using the parameter
			// TODO: the ID Sets: Factory.CreateBoolSet, etc
		}

		public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node) {
			InvocationExpressionSyntax invocationExpressionSyntax = node;

			IMethodSymbol symbol;
			// Check if expression is just a method name: SetDefaults(123)
			if (invocationExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax) {
				symbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol as IMethodSymbol;
			}
			// Check if expression is accessing a member: item.SetDefaults(123)
			else if (invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
				symbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol as IMethodSymbol;
			}
			else
				return base.VisitInvocationExpression(node);

			if (symbol == null || symbol.ContainingType == null)
				return base.VisitInvocationExpression(node);

			var argumentListSyntax = invocationExpressionSyntax.ArgumentList as ArgumentListSyntax;
			if (argumentListSyntax == null)
				return base.VisitInvocationExpression(node);
			int argumentCount = argumentListSyntax.Arguments.Count;

			string fullyQualifiedMethodName = symbol.ToString();
			var parameterTypeNames = symbol.Parameters.Select(p => p.Type.Name);
			var methodParameterToIDTypeBinding = MethodParameterToIDTypeBindings.FirstOrDefault(x => x.fullMethodWithParameters == fullyQualifiedMethodName && x.parameterNames.SequenceEqual(parameterTypeNames));
			if (methodParameterToIDTypeBinding == null)
				return base.VisitInvocationExpression(node);

			if (argumentCount <= methodParameterToIDTypeBinding.parameterIndex)
				return base.VisitInvocationExpression(node);
			if (!(argumentListSyntax.Arguments[methodParameterToIDTypeBinding.parameterIndex].Expression is LiteralExpressionSyntax parameter && parameter.IsKind(SyntaxKind.NumericLiteralExpression)))
				return base.VisitInvocationExpression(node);

			// TODO: Named arguments can mix up the order of parameters, invalidating the parameter logic 
			if (!(parameter.Token.Value is int))
				return base.VisitInvocationExpression(node);

			int rightValue = (int)parameter.Token.Value;

			if (methodParameterToIDTypeBinding.idDictionary.TryGetName(rightValue, out string IdentifierString)) {
				NameSyntax itemIDIdentifider = SyntaxFactory.IdentifierName(methodParameterToIDTypeBinding.idType);
				IdentifierNameSyntax name = SyntaxFactory.IdentifierName(IdentifierString);

				var newNode = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, itemIDIdentifider, name)
											.WithLeadingTrivia(parameter.GetLeadingTrivia())
											.WithTrailingTrivia(parameter.GetTrailingTrivia());

				totalChanges++;

				return node.ReplaceNode(parameter, newNode);
			}

			return base.VisitInvocationExpression(node);
		}

		public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node) {
			// Only support simple assignment: a = b
			if (!node.IsKind(SyntaxKind.SimpleAssignmentExpression))
				return base.VisitAssignmentExpression(node);

			// Check if right side is literal number: a = 123
			if (!(node.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
				return base.VisitAssignmentExpression(node);

			ISymbol symbol;
			// Check if left is just a field: a = 123
			if (node.Left is IdentifierNameSyntax identifierNameSyntax) {
				symbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
			}
			// Check if left is accessing a member: a.b = 123
			else if (node.Left is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
				symbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
			}
			else
				return base.VisitAssignmentExpression(node);

			// Check if left Type exists: item.b = 123
			if (symbol == null || symbol.ContainingType == null)
				return base.VisitAssignmentExpression(node);

			if (!(symbol is IFieldSymbol fieldSymbol))
				return base.VisitAssignmentExpression(node);

			string containingType = symbol.ContainingType.ToString();
			string fieldName = fieldSymbol.Name;
			var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName);
			if (FieldToIDTypeBinding == null)
				return base.VisitAssignmentExpression(node);

			int rightValue = (int)right.Token.Value;
			if (FieldToIDTypeBinding.idDictionary.TryGetName(rightValue, out string IdentifierString)) {
				NameSyntax IDIdentifier = SyntaxFactory.IdentifierName(FieldToIDTypeBinding.idType);
				IdentifierNameSyntax name = SyntaxFactory.IdentifierName(IdentifierString);

				var newNode = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IDIdentifier, name)
											.WithLeadingTrivia(right.GetLeadingTrivia())
											.WithTrailingTrivia(right.GetTrailingTrivia());
				totalChanges++;
				return node.ReplaceNode(right, newNode);
			}
			return base.VisitAssignmentExpression(node);
		}

		public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node) {
			// Check if right side is literal number: a == 123
			if (!(node.Right is LiteralExpressionSyntax right && right.IsKind(SyntaxKind.NumericLiteralExpression)))
				return base.VisitBinaryExpression(node);

			ISymbol symbol;
			// Check if left is just a field: a = 123
			if (node.Left is IdentifierNameSyntax identifierNameSyntax) {
				symbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
			}
			// Check if left is accessing a member: a.b = 123
			else if (node.Left is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
				symbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
			}
			else
				return base.VisitBinaryExpression(node);

			// Check if left Type exists: item.b = 123
			if (symbol == null || symbol.ContainingType == null)
				return base.VisitBinaryExpression(node);

			if (!(symbol is IFieldSymbol fieldSymbol))
				return base.VisitBinaryExpression(node);

			string containingType = symbol.ContainingType.ToString();

			string fieldName = fieldSymbol.Name;
			if(fieldName == "netMode") {
				Console.WriteLine();
			}
			var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName);
			if (FieldToIDTypeBinding == null)
				return base.VisitBinaryExpression(node);

			int rightValue = (int)right.Token.Value;

			if (FieldToIDTypeBinding.idDictionary.TryGetName(rightValue, out string IdentifierString)) {
				NameSyntax IDIdentifier = SyntaxFactory.IdentifierName(FieldToIDTypeBinding.idType);
				IdentifierNameSyntax name = SyntaxFactory.IdentifierName(IdentifierString);

				var newNode = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IDIdentifier, name)
											.WithLeadingTrivia(right.GetLeadingTrivia())
											.WithTrailingTrivia(right.GetTrailingTrivia());
				totalChanges++;
				return node.ReplaceNode(right, newNode);
			}

			return base.VisitBinaryExpression(node);
		}

		public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node) {
			var toReplace = new List<LiteralExpressionSyntax>();

			ISymbol symbol;
			// Check if expression is just a field: switch(type)
			if (node.Expression is IdentifierNameSyntax identifierNameSyntax) {
				symbol = SemanticModel.GetSymbolInfo(identifierNameSyntax).Symbol;
			}
			// Check if expression is accessing a member: switch(item.type)
			else if (node.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax) {
				symbol = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
			}
			else
				return base.VisitSwitchStatement(node);

			// Check if Type exists
			if (symbol == null || symbol.ContainingType == null)
				return base.VisitSwitchStatement(node);
			if (!(symbol is IFieldSymbol fieldSymbol))
				return base.VisitSwitchStatement(node);

			// See if we have ID metadata about Type
			string containingType = symbol.ContainingType.ToString();
			string fieldName = fieldSymbol.Name;
			var FieldToIDTypeBinding = FieldToIDTypeBindings.FirstOrDefault(x => x.fullyQualifiedClassName == containingType && x.field == fieldName);
			if (FieldToIDTypeBinding == null)
				return base.VisitSwitchStatement(node);

			// Iterate over all switch labels.
			foreach (var section in node.Sections) {
				foreach (var switchLabel in section.Labels) {
					if (switchLabel is CaseSwitchLabelSyntax caseSwitchLabel && caseSwitchLabel.Value is LiteralExpressionSyntax numericLiteralExpression
						&& numericLiteralExpression.IsKind(SyntaxKind.NumericLiteralExpression)) {
						int switchLabelValue = (int)numericLiteralExpression.Token.Value;
						if (FieldToIDTypeBinding.idDictionary.ContainsId(switchLabelValue)) {
							toReplace.Add(numericLiteralExpression);
						}
					}
				}
			}

			// Batch node replacement happens in ReplaceNodes
			if (toReplace.Count > 0) {
				return node.ReplaceNodes(toReplace, (n1, n2) => {
					int switchLabelValue = (int)n1.Token.Value;

					if (FieldToIDTypeBinding.idDictionary.TryGetName(switchLabelValue, out string IdentifierString)) {
						NameSyntax IDIdentifier = SyntaxFactory.IdentifierName(FieldToIDTypeBinding.idType);
						IdentifierNameSyntax name = SyntaxFactory.IdentifierName(IdentifierString);

						var newNode = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IDIdentifier, name)
													.WithLeadingTrivia(n1.GetLeadingTrivia())
													.WithTrailingTrivia(n1.GetTrailingTrivia());
						totalChanges++;
						return newNode;
					}
					return n2;
				});
			}
			return base.VisitSwitchStatement(node);
		}

		public override SyntaxNode VisitCaseSwitchLabel(CaseSwitchLabelSyntax node) {
			return base.VisitCaseSwitchLabel(node);
			// Below is a hacky implementation of something old.

			//var SetDefaultsMethodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax>(
			//	(x) => x.Identifier.ValueText == "SetDefaults1");

			//if (SetDefaultsMethodDeclaration != null) {
			//	System.Console.WriteLine();

			//	if (node.Value is LiteralExpressionSyntax numericLiteralExpression && numericLiteralExpression.IsKind(SyntaxKind.NumericLiteralExpression)) {

			//		int itemidValue = (int)numericLiteralExpression.Token.Value;

			//		if (ItemID.Search.ContainsId(itemidValue)) {
			//			string itemidIdentifierString = ItemID.Search.GetName(itemidValue);
			//			NameSyntax itemIDIdentifider = SyntaxFactory.IdentifierName("ItemID");
			//			IdentifierNameSyntax name = SyntaxFactory.IdentifierName(itemidIdentifierString);

			//			var newNode = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, itemIDIdentifider, name);
			//			totalChanges++;
			//			return node.ReplaceNode(numericLiteralExpression, newNode);
			//		}
			//	}

			//	//NameSyntax name = SyntaxFactory.CaseSwitchLabel(SyntaxFactory.ExpressionStatement());
			//	//NameSyntax name = SyntaxFactory.IdentifierName("Oops");

			//	//node.with
			//	//var newRoot = root.ReplaceNode(itemIDArgumentLiteral, name);
			//}

			//return base.VisitCaseSwitchLabel(node);
		}
	}
}
