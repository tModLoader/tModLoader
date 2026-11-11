using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Systems
{
	// Shows how to change the vanilla Journey Mode power buttons and categories.
	// To add a whole new category to the power strip, see ExampleCreativeCategory instead.
	public class ExampleCreativePowersSystem : ModSystem
	{
		public static LocalizedText RightClickToHealText { get; private set; }

		public override void SetStaticDefaults() {
			RightClickToHealText = Mod.GetLocalization($"{nameof(ExampleCreativePowersSystem)}.RightClickToHeal");
		}

		public override void ModifyCreativePowerButton(ICreativePower power, UIElement button) {
			// Right clicking the God Mode button also fully heals the player.
			if (power is CreativePowers.GodmodePower) {
				button.OnRightClick += (_, _) => {
					// In multiplayer the server can restrict who may use each power. Check the power's permission before acting on its behalf.
					if (!CreativePowersHelper.IsAvailableForPlayer(power, Main.myPlayer)) {
						return;
					}

					SoundEngine.PlaySound(SoundID.MenuTick);
					Main.LocalPlayer.Heal(Main.LocalPlayer.statLifeMax2);
				};
			}
		}

		public override void ModifyCreativePowerHoverText(ICreativePower power, ref string text) {
			// Let the player know about the right click action added above.
			if (power is CreativePowers.GodmodePower) {
				text += "\n" + RightClickToHealText.Value;
			}
		}

		public override bool CanShowCreativeCategory(int category) {
			// To decide based on another category, ask CreativeMenuLoader.IsCategoryShown rather than calling other mods' hooks. It gives every mod the same answer and can't loop forever.
			// For example, uncomment these lines to show ExampleCreativeCategory only while some mod hides vanilla's Personal category:
			// if (category == ModContent.GetInstance<Content.CreativeCategories.ExampleCreativeCategory>().Type) {
			// 	return !CreativeMenuLoader.IsCategoryShown(CreativeCategoryID.Personal);
			// }

			// Return false to hide a category from the power strip. Uncomment this line to hide the Research category:
			// return category != CreativeCategoryID.Research;
			return true;
		}
	}
}
