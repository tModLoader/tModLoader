using Terraria.WorldBuilding;
// TODO: Causes infinite loop.
// not-yet-implemented
//using static Terraria.WorldBuilding.Conditions;
//using ActionsAlias = Terraria.WorldBuilding.Actions;
// instead-expect
//using static Terraria.World.Generation.Conditions;
//using ActionsAlias = Terraria.World.Generation.Actions;
using TupalAlias = (int X, int Y);
using IntArray = int[];

public class RenamedNamespacesTest
{
	void Method() {
		// namespace: Terraria.World.Generation -> Terraria.WorldBuilding
		GenPass[] tasks = null;
		var a = new Terraria.WorldBuilding.Actions.Smooth();
		a = new Actions.Smooth();
		//_ = new IsTile();
		//ActionsAlias.Smooth smooth = new ActionsAlias.Smooth();
	}
}