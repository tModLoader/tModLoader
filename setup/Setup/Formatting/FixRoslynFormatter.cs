using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Setup.Formatting
{
	// https://github.com/dotnet/roslyn/issues/36913#issuecomment-587340825
	public class FixRoslynFormatter
	{
		private static bool applied;

		public static void Apply() {
			if (applied)
				return;

			applied = true;

			var a = typeof(CSharpFormattingOptions).Assembly;
			var t = a.GetType("Microsoft.CodeAnalysis.CSharp.Formatting.NewLineUserSettingFormattingRule");
			HookEndpointManager.Modify(t.FindMethod("GetAdjustNewLinesOperation"), (Action<ILContext>)HookAdjustOperation);
			HookEndpointManager.Modify(t.FindMethod("GetAdjustSpacesOperation"), (Action<ILContext>)HookAdjustOperation);

			t = a.GetType("Microsoft.CodeAnalysis.CSharp.Formatting.SuppressFormattingRule");
			HookEndpointManager.Modify(t.GetMethods((BindingFlags)(-1)).Single(m => m.Name == "AddInitializerSuppressOperations" && m.GetParameters().Length == 2), (Action<ILContext>)HookSuppressOperations);
		}

		private static void HookAdjustOperation(ILContext il) {
			var c = new ILCursor(il);
			c.GotoNext(i => i.MatchCall(typeof(CSharpFormattingOptions), "get_NewLinesForBracesInObjectCollectionArrayInitializers"));
			c.GotoPrev(i => i.MatchLdcI4((int)SyntaxKind.ObjectInitializerExpression));

			//in GetAdjustSpacesOperation
			// call instance class SyntaxNode SyntaxToken::get_Parent()
			// ldc.i4 8644
			// call bool CSharpExtensions::IsKind(class SyntaxNode, class SyntaxKind)
			// brfalse ...

			// in GetAdjustNewLinesOperation
			// call instance class SyntaxNode SyntaxToken::get_Parent()
			// call valuetype SyntaxKind CSharpExtensions::Kind(class SyntaxNode)
			// ldc.i4 8644
			// beq ...

			// want both of these checks to also match other syntax kinds than ObjectInitializerExpression (8644)
			c.Remove();
			c.Emit(OpCodes.Dup);
			if (c.Prev.Previous.MatchCall<SyntaxToken>("get_Parent"))
				c.EmitDelegate<Func<SyntaxNode, SyntaxKind>>(Microsoft.CodeAnalysis.CSharp.CSharpExtensions.Kind);

			c.EmitDelegate<Func<SyntaxKind, SyntaxKind>>(kind => {
				if (kind == SyntaxKind.CollectionInitializerExpression || kind == SyntaxKind.ArrayInitializerExpression || kind == SyntaxKind.ImplicitArrayCreationExpression || kind == SyntaxKind.StackAllocArrayCreationExpression)
					return kind; //replace the 'target kind constant' with the current kind

				return SyntaxKind.ObjectInitializerExpression;// equivalent to ldc.if 8644
			});
		}

		/// <summary>
		/// Roslyn supresses all initializers from formatting. This IL edit changes the suprression span to not include the opening brace token.
		/// Note that nested initializers won't be formatted because they're in the ignore span of the parent
		/// </summary>
		private static void HookSuppressOperations(ILContext il) {
			var c = new ILCursor(il);
			c.GotoNext(i => i.MatchCall<SyntaxToken>("GetPreviousToken"));
			c.Index -= 6;
			c.RemoveRange(7);
		}
	}
}
