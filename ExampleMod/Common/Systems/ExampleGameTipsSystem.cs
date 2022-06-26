using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace ExampleMod.Common.Systems
{
	/// <summary>
	/// This small ModSystem shows off the <seealso cref="ModSystem.ModifyGameTips"/> hook, which allows you to modify
	/// the tips/hints that show up during loading screens.
	/// </summary>
	public class ExampleGameTipsSystem : ModSystem
	{

		public override void ModifyGameTips(IReadOnlyList<GameTipData> gameTips) {
			//If you wish to add your OWN tips, then you have to put them in a Localization file. Check out
			//the GameTips key in the en-US for functionality.

			//What if we want to modify Vanilla tips? There is a GameTipID built into tModLoader that should make
			//disabling certain tips easier.
			//For example, let's turn off the blood moon and solar eclipse tips!
			gameTips[GameTipID.BloodMoonZombieDoorOpening].DisableVisibility();
			gameTips[GameTipID.SolarEclipseCreepyMonsters].DisableVisibility();

			//Now, say you want to modify OTHER mod's tips? You can do that too! Make sure you use the right mod and key name.
			GameTipData disabledTip = gameTips.FirstOrDefault(tip => tip.Mod == "ExampleMod" && tip.ShortKey == "DisabledExampleTip");

			//If you haven't seen null propagation before, in short, the question mark checks if the value is null, and if it is,
			//nothing happens and no error is thrown; but if it isn't null, call the method as usual!
			disabledTip?.DisableVisibility();
		}
	}
}
