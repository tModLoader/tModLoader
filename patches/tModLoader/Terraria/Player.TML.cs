using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria
{
	public partial class Player
	{
		internal IList<string> usedMods;
		internal ModPlayer[] modPlayers = Array.Empty<ModPlayer>();
		public Item equippedWings = null;

		public HashSet<int> NearbyModTorch { get; private set; } = new HashSet<int>();

		// Get

		/// <summary> Gets the instance of the specified ModPlayer type. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		public T GetModPlayer<T>() where T : ModPlayer
			=> GetModPlayer(ModContent.GetInstance<T>());

		/// <summary> Gets the local instance of the type of the specified ModPlayer instance. This will throw exceptions on failure. </summary>
		/// <exception cref="KeyNotFoundException"/>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public T GetModPlayer<T>(T baseInstance) where T : ModPlayer
			=> modPlayers[baseInstance.index] as T ?? throw new KeyNotFoundException($"Instance of '{typeof(T).Name}' does not exist on the current player.");

		// TryGet

		/// <summary> Gets the instance of the specified ModPlayer type. </summary>
		public bool TryGetModPlayer<T>(out T result) where T : ModPlayer
			=> TryGetModPlayer(ModContent.GetInstance<T>(), out result);

		/// <summary> Safely attempts to get the local instance of the type of the specified ModPlayer instance. </summary>
		/// <returns> Whether or not the requested instance has been found. </returns>
		public bool TryGetModPlayer<T>(T baseInstance, out T result) where T : ModPlayer {
			if (baseInstance == null || baseInstance.index < 0 || baseInstance.index >= modPlayers.Length) {
				result = default;

				return false;
			}

			result = modPlayers[baseInstance.index] as T;

			return result != null;
		}

		/// <summary>
		/// Will spawn an item like QuickSpawnItem, but clones it (handy when you need to retain item infos)
		/// </summary>
		/// <param name="item">The item you want to be cloned</param>
		/// <param name="stack">The stack to give the item. Note that this will override maxStack if it's higher.</param>
		public int QuickSpawnClonedItem(Item item, int stack = 1) {
			int index = Item.NewItem((int)position.X, (int)position.Y, width, height, item.type, stack, false, -1, false, false);
			Item clone = Main.item[index] = item.Clone();
			clone.whoAmI = index;
			clone.position = position;
			clone.stack = stack;

			// Sync the item for mp
			if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f, 0f, 0f, 0, 0, 0);

			return index;
		}

		public int QuickSpawnItem(Item item, int stack = 1) => QuickSpawnItem(item.type, stack);

		/// <inheritdoc cref="QuickSpawnClonedItem"/>
		public Item QuickSpawnClonedItemDirect(Item item, int stack = 1) => Main.item[QuickSpawnClonedItem(item, stack)];

		public Item QuickSpawnItemDirect(Item item, int stack = 1) => Main.item[QuickSpawnItem(item.type, stack)];

		public Item QuickSpawnItemDirect(int type, int stack = 1) => Main.item[QuickSpawnItem(type, stack)];

		/// <summary> Returns whether or not this Player currently has a (de)buff of the provided type. </summary>
		public bool HasBuff(int type) => FindBuffIndex(type) != -1;

		/// <inheritdoc cref="HasBuff(int)" />
		public bool HasBuff<T>() where T : ModBuff
			=> HasBuff(ModContent.BuffType<T>());

		// Damage Classes

		private DamageClassData[] damageData;

		internal void ResetDamageClassData() {
			damageData = new DamageClassData[DamageClassLoader.DamageClassCount];

			for (int i = 0; i < damageData.Length; i++) {
				damageData[i] = new DamageClassData(StatModifier.One, 0, StatModifier.One);
				DamageClassLoader.DamageClasses[i].SetDefaultStats(this);
			}
		}


		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref int GetCritChance<T>() where T : DamageClass => ref GetCritChance(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage<T>() where T : DamageClass => ref GetDamage(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback<T>() where T : DamageClass => ref GetKnockback(ModContent.GetInstance<T>());

		/// <summary>
		/// Gets the crit modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref int GetCritChance(DamageClass damageClass) => ref damageData[damageClass.Type].critChance;

		/// <summary>
		/// Gets the damage modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetDamage(DamageClass damageClass) => ref damageData[damageClass.Type].damage;

		/// <summary>
		/// Gets the knockback modifier for this damage type on this player.
		/// This returns a reference, and as such, you can freely modify this method's return value with operators.
		/// </summary>
		public ref StatModifier GetKnockback(DamageClass damageClass) => ref damageData[damageClass.Type].knockback;

		/// <summary>
		/// Container for current SceneEffect client properties such as: Backgrounds, music, and water styling
		/// </summary>
		public SceneEffectLoader.SceneEffectInstance CurrentSceneEffect { get; set; } = new SceneEffectLoader.SceneEffectInstance();

		/// <summary>
		/// Stores whether or not the player is in a modbiome using boolean bits.
		/// </summary>
		internal BitArray modBiomeFlags = new BitArray(0);

		/// <summary>
		/// Determines if the player is in specified ModBiome. This will throw exceptions on failure.
		/// </summary>
		/// <exception cref="IndexOutOfRangeException"/>
		/// <exception cref="NullReferenceException"/>
		public bool InModBiome(ModBiome baseInstance) => modBiomeFlags[baseInstance.ZeroIndexType];

		/// <summary>
		/// The zone property storing if the player is not in any particular biome. Updated in <see cref="UpdateBiomes"/>
		/// Does NOT account for height. Please use ZoneForest / ZoneNormalX for height based derivatives.
		/// </summary>
		public bool ZonePurity { get; set; } = false;

		/// <summary>
		/// Calculates whether or not the player is in the purity/forest biome.
		/// </summary>
		public bool InZonePurity() {
			bool one = ZoneBeach || ZoneCorrupt || ZoneCrimson || ZoneDesert || ZoneDungeon || ZoneGemCave;
			bool two = ZoneGlowshroom || ZoneGranite || ZoneGraveyard || ZoneHallow || ZoneHive || ZoneJungle;
			bool three = ZoneLihzhardTemple || ZoneMarble || ZoneMeteor || ZoneSnow || ZoneUnderworldHeight;
			bool four = modBiomeFlags.Cast<bool>().Contains(true);
			return !(one || two || three || four);
		}

		// Convenience Zone properties for Modders
		
		/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneOverworldHeight</code></summary>
		public bool ZoneForest => ZonePurity && ZoneOverworldHeight;
		
		/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneRockLayerHeight</code></summary>
		public bool ZoneNormalCaverns => ZonePurity && ZoneRockLayerHeight;
		
		/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneDirtLayerHeight</code></summary>
		public bool ZoneNormalUnderground => ZonePurity && ZoneDirtLayerHeight;
		
		/// <summary> Shorthand for <code>ZonePurity &amp;&amp; ZoneSkyHeight</code></summary>
		public bool ZoneNormalSpace => ZonePurity && ZoneSkyHeight;

		/// <summary>
		/// Invoked at the end of loading vanilla player data from files to fix stuff that isn't initialized coming out of load.
		/// Only run on the Player select screen during loading of data.
		/// Primarily meant to prevent unwarranted first few frame fall damage/lava damage if load lagging
		/// Corrects the player.lavaMax time, wingsLogic, and no fall dmg to be accurate for the provided items in accessory slots.
		/// </summary>
		public static void LoadPlayer_LastMinuteFixes(Item item, Player newPlayer) {
			int type = item.type;
			if (type == 908 || type == 4874 || type == 5000)
				newPlayer.lavaMax += 420;

			if (type == 906 || type == 4038)
				newPlayer.lavaMax += 420;

			if (newPlayer.wingsLogic == 0 && item.wingSlot >= 0) {
				newPlayer.wingsLogic = item.wingSlot;
				newPlayer.equippedWings = item;
			}

			if (type == 158 || type == 396 || type == 1250 || type == 1251 || type == 1252)
				newPlayer.noFallDmg = true;

			newPlayer.lavaTime = newPlayer.lavaMax;
		}

		/// <summary>
		/// Invoked in UpdateVisibleAccessories. Runs common code for both modded slots and vanilla slots based on provided Items.
		/// </summary>
		public void UpdateVisibleAccessories(Item item, bool invisible, int slot = -1, bool modded = false) {
			if (eocDash > 0 && shield == -1 && item.shieldSlot != -1) {
				shield = item.shieldSlot;
				if (cShieldFallback != -1)
					cShield = cShieldFallback;
			}

			if (shieldRaised && shield == -1 && item.shieldSlot != -1) {
				shield = item.shieldSlot;
				if (cShieldFallback != -1)
					cShield = cShieldFallback;
			}

			if (ItemIsVisuallyIncompatible(item))
				return;

			if (item.wingSlot > 0) {
				if (invisible && (velocity.Y == 0f || mount.Active))
					return;

				wings = item.wingSlot;
			}

			if (!invisible)
				UpdateVisibleAccessory(slot, item, modded);
		}

		/// <summary>
		/// Drops the ref'd item from the player at the position, and than turns the ref'd Item to air.
		/// </summary>
		public void DropItem(Vector2 position, ref Item item) {
			if (item.stack > 0) {
				int num3 = Item.NewItem((int)position.X, (int)position.Y, width, height, item.type);
				Main.item[num3].netDefaults(item.netID);
				Main.item[num3].Prefix(item.prefix);
				Main.item[num3].stack = item.stack;
				Main.item[num3].velocity.Y = (float)Main.rand.Next(-20, 1) * 0.2f;
				Main.item[num3].velocity.X = (float)Main.rand.Next(-20, 21) * 0.2f;
				Main.item[num3].noGrabDelay = 100;
				Main.item[num3].newAndShiny = false;
				Main.item[num3].ModItem = item.ModItem;
				Main.item[num3].globalItems = item.globalItems;
				if (Main.netMode == 1)
					NetMessage.SendData(21, -1, -1, null, num3);
			}

			item.TurnToAir();
		}

		public int GetHealLife(Item item, bool quickHeal = false) {
			int healValue = item.healLife;
			ItemLoader.GetHealLife(item, this, quickHeal, ref healValue);
			PlayerLoader.GetHealLife(this, item, quickHeal, ref healValue);
			return healValue > 0 ? healValue : 0;
		}

		public int GetHealMana(Item item, bool quickHeal = false) {
			int healValue = item.healMana;
			ItemLoader.GetHealMana(item, this, quickHeal, ref healValue);
			PlayerLoader.GetHealMana(this, item, quickHeal, ref healValue);
			return healValue > 0 ? healValue : 0;
		}

		public bool CanBuyItem(int price, int customCurrency = -1) {
			if (customCurrency != -1)
				return CustomCurrencyManager.BuyItem(this, price, customCurrency);

			long num = Utils.CoinsCount(out _, inventory, new[] { 58, 57, 56, 55, 54 });
			long num2 = Utils.CoinsCount(out _, bank.item, Array.Empty<int>());
			long num3 = Utils.CoinsCount(out _, bank2.item, Array.Empty<int>());
			long num4 = Utils.CoinsCount(out _, bank3.item, Array.Empty<int>());

			long num5 = Utils.CoinsCombineStacks(out _, new[] { num, num2, num3, num4 });

			return num5 >= price;
		}

		public int GetManaCost(Item item) {
			float reduce = manaCost;
			float mult = 1;
			// TODO: Make a space gun set
			if (spaceGun && (item.type == ItemID.SpaceGun || item.type == ItemID.ZapinatorGray || item.type == ItemID.ZapinatorOrange))
				mult = 0;

			if(item.type == ItemID.BookStaff && altFunctionUse == 2)
				mult = 2;

			CombinedHooks.ModifyManaCost(this, item, ref reduce, ref mult);
			int mana = (int)(item.mana * reduce * mult);
			return mana >= 0 ? mana : 0;
		}

		public bool CheckMana(Item item, int amount = -1, bool pay = false, bool blockQuickMana = false) {
			if (amount <= -1)
				amount = GetManaCost(item);

			if (statMana >= amount) {
				if (pay) {
					CombinedHooks.OnConsumeMana(this, item, amount);
					statMana -= amount;
				}

				return true;
			}

			if (blockQuickMana)
				return false;

			CombinedHooks.OnMissingMana(this, item, amount);
			if (statMana < amount && manaFlower)
				QuickMana();

			if (statMana >= amount) {
				if (pay) {
					CombinedHooks.OnConsumeMana(this, item, amount);
					statMana -= amount;
				}

				return true;
			}

			return false;

		}
	}
}
