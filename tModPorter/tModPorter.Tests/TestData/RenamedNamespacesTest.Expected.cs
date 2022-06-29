using Terraria.WorldBuilding;

public class RenamedNamespacesTest
{
	void Method() {
		// namespace: Terraria.World.Generation -> Terraria.WorldBuilding
		GenPass[] tasks = null;
		var a = new Terraria.WorldBuilding.Actions.Smooth();
		a = new Actions.Smooth();
	}
}