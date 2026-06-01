using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.IO;

namespace Terraria;

public partial class Item : TagSerializable, IEntityWithGlobals<GlobalItem>
{
	public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

	private int currentUseAnimationCompensation;

	/// <summary>
	/// The ModItem instance attached to this Item. All modded items will have an appropriate instance of the ModItem-inheriting class assigned to this field. Will be null for vanilla items. Can be used to check if an Item is a modded item or not.
	/// <para/> If you need to access members of a specific ModItem class from code outside of the ModItem, you can cast to your class:
	/// <code>
	/// if (item.ModItem is MyItem myItem)
	/// {
	///     myItem.myField = 5;
	/// }
	/// </code>
	/// </summary>
	public ModItem ModItem { get; internal set; }

#region Globals
	int IEntityWithGlobals<GlobalItem>.Type => type;
	internal GlobalItem[] _globals;
	public RefReadOnlyArray<GlobalItem> EntityGlobals => _globals;
	public EntityGlobalsEnumerator<GlobalItem> Globals => new(this);

	/// <summary> Gets the instance of the specified GlobalItem type. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="IndexOutOfRangeException"/>
	public T GetGlobalItem<T>() where T : GlobalItem
		=> GlobalItem.GetGlobal<T>(type, EntityGlobals);

	/// <summary> Gets the local instance of the type of the specified GlobalItem instance. This will throw exceptions on failure. </summary>
	/// <exception cref="KeyNotFoundException"/>
	/// <exception cref="NullReferenceException"/>
	public T GetGlobalItem<T>(T baseInstance) where T : GlobalItem
		=> GlobalItem.GetGlobal<T>(type, EntityGlobals, baseInstance);

	/// <summary> Gets the instance of the specified GlobalItem type. </summary>
	public bool TryGetGlobalItem<T>(out T result) where T : GlobalItem
		=> GlobalItem.TryGetGlobal(type, EntityGlobals, out result);

	/// <summary> Safely attempts to get the local instance of the type of the specified GlobalItem instance. </summary>
	/// <returns> Whether or not the requested instance has been found. </returns>
	public bool TryGetGlobalItem<T>(T baseInstance, out T result) where T : GlobalItem
		=> GlobalItem.TryGetGlobal(type, EntityGlobals, baseInstance, out result);
#endregion

	public List<Mod> StatsModifiedBy { get; private set; } = new();

	/// <summary>
	/// An additional identifier
	/// </summary>
	public int NetStateVersion { get; private set; }

	/// <summary>
	/// Call this to trigger a re-sync of this item in a player's inventory or equipment in multiplayer.<br/>
	/// The item will be sent to the server and other players at the end of the frame (not immediately).<br/>
	///<br/>
	/// Has no effect on server-side items or items in remote player's inventories<br/>
	/// </summary>
	public void NetStateChanged() => NetStateVersion++;

	/// <summary>
	/// Dictates whether or not attack speed modifiers on this weapon will actually affect its use time.<br/>
	/// Defaults to false, which allows attack speed modifiers to affect use time. Set this to true to prevent this from happening.<br/>
	/// Used in vanilla by all melee weapons which shoot a projectile and have <see cref="noMelee"/> set to false.<para/>
	/// Tools should typically set this to true to prevent the tool from mining quicker when the player has attack speed modifiers. By convention tool speed should not be affected by attack speed. They will still attack quicker, but their use time (mine/hammer/axe speed) should not be affected.
	/// </summary>
	public bool attackSpeedOnlyAffectsWeaponAnimation { get; set; }

	/// <summary>
	/// Set to true in SetDefaults to allow this item to receive a prefix on reforge even if maxStack is not 1.
	/// <br>This prevents it from receiving a prefix on craft.</br>
	/// </summary>
	public bool AllowReforgeForStackableItem { get; set; }

	/// <summary>
	/// Similar to useTurn, but only allows turning when an animation starts (eg between swings). Many early-game vanilla swords use this as it makes them clunky and hard to kite with.
	/// <br/> Defaults to <see langword="false"/>.
	/// </summary>
	public bool useTurnOnAnimationStart { get; set; }

	/// <summary>
	/// Dictates the amount of times a weapon can be used (shot, etc) each time it animates (is swung, clicked, etc).<br/>
	/// Defaults to null.<br/>
	/// Used in vanilla by the following:<br/>
	/// - BookStaff<br/>
	/// - FairyQueenMagicItem<br/>
	/// - FairyQueenRangedItem<br/>
	/// </summary>
	public int? useLimitPerAnimation { get; set; }

	/// <summary>
	/// Dictates whether or not this item should only consume ammo on its first shot of each use.<br/>
	/// Defaults to false.<br/>
	/// Used in vanilla by the following:<br/>
	/// - Flamethrower<br/>
	/// - Elf Melter<br/>
	/// </summary>
	public bool consumeAmmoOnFirstShotOnly { get; set; }

	/// <summary>
	/// Dictates whether or not this item should only consume ammo on its last shot of each use.<br/>
	/// Defaults to false. <br/>
	/// Used in vanilla by the following:<br/>
	/// - ClockworkAssaultRifle<br/>
	/// - Clentaminator<br/>
	/// - FairyQueenRangedItem<br/>
	/// </summary>
	public bool consumeAmmoOnLastShotOnly { get; set; }

	/// <summary>
	/// When enabled and the player is hurt, <see cref="Player.channel"/> will be set to false
	/// </summary>
	public bool InterruptChannelOnHurt { get; set; }

	/// <summary>
	/// When enabled and the player is hurt, <see cref="Player.channel"/> will be set to false, and the item animation will stop immediately
	/// </summary>
	public bool StopAnimationOnHurt { get; set; }

	/// <summary>
	/// When true, shooting any projectile from this item will make the owner face the projectile. Defaults to true.<br/>
	/// The only 2 vanilla items that change this from true to false are the Grand Design and Beam Sword<br/>
	/// This is different to <see cref="Item.useTurn"/>. Item.useTurn will prevent the player from changing their direction while the animation is playing if it is set to true.<br/>
	/// </summary>
	public bool ChangePlayerDirectionOnShoot { get; set; } = true;

	private DamageClass _damageClass = DamageClass.Default;
	/// <summary>
	/// The damage type of this Item. Assign to DamageClass.Melee/Ranged/Magic/Summon/Throwing for vanilla classes, or <see cref="ModContent.GetInstance"/> for custom damage types.
	/// </summary>
	public DamageClass DamageType {
		get => _damageClass;
		set => _damageClass = value ?? throw new ArgumentException("An item's DamageType cannot be null.");
	}

	private int _armorPenetration = 0;
	/// <summary>
	/// The number of defense points that this item can ignore on its own. Cannot be set to negative values. Defaults to 0.
	/// </summary>
	public int ArmorPenetration {
		get => _armorPenetration;
		set => _armorPenetration = Math.Max(0, value);
	}

	public TagCompound SerializeData() => ItemIO.Save(this);

	/// <inheritdoc cref="CountsAsClass"/>
	public bool CountsAsClass<T>() where T : DamageClass
		=> CountsAsClass(ModContent.GetInstance<T>());

	/// <summary>
	/// This is used to check if this item benefits from the specified <see cref="DamageClass"/>.
	/// </summary>
	/// <param name="damageClass">The DamageClass to check for in this item.</param>
	/// <returns><see langword="true"/> if this item's <see cref="DamageClass"/> matches <paramref name="damageClass"/>, <see langword="false"/> otherwise</returns>
	public bool CountsAsClass(DamageClass damageClass)
		=> DamageClassLoader.effectInheritanceCache[DamageType.Type, damageClass.Type];

	/// <summary>
	/// returns false if and only if type, stack and prefix match<br/>
	/// <seealso cref="IsNetStateDifferent(Item)"/>
	/// </summary>
	public bool IsNotSameTypePrefixAndStack(Item compareItem) => type != compareItem.type || stack != compareItem.stack || prefix != compareItem.prefix;

	/// <summary>
	/// Returns true if these items are different and there is a need to re-sync them
	/// </summary>
	public bool IsNetStateDifferent(Item compareItem) => type != compareItem.type || stack != compareItem.stack || prefix != compareItem.prefix || NetStateVersion != compareItem.NetStateVersion;

	/// <summary>
	/// Use this instead of <see cref="Clone"/> for much faster state snapshotting and change sync detection.<br/>
	/// Note!! <see cref="SetDefaults(int)"/> will NOT be called. The target item will remain as it was (most likely air), except for type, stack, prefix and netStateVersion
	/// </summary>
	public void CopyNetStateTo(Item target)
	{
		target.type = type;
		target.stack = stack;
		target.prefix = prefix;
		target.NetStateVersion = NetStateVersion;
	}

	[ThreadStatic]
	private static string cloningDisabled = null;
	public ref struct DisableCloneMethod
	{
		public DisableCloneMethod(string msg) => cloningDisabled = msg;
		public void Dispose() => cloningDisabled = null;
	}

	[ThreadStatic]
	private static bool newItemDisabled = false;
	// Used to disable NewItem in situations that would result in an undesirable amount of patches.
	internal ref struct DisableNewItemMethod
	{
		internal DisableNewItemMethod(bool disabled) => newItemDisabled = disabled;
		internal void Dispose() => newItemDisabled = false;
	}

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses a Rectangle instead of X, Y, Width, and Height to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses a Vector2 instead of X, Y, Width, and Height to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, (int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses an Item instead of just the item type. All modded data will be preserved.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, int X, int Y, int Width, int Height, Item item, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false)
		=> Item.NewItem_Inner(source, X, Y, Width, Height, item, item.type, item.stack, noBroadcast, item.prefix, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses an Item instead of just the item type. All modded data will be preserved.
	/// <br/><br/>This particular overload uses a Vector2 instead of X and Y to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Vector2 pos, Vector2 randomBox, Item item, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, (int)pos.X, (int)pos.Y, (int)randomBox.X, (int)randomBox.Y, item, noBroadcast, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses an Item instead of just the item type. All modded data will be preserved.
	/// <br/><br/>This particular overload uses a Vector2 instead of X and Y to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Vector2 pos, int Width, int Height, Item item, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, (int)pos.X, (int)pos.Y, Width, Height, item, noBroadcast, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses an Item instead of just the item type. All modded data will be preserved.
	/// <br/><br/>This particular overload uses a Vector2 instead of X, Y, Width, and Height to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Vector2 position, Item item, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, (int)position.X, (int)position.Y, 0, 0, item, noBroadcast, noGrabDelay, reverseLookup);

	/// <summary>
	/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
	/// <br/><br/>This particular overload uses an Item instead of just the item type. All modded data will be preserved.
	/// <br/><br/>This particular overload uses a Rectangle instead of X, Y, Width, and Height to determine the actual spawn position.
	/// </summary>
	/// <returns><inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/></returns>
	public static int NewItem(IEntitySource source, Rectangle rectangle, Item item, bool noBroadcast = false, bool noGrabDelay = false, bool reverseLookup = false)
		=> NewItem(source, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, item, noBroadcast, noGrabDelay, reverseLookup);

	private void ApplyItemAnimationCompensationsToVanillaItems()
	{
		// #2351
		// Compensate for the change of itemAnimation getting reset at 0 instead of vanilla's 1.
		// all items with autoReuse in vanilla are affected, but the animation only has a physical effect for !noMelee items
		// for those items, we want the faster animation as that governs reuse time as dps is determined by swing speed.
		// for the others like ranged weapons, it's fine to keep the animation matching the use time, as dps is determined by item use speed
		currentUseAnimationCompensation = 0;

		if (type < ItemID.Count && autoReuse && !noMelee) {
			useAnimation--;
			currentUseAnimationCompensation--;
		}

		// in vanilla, items without autoReuse get a frame where itemAnimation == 0 between uses allowing the direction change checks in HorizontalMovement to apply
		// in tML we need to explicitly enable this behavior
		if (type < ItemID.Count && useStyle != 0 && !autoReuse && !useTurn && shoot == 0 && damage > 0) {
			useTurnOnAnimationStart = true;
		}
	}

	private void UndoItemAnimationCompensations()
	{
		useAnimation -= currentUseAnimationCompensation;
		currentUseAnimationCompensation = 0;
	}

	private void RestoreMeleeSpeedBehaviorOnVanillaItems()
	{
		bool isTool = pick > 0 || axe > 0 || hammer > 0 || type == ItemID.GravediggerShovel;
		if (type < ItemID.Count && melee && (shoot > 0 && !ItemID.Sets.Spears[type] && !shootsEveryUse || isTool)) {
			if (noMelee)
				DamageType = DamageClass.MeleeNoSpeed;
			else
				attackSpeedOnlyAffectsWeaponAnimation = true;
		}
	}

	// Added by TML.
	/// <summary>
	/// Determines the <see cref="PrefixCategory">prefix categories</see> of this <see cref="Item"/>.
	/// </summary>
	/// <returns>A <see cref="List{PrefixCategory}"/> of every category this <see cref="Item"/> matches, the <see cref="List{PrefixCategory}"/> will be empty if this <see cref="Item"/> doesn't have any categories.</returns>
	public List<PrefixCategory> GetPrefixCategories() {
		ref List<PrefixCategory> categories = ref PrefixLoader.itemPrefixesByType[type];
		if (categories is not null) return categories;
		categories = [];
		if (PrefixLegacy.ItemSets.SwordsHammersAxesPicks[type] || ItemLoader.MeleePrefix(this))
			categories.Add(PrefixCategory.Melee);

		if (PrefixLegacy.ItemSets.GunsBows[type] || ItemLoader.RangedPrefix(this))
			categories.Add(PrefixCategory.Ranged);

		if (PrefixLegacy.ItemSets.MagicAndSummon[type] || ItemLoader.MagicPrefix(this))
			categories.Add(PrefixCategory.Magic);

		if (PrefixLegacy.ItemSets.SpearsMacesChainsawsDrillsPunchCannon[type] || PrefixLegacy.ItemSets.BoomerangsChakrams[type] || PrefixLegacy.ItemSets.ItemsThatCanHaveLegendary2[type] || ItemLoader.WeaponPrefix(this) || categories.Count != 0)
			categories.Add(PrefixCategory.AnyWeapon);

		if (IsAPrefixableAccessory())
			categories.Add(PrefixCategory.Accessory);

		return categories;
	}
}