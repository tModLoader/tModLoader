using Terraria.World.Generation;

public class RenamedNamespacesTest
{
	void Method() {
		// namespace: Terraria.World.Generation -> Terraria.WorldBuilding
		GenPass[] tasks = null;
		var a = new Terraria.World.Generation.Actions.Smooth();
		a = new Actions.Smooth();
	}
}