using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	/// <summary>
	/// This small ModSystem shows off the <seealso cref="ModSystem.ModifyGameTips"/> hook, which allows you to modify
	/// the tips/hints that show up during loading screens.
	/// </summary>
	public class ExampleGameTipsSystem : ModSystem
	{
		//Set this value to the HIGHEST number for tips that appears in your localization file. For ExampleMod, it's 2.
		public const int TipCount = 2;

		public override void ModifyGameTips(IReadOnlyList<GameTipData> gameTips, out List<LocalizedText> newTips) {
			//What if we want to modify Vanilla tips? There is a GameTipID built into tModLoader that should make
			//disabling certain tips easier.
			//For example, let's turn off the blood moon and solar eclipse tips!
			gameTips[GameTipID.BloodMoonZombieDoorOpening].DisableVisibility();
			gameTips[GameTipID.SolarEclipseCreepyMonsters].DisableVisibility();

			//Do you just want to add your own tips? Just return a list of texts that you want to add!
			//If you have a ton of tips within your localization file, you're going to have to create said translations yourself,
			//since they are not autoloaded:
			for (int i = 0; i <= TipCount; i++) {
				LocalizationLoader.GetOrCreateTranslation(Mod, $"GameTips.ExampleTip{i}");
			}

			newTips = Language.FindAll(Lang.CreateDialogFilter("Mods.ExampleMod.GameTips.")).ToList();
		}
	}
}
