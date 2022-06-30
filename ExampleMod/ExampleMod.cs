using ExampleMod.Common.GlobalItems;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	// This is a partial class, meaning some of its parts were split into other files. See ExampleMod.*.cs for other portions.
	public partial class ExampleMod : Mod
	{
		public const string AssetPath = $"{nameof(ExampleMod)}/Assets/";

		public static int ExampleCustomCurrencyId;

		public static List<Item> consumedItems = new();

		public override void Load() {
			// Registers a new custom currency
			ExampleCustomCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.ExampleCustomCurrency(ModContent.ItemType<Content.Items.ExampleItem>(), 999L, "Mods.ExampleMod.Currencies.ExampleCustomCurrency"));
		}

		public override void Unload() {
			// The Unload() methods can be used for unloading/disposing/clearing special objects, unsubscribing from events, or for undoing some of your mod's actions.
			// Be sure to always write unloading code when there is a chance of some of your mod's objects being kept present inside the vanilla assembly.
			// The most common reason for that to happen comes from using events, NOT counting On.* and IL.* code-injection namespaces.
			// If you subscribe to an event - be sure to eventually unsubscribe from it.

			// NOTE: When writing unload code - be sure use 'defensive programming'. Or, in other words, you should always assume that everything in the mod you're unloading might've not even been initialized yet.
			// NOTE: There is rarely a need to null-out values of static fields, since TML aims to completely dispose mod assemblies in-between mod reloads.
		}

		public override void PostAddRecipes() {
			for (int i = 0; i < Main.recipe.Length; i++) {
				foreach(Item ingredient in Main.recipe[i].requiredItem) {
					//Checks if any ingredient meets the conditions for ExampleExperienceItem by trying to get global item
					//If at least one does, add the OnConsueItemCallback to the recipe.
					//The "conditions" reffering to ExampleExperienceItem's AppliesToEntity() method.
					if (ingredient.TryGetGlobalItem(out ExampleExperienceItem ingredientGlobal)) {
						//OnConsumeItemCallback gives access to the item that is about to be consumed while crafting.
						Recipe.OnConsumeItemCallback consumeCallback = (Recipe recipe, Item item, ref int amountRemaining) => {
							//Because recipies can have multiple ingredients, some that dont meet the conditions of ExampleExperienceItem,
							//We need to try getting the global item again inside the OnConsumeItemCallback.
							if (item.TryGetGlobalItem(out ExampleExperienceItem exampleExperienceItem)) {
								//The return value determines if the vanilla code for consuming this specific item will be ran.
								//If trying to prevent an item being consumed, either set amountRemaining = 0 or return false;
								//Note: retruning false will prevent the item being consumed, however the craft has already begun and will not stop.
								//If the ammountRemaining is > 0, it will continue looking for the same item type to consume.
								//It would generally be best to reduce the amountRemaining by the item.stack
								//	or setting amountRemaining to 0 when prevening an item from being consumed.
								if (exampleExperienceItem.experience < 1000) {
									ExampleExperienceItem.SpawnCoins(item);
									consumedItems.Add(item.Clone());
									return true;
								}
								else {
									exampleExperienceItem.experience -= 1000;
									amountRemaining -= item.stack;
									return false;
								}
							}
							//If the ingredient, item, does not meet the conditions of ExampleExperienceItem,
							//Return true to allow the vanilla consume item code to run.
							return true;
						};
						Main.recipe[i].AddOnConsumeItemCallback(consumeCallback);

						Recipe.OnCraftCallback craftCallback = (Recipe recipe, Item item) => {
							if(item.TryGetGlobalItem(out ExampleExperienceItem itemGlobal)) {
								foreach (Item consumedItem in consumedItems) {
									itemGlobal.experience += consumedItem.GetGlobalItem<ExampleExperienceItem>().experience;
									itemGlobal.UpdateLevel();
								}
							}
							consumedItems.Clear();
						};
						Main.recipe[i].AddOnCraftCallback(craftCallback);
						//We only want to add the callback to a recipe once, so if we find one ingredient that meets the conditions
						//of ExampleExperienceItem, stop checking the ingredients from that recipe with break.
						break;
					}
				}
			}
		}
	}
}
