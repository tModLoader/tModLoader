using Terraria.Localization;

namespace Terraria.ModLoader;

public abstract class ModLiquid : ModBlockType
{
	/// <summary>
	/// <inheritdoc cref="AddMapEntry(Color, LocalizedText)"/>
	/// <br/><br/> <b>Overload specific:</b> This overload has an additional <paramref name="nameFunc"/> parameter. This function will be used to dynamically adjust the hover text. The parameters for the function are the default display name, x-coordinate, and y-coordinate. This function is most typically used for chests and dressers to show the current chest name, if assigned, instead of the default chest name. <see href="https://github.com/tModLoader/tModLoader/blob/1.4.4/ExampleMod/Content/Tiles/Furniture/ExampleChest.cs">ExampleMod's ExampleChest</see> is one example of this functionality.
	/// </summary>
	public void AddMapEntry(Color color, LocalizedText name, Func<string, int, int, string> nameFunc)
	{
		if (!MapLoader.initialized) {
			MapEntry entry = new MapEntry(color, name, nameFunc);
			if (!MapLoader.liquidEntries.Keys.Contains(Type)) {
				MapLoader.liquidEntries[Type] = new List<MapEntry>();
			}
			MapLoader.liquidEntries[Type].Add(entry);
		}
	}
}