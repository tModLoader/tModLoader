using Terraria.WorldBuilding;
// TODO: Causes infinite loop.
//using static Terraria.WorldBuilding.Conditions;
//using ActionsAlias = Terraria.WorldBuilding.Actions;
using TupalAlias = (int X, int Y);
using IntArray = int[];

public class RenamedNamespacesTest
{
	void Method() {
		// namespace: Terraria.World.Generation -> Terraria.WorldBuilding
		GenPass[] tasks = null;
		var a = new Terraria.WorldBuilding.Actions.Smooth();
		a = new Actions.Smooth();
		_ = new IsTile();
		ActionsAlias.Smooth smooth = new ActionsAlias.Smooth();
	}
}