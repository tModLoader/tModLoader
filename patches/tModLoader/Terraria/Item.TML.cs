using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Terraria
{
	public partial class Item : TagSerializable, IEntityWithGlobals<GlobalItem>
	{
		public static readonly Func<TagCompound, Item> DESERIALIZER = ItemIO.Load;

		private int currentUseAnimationCompensation;

		public ModItem ModItem { get; internal set; }

		internal Instanced<GlobalItem>[] globalItems = Array.Empty<Instanced<GlobalItem>>();

		public RefReadOnlyArray<Instanced<GlobalItem>> Globals => new(globalItems);

		public List<Mod> StatsModifiedBy { get; private set; } = new();

		/// <summary>
		/// Dictates whether or not attack speed modifiers on this weapon will actually affect its use time.<br/>
		/// Defaults to false, which allows attack speed modifiers to affect use time. Set this to true to prevent this from happening.<br/>
		/// Used in vanilla by all melee weapons which shoot a projectile and have <see cref="noMelee"/> set to false.
		/// </summary>
		public bool attackSpeedOnlyAffectsWeaponAnimation { get; set; }

		/// <summary>
		/// Set to true in SetDefaults to allow this item to receive a prefix on reforge even if maxStack is not 1.
		/// <br>This prevents it from receiving a prefix on craft.</br>
		/// </summary>
		public bool AllowReforgeForStackableItem { get; set; }

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
		/// Used to make stackable items reforgeable
		/// </summary>
		public bool IsCandidateForReforge => maxStack == 1 || AllowReforgeForStackableItem;

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

		/// <summary> Gets the instance of the specified GlobalItem type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetGlobalItem<T>(bool exactType = true) where T : GlobalItem
			=> GlobalType.GetGlobal<Item, GlobalItem, T>(globalItems, exactType);

		/// <summary> Gets the local instance of the type of the specified GlobalItem instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetGlobalItem<T>(T baseInstance) where T : GlobalItem
			=> GlobalType.GetGlobal<Item, GlobalItem, T>(globalItems, baseInstance);

		/// <summary> Gets the instance of the specified GlobalItem type. </summary>
		public bool TryGetGlobalItem<T>(out T result, bool exactType = true) where T : GlobalItem
			=> GlobalType.TryGetGlobal(globalItems, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalItem instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalItem<T>(T baseInstance, out T result) where T : GlobalItem
			=> GlobalType.TryGetGlobal(globalItems, baseInstance, out result);

		public TagCompound SerializeData() => ItemIO.Save(this);

		public bool CountsAsClass<T>() where T : DamageClass
			=> CountsAsClass(ModContent.GetInstance<T>());

		public bool CountsAsClass(DamageClass damageClass)
			=> DamageClassLoader.effectInheritanceCache[DamageType.Type, damageClass.Type];

		// public version of IsNotTheSameAs for modders
		/// <summary>
		/// returns false if and only if netID (deprecated, equivalent to type), stack and prefix match
		/// </summary>
		public bool IsNotSameTypePrefixAndStack(Item compareItem) {
			if (netID == compareItem.netID && stack == compareItem.stack)
				return prefix != compareItem.prefix;

			return true;
		}

		internal static void PopulateMaterialCache() {
			for (int i = 0; i < Recipe.numRecipes; i++) {
				foreach (Item item in Main.recipe[i].requiredItem) {
					ItemID.Sets.IsAMaterial[item.type] = true;
				}
			}

			foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values) {
				foreach (var item in recipeGroup.ValidItems) {
					ItemID.Sets.IsAMaterial[item] = true;
				}
			}

			ItemID.Sets.IsAMaterial[71] = false;
			ItemID.Sets.IsAMaterial[72] = false;
			ItemID.Sets.IsAMaterial[73] = false;
			ItemID.Sets.IsAMaterial[74] = false;
		}

		/// <summary>
		/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
		/// <br/><br/>This particular overload uses a Rectangle instead of X, Y, Width, and Height to determine the actual spawn position.
		/// </summary>
		public static int NewItem(IEntitySource source, Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
			=> NewItem(source, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

		/// <summary>
		/// <inheritdoc cref="Item.NewItem(IEntitySource, int, int, int, int, int, int, bool, int, bool, bool)"/>
		/// <br/><br/>This particular overload uses a Vector2 instead of X, Y, Width, and Height to determine the actual spawn position.
		/// </summary>
		public static int NewItem(IEntitySource source, Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
			=> NewItem(source, (int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

		private void ApplyItemAnimationCompensationsToVanillaItems() {
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
		}

		private void UndoItemAnimationCompensations() {
			useAnimation -= currentUseAnimationCompensation;
			currentUseAnimationCompensation = 0;
		}

		private void RestoreMeleeSpeedBehaviorOnVanillaItems() {
			if (type < ItemID.Count && melee && shoot > 0 && !ItemID.Sets.Spears[type]) {
				if (noMelee)
					DamageType = DamageClass.MeleeNoSpeed;
				else
					attackSpeedOnlyAffectsWeaponAnimation = true;
			}
		}

		// Internal utility method. Move somewhere, if there's a better place.
		internal static void DropItem(IEntitySource source, Item item, Rectangle rectangle) {
			int droppedItemId = NewItem(source, rectangle, item.netID, 1, noBroadcast: true, prefixGiven: item.prefix);
			var droppedItem = Main.item[droppedItemId];

			droppedItem.ModItem = item.ModItem;
			droppedItem.globalItems = item.globalItems;

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(21, -1, -1, null, droppedItemId);
		}
	}
}