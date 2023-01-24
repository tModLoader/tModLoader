using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

public class ModSystemTest : ModSystem
{
	public override void ModifyWorldGenTasks(List<GenPass> tasks, ref float totalWeight) { }

	public override void SetLanguage(GameCulture culture) { }
}