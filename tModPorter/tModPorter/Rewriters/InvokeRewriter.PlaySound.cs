using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static tModPorter.Rewriters.SimpleSyntaxFactory;

namespace tModPorter.Rewriters;

partial class InvokeRewriter
{
	private static IDictionary<int, string> _LegacySoundIDs;
	private static string LookupLegacySoundID(Compilation comp, int id) {
		_LegacySoundIDs ??= comp.GetTypeByMetadataName("Terraria.ID.LegacySoundIDs")
			.GetMembers()
			.OfType<IFieldSymbol>()
			.Where(f => f.IsConst)
			.ToDictionary(f => (int)f.ConstantValue, f => f.Name);
		return _LegacySoundIDs.TryGetValue(id, out var name) ? name : null;
	}

	public static RewriteInvoke ConvertPlaySound(bool onMain) => (rw, invoke, _) => {
		var model = rw.model;
		var args = invoke.ArgumentList.Arguments;
		if (onMain)
			invoke = invoke.WithExpression(MemberAccessExpression(rw.UseType("Terraria.Audio.SoundEngine"), "PlaySound").WithTriviaFrom(invoke.Expression));

		ExpressionSyntax type = null;
		ExpressionSyntax position = null;
		ExpressionSyntax style = null;
		ExpressionSyntax volumeScale = null;
		ExpressionSyntax pitchOffset = null;

		// identify variant
		// type, position, style?
		// type, x?, y?, Style?, volumeScale?, pitchOffset?
		var (positional, named) = ParseArgs(args);

		if (named.Any(n => n.NameColon.Name.Identifier.Text == "position") || positional.Length >= 2 && model.GetOperation(positional[1]).Type.ToString() == "Microsoft.Xna.Framework.Vector2") {
			ArrangeArgs(ref positional, named, new string[] { "type", "position", "style" });
			(type, position, style) = (positional[0], positional[1], positional[2]);

		}
		else {
			ArrangeArgs(ref positional, named, new string[] { "type", "x", "y", "Style", "volumeScale", "pitchOffset" });
			(type, var x, var y, style, volumeScale, pitchOffset) = (positional[0], positional[1], positional[2], positional[3], positional[4], positional[5]);

			// x, y to position
			if (model.NonDefault(x, -1) && model.NonDefault(y, -1)) {
				x = UnwrapPredefinedCast(x, "int");
				y = UnwrapPredefinedCast(y, "int");
				if (x is MemberAccessExpressionSyntax { Expression: var _x, Name.Identifier.Text: "X" } && y is MemberAccessExpressionSyntax { Expression: var _y, Name.Identifier.Text: "Y" } && EqualWithNoSuspectedSideEffects(_x, _y)) {
					position = _x;
				}
				else {
					position = ObjectCreationExpression(rw.UseType("Microsoft.Xna.Framework.Vector2"), x, y);
				}
			}
		}

		if (type is ObjectCreationExpressionSyntax { Type: IdentifierNameSyntax { Identifier.Text: "LegacySoundStyle" }, ArgumentList.Arguments: var legacyStyleArgs }) {
			// assume positional, min 2 args
			type = legacyStyleArgs[0].Expression;
			style = legacyStyleArgs[1].Expression;
		}

		bool soundTypeWasInt = false;
		if (model.GetOperation(type) is { ConstantValue.Value: int id }) {
			soundTypeWasInt = true;

			string s = LookupLegacySoundID(model.Compilation, id) ?? $"Unknown{id}";
			type = MemberAccessExpression(rw.UseType("Terraria.ID.SoundID"), s);
		}

		if (type is not MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "SoundID" }, Name.Identifier.Text: var soundIdConstName } soundIDType)
			return invoke;

		int? styleId = null;
		if (style != null) {
			if (model.GetOperation(style) is not { ConstantValue.Value: int _i })
				return invoke;

			styleId = _i;
		}
		else if (onMain || soundTypeWasInt) {
			styleId = 1;
		}

		if (styleId != null) {
			if (soundIdConstName == "NPCKilled")
				soundIdConstName = "NPCDeath";

			var newName = soundIdConstName switch {
				"Item" or "NPCHit" or "NPCDeath" or "Zombie" => soundIdConstName + styleId,
				"Roar" => styleId switch { 1 => "WormDig", 2 => "ScaryScream", 4 => "WormDigQuiet", _ => "Roar" },
				"Splash" => styleId switch { 1 => "SplashWeak", _ => "Splash" },
				"ForceRoar" => styleId switch { -1 => "ForceRoarPitched", _ => "ForceRoar" },
				_ => null
			};

			if (newName != null)
				type = soundIDType.WithName(IdentifierName(newName).WithTriviaFrom(soundIDType.Name));
		}

		if (model.NonDefault(volumeScale, 1f))
			type = InvocationExpression(type.WithoutTrivia(), "WithVolumeScale", volumeScale).WithTriviaFrom(type);

		if (model.NonDefault(pitchOffset, 0f))
			type = InvocationExpression(type.WithoutTrivia(), "WithPitchOffset", pitchOffset).WithTriviaFrom(type);

		var newArgs = new List<ExpressionSyntax> { type };
		if (position != null)
			newArgs.Add(position);

		if (args.Select(arg => arg.Expression).SequenceEqual(newArgs))
			return invoke;

		return invoke.WithArgumentList(ArgumentList(newArgs));
	};
}