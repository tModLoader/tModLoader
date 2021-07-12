using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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

		public RefReadOnlyArray<Instanced<GlobalItem>> Globals => new RefReadOnlyArray<Instanced<GlobalItem>>(globalItems);

		private DamageClass _damageClass = DamageClass.Generic;
		/// <summary>
		/// The damage type of this Item. Assign to DamageClass.Melee/Ranged/Magic/Summon/Throwing for vanilla classes, or ModContent.GetInstance<T>() for custom damage types.
		/// </summary>
		public DamageClass DamageType {
			get => _damageClass;
			set => _damageClass = value ?? throw new ArgumentException("DamageType cannot be null");
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
			=> GlobalType.TryGetGlobal<GlobalItem, T>(globalItems, exactType, out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified GlobalItem instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetGlobalItem<T>(T baseInstance, out T result) where T : GlobalItem
			=> GlobalType.TryGetGlobal<GlobalItem, T>(globalItems, baseInstance, out result);

		public TagCompound SerializeData() => ItemIO.Save(this);

		public bool CountsAsClass<T>() where T : DamageClass
			=> CountsAsClass(ModContent.GetInstance<T>());

		public bool CountsAsClass(DamageClass damageClass)
			=> DamageClassLoader.countsAs[DamageType.Type, damageClass.Type];

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

		public static int NewItem(Rectangle rectangle, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
			=> NewItem(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

		public static int NewItem(Vector2 position, int Type, int Stack = 1, bool noBroadcast = false, int prefixGiven = 0, bool noGrabDelay = false, bool reverseLookup = false)
			=> NewItem((int)position.X, (int)position.Y, 0, 0, Type, Stack, noBroadcast, prefixGiven, noGrabDelay, reverseLookup);

		private void ApplyItemAnimationCompensations() {
			// Compensate for the change of itemAnimation getting reset at 0 instead of vanilla's 1.

			currentUseAnimationCompensation = 0;

			if (type < ItemID.Count && !noMelee) {
				useAnimation--;
				currentUseAnimationCompensation--;
			}
		}

		private void UndoItemAnimationCompensations() {
			useAnimation -= currentUseAnimationCompensation;
			currentUseAnimationCompensation = 0;
		}

		// Internal utility methods below. Move somewhere, if there's a better place.

		internal static void DropItem(Item item, Rectangle rectangle) {
			int droppedItemId = NewItem(rectangle, item.netID, 1, noBroadcast: true, prefixGiven: item.prefix);
			var droppedItem = Main.item[droppedItemId];

			droppedItem.ModItem = item.ModItem;
			droppedItem.globalItems = item.globalItems;

			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(21, -1, -1, null, droppedItemId);
		}
	}
}