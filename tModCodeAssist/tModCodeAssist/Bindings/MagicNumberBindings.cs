using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using ReLogic.Reflection;
using Terraria.ID;

namespace tModCodeAssist.Bindings;

public static class MagicNumberBindings
{
	public abstract class Binding(Binding.CreationContext context)
	{
		public readonly record struct CreationContext(
			in string OwningClassName,
			in string MemberName,
			in string ShortIdType,
			in string FullIdType,
			in IdDictionary Search,
			in bool AllowNegativeIDs = false
		);

		public string ShortIdType => context.ShortIdType;
		public string FullIdType => context.FullIdType;
		public IdDictionary Search => context.Search;
		public bool AllowNegativeIDs => context.AllowNegativeIDs;
	}
	private sealed class FieldBinding(Binding.CreationContext context) : Binding(context)
	{
	}
	private sealed class MethodParameterBinding(Binding.CreationContext context, int parameterOrder) : Binding(context)
	{
		public int ParameterOrder => parameterOrder;
	}

	// Foo
	private static readonly SymbolDisplayFormat MethodNameOnlyDisplayFormat = SymbolDisplayFormat.MinimallyQualifiedFormat
		.WithMemberOptions(SymbolDisplayMemberOptions.None)
		.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
		.WithParameterOptions(SymbolDisplayParameterOptions.None);

	// Foo(int, System.Collections.Generic.List<int>)
	private static readonly SymbolDisplayFormat MethodWithQualifiedParametersDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat
		.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
		.WithMemberOptions(SymbolDisplayMemberOptions.IncludeParameters)
		.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)
		.WithParameterOptions(SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeExtensionThis);

	private static readonly object @lock = new();
	private static ConcurrentDictionary<Type, IdDictionary> searchCache;
	private static ConcurrentDictionary<string, Dictionary<string, Binding>> bindingByMemberByOwningClass;

	public static void PopulateBindings()
	{
		lock (@lock) {
			if (bindingByMemberByOwningClass != null)
				return;

			searchCache = [];
			bindingByMemberByOwningClass = [];

			AddBinding<TileID>("Terraria.Item", "createTile", (ctx) => new FieldBinding(ctx));
			AddBinding<ItemID>("Terraria.Item", "type", (ctx) => new FieldBinding(ctx));
			AddBinding<ItemID>("Terraria.Player", "cursorItemIconID", (ctx) => new FieldBinding(ctx));
			AddBinding<ProjectileID>("Terraria.Item", "shoot", (ctx) => new FieldBinding(ctx));
			AddBinding<ProjectileID>("Terraria.ModLoader.ModProjectile", "AIType", (ctx) => new FieldBinding(ctx));
			AddBinding<ItemUseStyleID>("Terraria.Item", "useStyle", (ctx) => new FieldBinding(ctx));
			AddBinding(typeof(ItemRarityID), "Terraria.Item", "rare", (ctx) => new FieldBinding(ctx), allowNegativeIDs: true);
			AddBinding<NPCAIStyleID>("Terraria.NPC", "aiStyle", (ctx) => new FieldBinding(ctx));
			AddBinding<NPCID>("Terraria.NPC", "type", (ctx) => new FieldBinding(ctx));
			AddBinding<NPCID>("Terraria.ModLoader.ModNPC", "AIType", (ctx) => new FieldBinding(ctx));
			AddBinding<NPCID>("Terraria.ModLoader.ModNPC", "AnimationType", (ctx) => new FieldBinding(ctx));
			AddBinding(typeof(NetmodeID), "Terraria.Main", "netMode", (ctx) => new FieldBinding(ctx));
			AddBinding<ProjAIStyleID>("Terraria.Projectile", "aiStyle", (ctx) => new FieldBinding(ctx));
			AddBinding<ProjectileID>("Terraria.Projectile", "type", (ctx) => new FieldBinding(ctx));
			AddBinding<DustID>("Terraria.ModLoader.ModBlockType", "DustType", (ctx) => new FieldBinding(ctx));
			AddBinding<DustID>("Terraria.ModLoader.ModDust", "UpdateType", (ctx) => new FieldBinding(ctx));
			AddBinding<TileID>("Terraria.Tile", "TileType", (ctx) => new FieldBinding(ctx));
			AddBinding<WallID>("Terraria.Tile", "WallType", (ctx) => new FieldBinding(ctx));
			AddBinding(typeof(PaintID), "Terraria.Tile", "TileColor", (ctx) => new FieldBinding(ctx));
			AddBinding(typeof(PaintID), "Terraria.Tile", "WallColor", (ctx) => new FieldBinding(ctx));
			AddBinding(typeof(LiquidID), "Terraria.Tile", "LiquidType", (ctx) => new FieldBinding(ctx));
			AddBinding<ExtrasID>("Terraria.GameContent.TextureAssets", "Extra", (ctx) => new FieldBinding(ctx), typeof(short));

			AddBinding<ItemID>("Terraria.Item", "CloneDefaults", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<MessageID>("Terraria.NetMessage", "SendData", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<DustID>("Terraria.Dust", "NewDust", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<DustID>("Terraria.Dust", "NewDustDirect", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<DustID>("Terraria.Dust", "NewDustPerfect", (ctx) => new MethodParameterBinding(ctx, 1));
			AddBinding<ItemID>("Terraria.Recipe", "Create", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<TileID>("Terraria.Recipe", "AddTile", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Recipe", "AddIngredient", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Recipe", "HasResult", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.ModLoader.Mod", "CreateRecipe", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, float, float, float, float, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 5));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectileDirect(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<BuffID>("Terraria.Player", "AddBuff", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.NPC", "AddBuff", (ctx) => new MethodParameterBinding(ctx, 0));

			AddBinding<ItemID>("Terraria.ID.ItemID.Sets", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<NPCID>("Terraria.ID.NPCID.Sets", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<ProjectileID>("Terraria.ID.ProjectileID.Sets", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<TileID>("Terraria.ID.TileID.Sets", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<TileID>("Terraria.ID.TileID.Sets.Conversion", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<WallID>("Terraria.ID.WallID.Sets", "*", (ctx) => new FieldBinding(ctx));
			AddBinding<WallID>("Terraria.ID.WallID.Sets.Conversion", "*", (ctx) => new FieldBinding(ctx));
		}
	}

	private static void AddBinding<T>(string owningClassName, string memberName, Func<Binding.CreationContext, Binding> func, Type idType = null, bool allowNegativeIDs = false)
	{
		AddBinding(typeof(T), owningClassName, memberName, func, idType: idType, allowNegativeIDs: allowNegativeIDs);
	}

	private static void AddBinding(Type idClass, string owningClassName, string memberName, Func<Binding.CreationContext, Binding> func, Type idType = null, bool allowNegativeIDs = false)
	{
		if (!searchCache.TryGetValue(idClass, out var search)) {
			var field = idClass.GetField("Search", (BindingFlags)(-1));
			if (field != null) {
				Debug.Assert(idType == null, "This idClass has a Search IdDictionary, please remove the idType argument");
				search = (IdDictionary)field.GetValue(null);
			}
			else {
				Debug.Assert(idType != null, "idType must be provided for classes without a Search IdDictionary");
				search = IdDictionary.Create(idClass, idType);
			}

			Debug.Assert(search != null);
			searchCache[idClass] = search;
		}

		var context = new Binding.CreationContext(owningClassName, memberName, idClass.Name, idClass.FullName, search, AllowNegativeIDs: allowNegativeIDs);
		var binding = func(context);

		if (bindingByMemberByOwningClass.TryGetValue(owningClassName, out var bindingByMember)) {
			bindingByMember.Add(memberName, binding);
		}
		else {
			bindingByMemberByOwningClass[owningClassName] = new() { [memberName] = binding };
		}
	}

	public static bool TryGetBinding(ISymbol symbol, out Binding binding)
	{
		binding = null;

		static string BuildQualifiedName(ISymbol symbol)
		{
			return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
		}

		if (symbol is IFieldSymbol fieldSymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out Dictionary<string, Binding> bindingByMember)) {
			if (bindingByMember.TryGetValue(fieldSymbol.MetadataName, out binding)) {
				return true;
			}

			if (bindingByMember.TryGetValue("*", out binding)) {
				return true;
			}
		}
		else if (symbol is IPropertySymbol propertySymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingByMember)) {
			if (bindingByMember.TryGetValue(propertySymbol.MetadataName, out binding)) {
				return true;
			}
		}
		else if (symbol is IMethodSymbol methodSymbol && bindingByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingByMember)) {
			if (bindingByMember.TryGetValue(methodSymbol.ToDisplayString(MethodNameOnlyDisplayFormat), out binding)) {
				return true;
			}

			if (bindingByMember.TryGetValue(methodSymbol.ToDisplayString(MethodWithQualifiedParametersDisplayFormat), out binding)) {
				return true;
			}
		}
		else if (symbol is IParameterSymbol parameterSymbol) {
			if (TryGetBinding(parameterSymbol.ContainingSymbol, out binding) && parameterSymbol.Ordinal == ((MethodParameterBinding)binding).ParameterOrder) {
				return true;
			}
		}

		return false;
	}
}
