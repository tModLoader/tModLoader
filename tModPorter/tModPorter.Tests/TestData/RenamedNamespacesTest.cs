using Terraria.World.Generation;
// TODO: Causes infinite loop.
//using static Terraria.World.Generation.Conditions;
//using ActionsAlias = Terraria.World.Generation.Actions;
using TupalAlias = (int X, int Y);
using IntArray = int[];

public class RenamedNamespacesTest
{
	void Method() {
		// namespace: Terraria.World.Generation -> Terraria.WorldBuilding
		GenPass[] tasks = null;
		var a = new Terraria.World.Generation.Actions.Smooth();
		a = new Actions.Smooth();
		//_ = new IsTile();
		//ActionsAlias.Smooth smooth = new ActionsAlias.Smooth();
	}
}