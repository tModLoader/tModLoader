using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

public class ModSystemTest : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) { }

#if COMPILE_ERROR
	public override void SetLanguage(GameCulture culture)/* tModPorter Note: Removed. Use OnLocalizationsLoaded. New hook is called at slightly different times, so read the documentation */ { }
#endif
}