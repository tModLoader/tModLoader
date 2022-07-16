using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace ExampleMod.Common.GlobalItems
{
	internal class ItemWithGrowingDamage : GlobalItem
	{
		public int experience;
		public int level;
		public List<Item> consumedWeapons = new List<Item>();
		public static int experiencePerLevel = 100;
		private static int chosenPrefix = -1;
		private static bool reforgeingItem = false;
		private static int chosenRarity = -1;
		private static bool skipAllowCheck = false;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			//Apply this GlobalItem to all swords
			return entity.DamageType == DamageClass.Melee && entity.noMelee == false && entity.useStyle == ItemUseStyleID.Swing;
		}

		public override void LoadData(Item item, TagCompound tag) {
			experience = tag.Get<int>("experience");//Load experience tag
			item.value += experience * 5;
			consumedWeapons = tag.Get<List<Item>>("consumedItems");
			UpdateLevel();
		}

		public override void SaveData(Item item, TagCompound tag) {
			tag["experience"] = experience;//Save experience tag
			tag["consumedItems"] = consumedWeapons;
		}

		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockBack, bool crit) {
			//The sword gains experience when damaging an npc.
			int xp = damage;
			experience += xp;
			item.value += xp * 5;
			UpdateLevel();
		}

		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
			//Gain 1% multiplicative damage for every level on the weapon.
			damage *= 1f + (float)level / 100f;
		}

		public void UpdateLevel() {
			level = experience / experiencePerLevel;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (experience > 0) {
				UpdateLevel();
				tooltips.Add(new TooltipLine(Mod, "level", $"Level: {level}") { OverrideColor = Color.LightGreen });
				string levelString = $" ({(level + 1) * experiencePerLevel - experience} to next level)";
				tooltips.Add(new TooltipLine(Mod, "experience", $"Experience: {experience}{levelString}") { OverrideColor = Color.White });
			}
		}

		public override void OnCreate(Item item, ItemCreationContext context) {
			if (context is RecipeCreationContext rContext) {
				foreach (Item ingredient in rContext.ConsumedItems) {
					if (ingredient.TryGetGlobalItem(out ItemWithGrowingDamage ingredientGlobal)) {
						//Transfer all experience from consumed items to the crafted item.
						experience += ingredientGlobal.experience;
						UpdateLevel();
						//If one of the consumed weapons has any prefix, track it in consumedWeapons.
						if (ingredient.prefix != 0)
							consumedWeapons.Add(ingredient.Clone());
					}
					item.value += experience * 5;
				}
			}
		}

		//The purpose of the ChosePrefix(), AllowPrefix(), PreReforge() and PostReforge() below is
		//	to improve the chance of getting better prefixes from reforging based on the prefixes of the consumed items.
		public override int ChoosePrefix(Item item, UnifiedRandom rand) {
			//The default value of chosenPrefix is -1 which will allow vanilla behavior.
			//chosenPrefix is set in the AllowPrefix() method to force a re-roll if conditions are met.
			return chosenPrefix;
		}

		public override bool AllowPrefix(Item item, int pre) {
			//itemBeforeReforge is set to a clone of the item being reforged in PreReforge() to only allow the below code to run while reforging.
			if(reforgeingItem && !skipAllowCheck) {
				//The item's rarity doesn't update until after the reforge is past AllowPrefix.  Because of this, CheckItemRarity() is needed to know the new rarity.
				int newItemRarity = CheckItemRarity(item, pre);

				//chosenPrefix and chosenRarity will always be -1 the first time AllowPrefix is called during a reforge.
				if (chosenPrefix == -1 && chosenRarity == -1) {
					//If the item has been reforged, check if any of the consumed weapons have a better prefix.
					List<Item> betterPrefixeItems = new List<Item>();
					for (int i = 0; i < consumedWeapons.Count; i++) {
						//Vanilla prefixes affect item rarity.  Generally, better prefixes cause a higher item rarity.
						if (consumedWeapons[i].rare > newItemRarity) {
							betterPrefixeItems.Add(consumedWeapons[i]);
						}
					}
					//If any consumed weapons had a better prefix than the reforged prefix, give a 40% to refororge the item again to one of the better prefixes.
					if (betterPrefixeItems.Count > 0) {
						//GetOneFromList will return one of the better prfixes or -1 if the roll failed.
						Item chosenWeapon = GetOneFromList(betterPrefixeItems, 0.4f);
						int prefixFromChosenWeapon = chosenWeapon.prefix;
						if (prefixFromChosenWeapon > 0) {
							chosenPrefix = prefixFromChosenWeapon;
							//Before forcing the reroll to the chosen prefix, make sure the prefix is allowed on the weapon.
							//AllowPrefix will be called again, this time, it will only execute the else statement, reseting chosenPrefix to -1.
							if (AllowPrefix(item, chosenPrefix)) {
								//Because chosenPrefix will be reset to -1, it needs to be set to prefixFromChosenWeapon again.
								chosenPrefix = prefixFromChosenWeapon;
							}
							else {
								//If the chosen prefix is not allowed on the chosen weapon, instead of chosing a specific prefix for the reforge,
								//	reforge until the weapon's rarity is at least the same rarity of the chosen weapon.(using the else if(chosenRarity > -1) code)
								chosenRarity = chosenWeapon.rare;
							}
							//Re-roll the item and force it to get the chosenPrefix by using the ChoosePrefix() method.
							//Returning false in AllowPrefix() forces an item to be re-rolled.
							return false;
						}
					}
				}
				else if (chosenRarity > -1) {
					//Creates a loop of reforging the item until the rarity of the weapon is at least the rarity of the chosen weapon from above.
					if (newItemRarity >= chosenRarity) {
						//Reset chosenRarity and stop re-rolling.
						chosenRarity = -1;
					}
					else {
						//If the reRolled prefix causes an item rarity lower than the chosen rarity, re-roll again.
						return false;
					}
				}
				else {
					//Prevent re-rolling more than once per reforge.
					//Reset chosenPrefix to its default value of -1 to allow for normal reforges to happen in the ChoosePrefix() method.
					chosenPrefix = -1;
				}
			}
			return true;
		}

		public override bool PreReforge(Item item) {
			reforgeingItem = true;
			return true;
		}

		public override void PostReforge(Item item) {
			reforgeingItem = false;
		}

		private static int CheckItemRarity(Item item, int prefix) {
			//An item's rarity doesn't update until after the reforge is past AllowPrefix().
			//	Because of this, CheckItemRarity() is needed to know the new rarity.
			//Prevent AllowPrefix() re-roll code from running since this item is only temporary to check the rarity caused by the prefix.
			skipAllowCheck = true;
			Item tempItem = new Item(item.type);
			tempItem.Prefix(prefix);
			skipAllowCheck = false;
			return tempItem.rare;
		}

		/// <summary>
		/// Randomly selects an item from the list if the chance is higher than the randomly generated float.
		/// </summary>
		/// <param name="options">Posible items to be selected.</param>
		/// <param name="chance">Chance to select an item from the list.</param>
		/// <returns>Item selected or null if chance was less than the generated float.</returns>
		public static Item GetOneFromList(List<Item> options, float chance) {
			//Example: items contains 4 items and chance = 0.4f (40%)
			float randFloat = Main.rand.NextFloat();//Example randFloat = 0.24f
			if(randFloat < chance) {
				float count = options.Count;// = 4f
				float chancePerItem = chance / count;// chancePerItem = 0.4f / 4f = 0.1f.  (10% chance each item)  
				int chosenItemNum = (int)(randFloat / chancePerItem);// chosenItemNum = (int)(0.24f / 0.1f) = (int)(2.4f) = 2.
				return options[chosenItemNum];// items[2] being the 3rd item in the list.
			}
			else {
				//If the chance is less than the generated float, return null.
				return new Item();
			}
		}

		//Currently not used in ExampleMod, but functions correctly
		public static void SpawnCoins(int coinValue, Player player) {
			int valuePerCoin = 1000000;//Starting with value of 1 platinum coin.
			for (int i = 3; i >= 0; i--) {
				int coins = coinValue / valuePerCoin;
				coinValue %= valuePerCoin;
				valuePerCoin /= 100;
				if (coins > 0)
					player.QuickSpawnItem(player.GetSource_GiftOrReward(), ItemID.CopperCoin + i, coins);
			}
		}
	}
}
