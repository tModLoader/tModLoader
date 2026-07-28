using ExampleMod.Content.NPCs;
using ExampleMod.Content.NPCs.TownPets;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	// This class showcases "Global substitutions".
	// Global substitutions are used to influence LocalizedText values automatically and dynamically.
	// Global substitutions are used for 2 purposes, substituting text into a LocalizedText and applying conditional logic to a LocalizedText indicating if it can be used or not:
	// 1. Substituting text is similar to how we can use placeholders ("{0}") or substitutions ("{$Some.Key}") normally, except the LocalizedText text value always reflects the dynamic value of the global substitutions automatically.
	// 2. LocalizedText entries can include global substitutions as conditions to apply conditional checks to whether or not the text can be shown or not. For example, the global substitution "{?Day}" will cause the containing LocalizedText to only be valid if it is daytime.
	public class LanguageSubstitutionSystem : ModSystem
	{
		public override void Load() {
			// Register global substitutions during mod loading by calling Lang.RegisterGlobalSubstitution.

			// The most common usages of global substitutions are TownNPCs mentioning other TownNPCs by their given names and displaying bound keybinds to the user:

			// The substitution "{ExamplePerson}" in a LocalizedText will automatically be replaced with the given name of the ExamplePerson in the world.
			// If there is no ExamplePerson in the world, this will return null which will indicated that the LocalizedText is not valid.
			// Helper methods like Lang.CreateDialogFilter or directly checking LocalizedText.ConditionsMet should be used to filter out invalid text.
			Lang.RegisterGlobalSubstitution("ExamplePerson", () => NPC.GetFirstNPCNameOrNull(ModContent.NPCType<ExamplePerson>()));
			Lang.RegisterGlobalSubstitution("ExampleTownPet", () => NPC.GetFirstNPCNameOrNull(ModContent.NPCType<ExampleTownPet>()));

			// This substitution will resolve to a comma-separated list of the assigned hotkeys for the RandomBuff keybind.
			// It will adjust automatically to changed hotkeys and depending on the current input method.
			// If there is no assigned hotkey, it will instead return "<Unbound>".
			Lang.RegisterGlobalSubstitution("InputTrigger_ExampleMod_RandomBuff", () => {
				string inputList = PlayerInput.GenerateInputTag_ForCurrentGamemode(tagForGameplay: true, "ExampleMod/RandomBuff");
				return string.IsNullOrWhiteSpace(inputList) ? Lang.menu[195].Value : inputList;
			});

			// Biome and Boss progression are also useful. These return bool, so they are used for conditional checks.
			// "{?ExampleBiome}" or "{?DownedMinionBoss}" in a LocalizedText will cause these conditions to be checked to indicate if the LocalizedText is valid or not.
			// Use "{?!KeyHere}" to reverse the conditional check.
			// Multiple checks can be included.
			Lang.RegisterGlobalSubstitution("ExampleBiome", () => ExampleConditions.InExampleBiome.IsMet());
			Lang.RegisterGlobalSubstitution("DownedMinionBoss", () => ExampleConditions.DownedMinionBoss.IsMet());

			// To see global substitutions in action, see the following localization entries and their corresponding code, if relevant:
			// 1. Mods.ExampleMod.Items.ExampleHood.SetBonus - Uses the vanilla {ToggleArmorSetBonusKey} substitution. It will automatically display "UP" or "DOWN" depending on the user's "Activate Set Bonuses" (Main.ReversedUpDownArmorSetBonuses) setting.
			// 2. Mods.ExampleMod.GameTips.ExampleTip3 - Displays {InputTrigger_ExampleMod_RandomBuff} to the user in a tip during world generation. It will only show if it has a bound hotkey.
			// 3. Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.PartyGirlDialogue - Uses the vanilla {PartyGirl} substitution. This will display the given name and will only show if a Party Girl is in the world.
			// 4. Mods.ExampleMod.Dialogue.ExampleTravelingMerchant.ConditionalDialogue1 - Displays {ExamplePerson}, a custom substitution. This will display the given name and will only show if an Example Person is in the world.
			// 5. Mods.ExampleMod.Dialogue.ExamplePerson.ConditionalDialogue1 - Uses the {?DownedMinionBoss} custom condition and {?GolemDefeated} vanilla condition in chat dialogue. This will only show if both Minion Boss and Golem have been defeated.
			// 6. Mods.ExampleMod.Dialogue.ExamplePerson.ConditionalDialogue2 - Shows negating conditions. This will only show at night and if there is no ExamplePet.
		}
	}
}
