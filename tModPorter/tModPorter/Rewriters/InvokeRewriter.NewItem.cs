using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

partial class InvokeRewriter
{
	private const string Vector2Name = "Microsoft.Xna.Framework.Vector2";
	private const string RectangleName = "Microsoft.Xna.Framework.Rectangle";
	private const string PointName = "Microsoft.Xna.Framework.Point";
	private const string Point16Name = "Terraria.DataStructures.Point16";
	private const string ItemName = "Terraria.Item";

	public static SyntaxNode RewriteNewItem(InvokeRewriter rw, InvocationExpressionSyntax invoke, NameSyntax methodName) {
		var model = rw.model;
		var (positional, named) = ParseArgs(invoke.ArgumentList.Arguments);

		// Calls which never had an IEntitySource are left for the modder to fix
		if (positional.Length < 2 || model.TypeOf(positional[0])?.InheritsFrom("Terraria.DataStructures.IEntitySource") is not true)
			return invoke;

		if (IdentifyOldTargetOverload(model, positional) is not string[] posParamNames)
			return invoke;

		if (!NeedsRefactor(posParamNames, positional, named) || !ArrangeNewItemArgs(ref positional, named, posParamNames))
			return invoke;

		ExpressionSyntax Arg(string name) => positional.ElementAtOrDefault(Array.IndexOf(posParamNames, name));
		var (item, type, stack, prefix, noBroadcast) = (Arg("item"), Arg("type"), Arg("stack"), Arg("prefix"), Arg("noBroadcast"));

		// arguments still at their 1.4.4 default are dropped, to keep the call as short as possible
		if (!model.NonDefault(prefix, 0))
			prefix = null;

		if (!model.NonDefault(noBroadcast, false))
			noBroadcast = null;

		var ownership = ToOwnership(rw, Arg("noGrabDelay"));
		var newPos = NewPosition(rw, posParamNames, positional);

		ExpressionSyntax[] itemArgs = item != null ? [item] : [type, stack, prefix];
		ExpressionSyntax velocity = null, modifier = null;
		ExpressionSyntax[] newArgs = [positional[0], .. newPos, .. itemArgs, ownership, velocity, modifier, noBroadcast];

		// the overload is identified by the types of the two parameters following source
		string param1Type = posParamNames is [_, "rectangle", ..] ? RectangleName : Vector2Name;
		string param2Type = newPos is [_, _] ? Vector2Name : item != null ? ItemName : "int";

		if (FindNewItemOverload(model, newArgs.Length, param1Type, param2Type) is not IMethodSymbol method)
			return invoke;

		var argList = ArgumentList(method, newArgs).WithTriviaFrom(invoke.ArgumentList);

		// a 5 argument (source, center, type, stack, prefix) call is ambiguous with the deprecated (source, position, Width, Height, type) overload. Supplying prefix by name resolves it.
		if ((param1Type, param2Type) is (Vector2Name, "int") && ownership == null && argList.Arguments is [_, _, _, _, { NameColon: null } prefixArg, ..])
			argList = argList.ReplaceNode(prefixArg, prefixArg.WithNameColon(NameColon("prefix").WithTrailingTrivia(Space)));

		return invoke.WithArgumentList(argList);
	}

	// Identifies the 1.4.4 overload being called from the types of its arguments, and returns its parameter names. Null if it's not an overload we can port.
	private static string[] IdentifyOldTargetOverload(SemanticModel model, ExpressionSyntax[] positional) {
		var arg1Type = model.TypeOf(positional[1]);

		string[] positionParams;
		if (arg1Type.Is(RectangleName)) {
			positionParams = ["rectangle"];
		}
		else if (arg1Type.IsIntegral()) {
			positionParams = ["X", "Y", "Width", "Height"];
		}
		else if (arg1Type.Is(Vector2Name)) {
			var arg2Type = model.TypeOf(positional.ElementAtOrDefault(2));
			// (position, Width, Height, type/item, ...) vs (center, type, stack, noBroadcast, ...)
			var arg4Type = model.TypeOf(positional.ElementAtOrDefault(4));
			if (arg2Type.Is(Vector2Name)) {
				positionParams = ["position", "size"];
			}
			else if (arg2Type.IsIntegral() && (arg4Type.IsIntegral() || arg4Type.Is(ItemName))) {
				positionParams = ["position", "Width", "Height"];
			}
			else {
				positionParams = ["position"];
			}
		}
		else {
			return null;
		}

		var itemArg = positional.ElementAtOrDefault(1 + positionParams.Length);
		bool itemVariant = model.TypeOf(itemArg).Is(ItemName) || itemArg != null && itemArg.IsKind(SyntaxKind.NullLiteralExpression);
		string[] tailParams = itemVariant
			? ["item", "noBroadcast", "noGrabDelay", "reverseLookup"]
			: ["type", "stack", "noBroadcast", "prefix", "noGrabDelay", "reverseLookup"];

		return ["source", .. positionParams, .. tailParams];
	}

	// 1.4.4 -> 1.4.5 param names
	private static readonly Dictionary<string, string> NewItemParamAliases = new() {
		["Type"] = "type",
		["Stack"] = "stack",
		["pfix"] = "prefix",
		["prefixGiven"] = "prefix",
	};

	private static bool NeedsRefactor(string[] posParamNames, ExpressionSyntax[] positional, ArgumentSyntax[] named) =>
		posParamNames is [_, "X", ..] or [_, _, "Width", ..] || // unergonomic 1.4.4 forms (X, Y, Width, Height) or (position, Width, Height)
		positional.Length > Array.IndexOf(posParamNames, "noBroadcast") || // Parameters from 'noBroadcast' onwards were moved, changed or removed
		named.Select(n => n.NameColon.Name.Identifier.Text).Any(name => NewItemParamAliases.ContainsKey(name) || name is "noGrabDelay" or "reverseLookup"); // renamed or removed named argument references

	// Resizes the positional arg array to match the selected overload, and converts any matching named args into positional
	private static bool ArrangeNewItemArgs(ref ExpressionSyntax[] positional, ArgumentSyntax[] named, string[] posParamNames) {
		Array.Resize(ref positional, posParamNames.Length);
		foreach (var n in named) {
			string name = n.NameColon.Name.Identifier.Text;
			int i = Array.IndexOf(posParamNames, NewItemParamAliases.GetValueOrDefault(name, name));
			if (i < 0) // a name we don't recognise means the overload was misidentified
				return false;

			positional[i] = n.Expression;
		}

		return true;
	}

	// (X, Y, Width, Height) -> (center)
	// (position, Width, Height) -> (position, size)
	// (position, size) -> unchanged
	// (position) -> unchanged
	// (rectangle) -> unchanged
	private static ExpressionSyntax[] NewPosition(InvokeRewriter rw, string[] posParamNames, ExpressionSyntax[] positional) => (posParamNames[1], posParamNames[2]) switch {
		("X", _) => [ToCenter(rw, positional[1], positional[2], positional[3], positional[4])],
		("position", "Width") => [positional[1], ObjectCreationExpression(rw.UseType(Vector2Name), positional[2], positional[3])],
		("position", "size") => [positional[1], positional[2]],
		_ => [positional[1]],
	};

	private static ExpressionSyntax ToOwnership(InvokeRewriter rw, ExpressionSyntax noGrabDelay) {
		if (noGrabDelay == null)
			return null;

		ExpressionSyntax Ownership(string value) => MemberAccessExpression(rw.UseType("Terraria.NewItemOwnership"), value);

		return rw.model.GetOperation(noGrabDelay)?.ConstantValue switch {
			{ HasValue: true, Value: false } => null,
			{ HasValue: true, Value: true } => Ownership("GrabDelayForAllPlayers"),
			_ => ConditionalExpression(noGrabDelay, OperatorToken(SyntaxKind.QuestionToken), Ownership("GrabDelayForAllPlayers"), OperatorToken(SyntaxKind.ColonToken), Ownership("None")),
		};
	}

	// (X, Y, Width, Height) -> the center of the box, in whichever form reads best
	//   entity.position.X, entity.position.Y, entity.width, entity.height  ->  entity.Center
	//   tileX * 16, tileY * 16, 16, 16                                     ->  new Point(tileX, tileY).ToWorldCoordinates()
	//   v.X, v.Y, 0, 0                                                     ->  v
	//   anything else                                                      ->  new Vector2(X + Width / 2, Y + Height / 2)
	private static ExpressionSyntax ToCenter(InvokeRewriter rw, ExpressionSyntax x, ExpressionSyntax y, ExpressionSyntax width, ExpressionSyntax height) {
		x = UnwrapPredefinedCast(x, "int");
		y = UnwrapPredefinedCast(y, "int");

		if (ToWorldCoordinates(rw, x, y, width, height) is ExpressionSyntax tileCenter)
			return tileCenter;

		if (x is MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: var ex, Name.Identifier.Text: "position" }, Name.Identifier.Text: "X" }
			&& y is MemberAccessExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: var ey, Name.Identifier.Text: "position" }, Name.Identifier.Text: "Y" }
			&& width is MemberAccessExpressionSyntax { Expression: var ew, Name.Identifier.Text: "width" }
			&& height is MemberAccessExpressionSyntax { Expression: var eh, Name.Identifier.Text: "height" }
			&& EqualWithNoSuspectedSideEffects(ex, ey)
			&& EqualWithNoSuspectedSideEffects(ex, ew)
			&& EqualWithNoSuspectedSideEffects(ex, eh))
			return MemberAccessExpression(ex, "Center");

		if (!rw.model.NonDefault(width, 0) && !rw.model.NonDefault(height, 0)) {
			if (x is MemberAccessExpressionSyntax { Expression: var px, Name.Identifier.Text: "X" }
				&& y is MemberAccessExpressionSyntax { Expression: var py, Name.Identifier.Text: "Y" }
				&& EqualWithNoSuspectedSideEffects(px, py))
				return px;

			return ObjectCreationExpression(rw.UseType(Vector2Name), x, y);
		}

		static ExpressionSyntax PlusHalf(ExpressionSyntax coord, ExpressionSyntax size) =>
			SimpleSyntaxFactory.BinaryExpression(SyntaxKind.AddExpression, coord,
				SimpleSyntaxFactory.BinaryExpression(SyntaxKind.DivideExpression, size is BinaryExpressionSyntax ? Parens(size) : size, IntLiteral(2)));

		return ObjectCreationExpression(rw.UseType(Vector2Name), PlusHalf(x, width), PlusHalf(y, height));
	}

	// (tileX * 16, tileY * 16, 16, 16) is a tile, and reads much better in tile coordinates
	private static ExpressionSyntax ToWorldCoordinates(InvokeRewriter rw, ExpressionSyntax x, ExpressionSyntax y, ExpressionSyntax width, ExpressionSyntax height) {
		if (TileCoord(rw, x) is not ExpressionSyntax tileX || TileCoord(rw, y) is not ExpressionSyntax tileY)
			return null;

		// the offset from the top left of the tile has to be expressible as a literal
		if (rw.model.GetOperation(width)?.ConstantValue is not { HasValue: true, Value: int w } || w <= 0 || w % 2 != 0 ||
			rw.model.GetOperation(height)?.ConstantValue is not { HasValue: true, Value: int h } || h <= 0 || h % 2 != 0)
			return null;

		bool IsPoint(ExpressionSyntax e) => rw.model.TypeOf(e) is var t && (t.Is(PointName) || t.Is(Point16Name));

		var point = tileX is MemberAccessExpressionSyntax { Expression: var px, Name.Identifier.Text: "X" }
			&& tileY is MemberAccessExpressionSyntax { Expression: var py, Name.Identifier.Text: "Y" }
			&& EqualWithNoSuspectedSideEffects(px, py) && IsPoint(px)
				? px.WithoutTrivia()
				: ObjectCreationExpression(rw.UseType(PointName), tileX.WithoutTrivia(), tileY.WithoutTrivia());

		return w == 16 && h == 16
			? InvocationExpression(point, "ToWorldCoordinates")
			: InvocationExpression(point, "ToWorldCoordinates", IntLiteral(w / 2), IntLiteral(h / 2));
	}

	private static ExpressionSyntax TileCoord(InvokeRewriter rw, ExpressionSyntax expr) =>
		expr is BinaryExpressionSyntax mul && mul.IsKind(SyntaxKind.MultiplyExpression)
			&& rw.model.GetOperation(mul.Right)?.ConstantValue is { HasValue: true, Value: 16 }
			&& rw.model.TypeOf(mul.Left).IsIntegral() ? mul.Left : null;

	private static LiteralExpressionSyntax IntLiteral(int value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

	private static IMethodSymbol FindNewItemOverload(SemanticModel model, int paramCount, string param1Type, string param2Type) =>
		model.Compilation.GetTypeByMetadataName(ItemName)
			.GetMembers("NewItem")
			.OfType<IMethodSymbol>()
			.FirstOrDefault(m => !m.IsObsolete()
				&& m.Parameters.Length == paramCount
				&& m.Parameters[1].Type.ToString() == param1Type
				&& m.Parameters[2].Type.ToString() == param2Type);
}
