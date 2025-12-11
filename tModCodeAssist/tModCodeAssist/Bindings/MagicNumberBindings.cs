using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

		public abstract bool AppliesTo(ISymbol symbol);
	}

	// Designates a field or property's type as an ID type.
	private sealed class FieldOrPropertyBinding(Binding.CreationContext context) : Binding(context)
	{
		// No need to check whether names match; we need to support wildcard
		// operators ('*') and filtering is handled prior to this call.
		public override bool AppliesTo(ISymbol symbol) => symbol is IFieldSymbol or IPropertySymbol;
	}

	// Designates a method's return type as an ID type.
	private sealed class MethodReturnBinding(Binding.CreationContext context) : Binding(context)
	{
		// No need to check whether names match; filtering is handled prior to
		// this call.
		// Be sure to filter out parameter symbols.
		public override bool AppliesTo(ISymbol symbol) => symbol is IMethodSymbol;
	}

	// Designates a method parameter's type as an ID type.
	private sealed class MethodParameterBinding(Binding.CreationContext context, int parameterOrdinal) : Binding(context)
	{
		public int ParameterOrdinal => parameterOrdinal;

		// We only want to match against parameters of the same ordinal.
		// No need to check whether the method name is the same; filtering is
		// handled prior to this call.
		public override bool AppliesTo(ISymbol symbol) =>
			symbol is IParameterSymbol parameterSymbol && parameterSymbol.Ordinal == ParameterOrdinal;
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
	private static ConcurrentDictionary<string, Dictionary<string, List<Binding>>> bindingsByMemberByOwningClass;

	public static void PopulateBindings()
	{
		lock (@lock) {
			if (bindingsByMemberByOwningClass != null)
				return;

			searchCache = [];
			bindingsByMemberByOwningClass = [];

			AddBinding<TileID>("Terraria.Item", "createTile", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ItemID>("Terraria.Item", "type", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ItemID>("Terraria.Player", "cursorItemIconID", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ProjectileID>("Terraria.Item", "shoot", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ProjectileID>("Terraria.ModLoader.ModProjectile", "AIType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ItemUseStyleID>("Terraria.Item", "useStyle", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding(typeof(ItemRarityID), "Terraria.Item", "rare", (ctx) => new FieldOrPropertyBinding(ctx), allowNegativeIDs: true);
			AddBinding<NPCAIStyleID>("Terraria.NPC", "aiStyle", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<NPCID>("Terraria.NPC", "type", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<NPCID>("Terraria.ModLoader.ModNPC", "AIType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<NPCID>("Terraria.ModLoader.ModNPC", "AnimationType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding(typeof(NetmodeID), "Terraria.Main", "netMode", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ProjAIStyleID>("Terraria.Projectile", "aiStyle", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ProjectileID>("Terraria.Projectile", "type", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<DustID>("Terraria.ModLoader.ModBlockType", "DustType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<DustID>("Terraria.ModLoader.ModDust", "UpdateType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<TileID>("Terraria.Tile", "TileType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<WallID>("Terraria.Tile", "WallType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding(typeof(PaintID), "Terraria.Tile", "TileColor", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding(typeof(PaintID), "Terraria.Tile", "WallColor", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding(typeof(LiquidID), "Terraria.Tile", "LiquidType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ExtrasID>("Terraria.GameContent.TextureAssets", "Extra", (ctx) => new FieldOrPropertyBinding(ctx), typeof(short));
			AddBinding<MountID>("Terraria.Item", "mountType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<MountID>("Terraria.Mount", "Type", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<BuffID>("Terraria.Item", "buffType", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<BuffID>("Terraria.Mount.MountData", "buff", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<BuffID>("Terraria.Mount", "BuffType", (ctx) => new FieldOrPropertyBinding(ctx));

			AddBinding<ItemID>("Terraria.Item", "CloneDefaults", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Item", "netDefaults", (ctx) => new MethodParameterBinding(ctx, 0), allowNegativeIDs: true);
			AddBinding<ItemID>("Terraria.Item", "SetDefaults(int)", (ctx) => new MethodParameterBinding(ctx, 0), allowNegativeIDs: true);
			AddBinding<ItemID>("Terraria.Item", "SetDefaults(int, bool, Terraria.GameContent.Items.ItemVariant)", (ctx) => new MethodParameterBinding(ctx, 0), allowNegativeIDs: true);
			AddBinding<MessageID>("Terraria.NetMessage", "SendData", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<DustID>("Terraria.Dust", "NewDust", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<DustID>("Terraria.Dust", "NewDustDirect", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<DustID>("Terraria.Dust", "NewDustPerfect", (ctx) => new MethodParameterBinding(ctx, 1));
			AddBinding<ItemID>("Terraria.Player", "CountItem", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "ConsumeItem", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "FindItem(int, Terraria.Item[])", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "FindItemInInventoryOrOpenVoidBag", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "HasItem(int)", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "HasItem(int, Terraria.Item[])", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "HasItemInInventoryOrOpenVoidBag", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "HasItemInAnyInventory", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "OpenBossBag", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "PutItemInInventoryFromItemUsage", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "StatusToNPC", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "StatusToPlayerPvP", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Player", "QuickSpawnItem(Terraria.DataStructures.IEntitySource, int, int)", (ctx) => new MethodParameterBinding(ctx, 1));
			AddBinding<ItemID>("Terraria.Player", "QuickSpawnItemDirect(Terraria.DataStructures.IEntitySource, int, int)", (ctx) => new MethodParameterBinding(ctx, 1));
			AddBinding<ItemID>("Terraria.Recipe", "Create", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<TileID>("Terraria.Recipe", "AddTile", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Recipe", "AddIngredient", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.Recipe", "HasResult", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<ItemID>("Terraria.ModLoader.Mod", "CreateRecipe", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<NPCID>("Terraria.Player", "isNearNPC", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<NPCID>("Terraria.NPC", "NewNPC", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<NPCID>("Terraria.NPC", "NewNPCDirect(Terraria.DataStructures.IEntitySource, int, int, int, int, float, float, float, float, int)", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<NPCID>("Terraria.NPC", "NewNPCDirect(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, int, int, float, float, float, float, int)", (ctx) => new MethodParameterBinding(ctx, 2));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectile(Terraria.DataStructures.IEntitySource, float, float, float, float, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 5));
			AddBinding<ProjectileID>("Terraria.Projectile", "NewProjectileDirect(Terraria.DataStructures.IEntitySource, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float, int, float, float, float)", (ctx) => new MethodParameterBinding(ctx, 3));
			AddBinding<BuffID>("Terraria.Player", "AddBuff", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.Player", "ClearBuff", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.Player", "FindBuffIndex", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.Player", "HasBuff(int)", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.NPC", "AddBuff", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.NPC", "FindBuffIndex", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<BuffID>("Terraria.NPC", "HasBuff(int)", (ctx) => new MethodParameterBinding(ctx, 0));
			AddBinding<RecipeGroupID>("Terraria.Recipe", "AddRecipeGroup(int, int)", (ctx) => new MethodParameterBinding(ctx, 0), idType: typeof(int)); // RecipeGroupID is a little strange, the values aren't actually correct, that is why it currently doesn't have a Search IdDictionary.

			AddBinding<ItemID>("Terraria.ID.ItemID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<NPCID>("Terraria.ID.NPCID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<ProjectileID>("Terraria.ID.ProjectileID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<TileID>("Terraria.ID.TileID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<TileID>("Terraria.ID.TileID.Sets.Conversion", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<WallID>("Terraria.ID.WallID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<WallID>("Terraria.ID.WallID.Sets.Conversion", "*", (ctx) => new FieldOrPropertyBinding(ctx));
			AddBinding<MountID>("Terraria.ID.MountID.Sets", "*", (ctx) => new FieldOrPropertyBinding(ctx));
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

		if (bindingsByMemberByOwningClass.TryGetValue(owningClassName, out var bindingsByMember)) {
			if (!bindingsByMember.TryGetValue(memberName, out var bindings))
				bindingsByMember[memberName] = bindings = [];

			bindings.Add(binding);
		}
		else {
			bindingsByMemberByOwningClass[owningClassName] = new() { [memberName] = [binding] };
		}
	}

	public static bool TryGetBindings(ISymbol symbol, out List<Binding> bindings)
	{
		bindings = null;

		if (symbol is IFieldSymbol fieldSymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out var bindingsByMember)) {
			if (bindingsByMember.TryGetValue(fieldSymbol.MetadataName, out bindings)) {
				return true;
			}

			if (bindingsByMember.TryGetValue("*", out bindings)) {
				return true;
			}
		}
		else if (symbol is IPropertySymbol propertySymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingsByMember)) {
			if (bindingsByMember.TryGetValue(propertySymbol.MetadataName, out bindings)) {
				return true;
			}
		}
		else if (symbol is IMethodSymbol methodSymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingsByMember)) {
			if (bindingsByMember.TryGetValue(methodSymbol.ToDisplayString(MethodNameOnlyDisplayFormat), out bindings)) {
				return true;
			}

			if (bindingsByMember.TryGetValue(methodSymbol.ToDisplayString(MethodWithQualifiedParametersDisplayFormat), out bindings)) {
				return true;
			}
		}
		else if (symbol is IParameterSymbol parameterSymbol) {
			if (TryGetBindings(parameterSymbol.ContainingSymbol, out bindings)) {
				return true;
			}
		}

		return false;
	}

	public static bool TryGetBinding(ISymbol symbol, out Binding binding)
	{
		if (!TryGetBindings(symbol, out var bindings)) {
			binding = null;
			return false;
		}

		return TryFindBinding(symbol, bindings, out binding);
	}

	// Whether the symbol or symbols within it (e.g. parameters of methods)
	// contain any bindings.
	public static bool HasBindingsForSymbol(ISymbol symbol)
	{
		if (symbol is IFieldSymbol fieldSymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out var bindingsByMember)) {
			return bindingsByMember.ContainsKey(fieldSymbol.MetadataName)
				|| bindingsByMember.ContainsKey("*");
		}
		else if (symbol is IPropertySymbol propertySymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingsByMember)) {
			return bindingsByMember.ContainsKey(propertySymbol.MetadataName);
		}
		else if (symbol is IMethodSymbol methodSymbol && bindingsByMemberByOwningClass.TryGetValue(BuildQualifiedName(symbol.ContainingType), out bindingsByMember)) {
			// This includes parameters!  They only get filtered out in
			// TryGetBinding(s).
			return bindingsByMember.ContainsKey(methodSymbol.ToDisplayString(MethodNameOnlyDisplayFormat))
				|| bindingsByMember.ContainsKey(methodSymbol.ToDisplayString(MethodWithQualifiedParametersDisplayFormat));
		}
		else if (symbol is IParameterSymbol parameterSymbol) {
			// The parameter case is a bit of an edge case in this context but
			// we can support it by treating it identically to its TryGet case.
			return TryGetBindings(parameterSymbol, out _);
		}

		return false;
	}

	private static string BuildQualifiedName(ISymbol symbol)
	{
		return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted));
	}

	private static bool TryFindBinding(ISymbol symbol, IEnumerable<Binding> bindings, out Binding binding)
	{
		binding = bindings.FirstOrDefault(x => x.AppliesTo(symbol));
		return binding is not null;
	}
}
