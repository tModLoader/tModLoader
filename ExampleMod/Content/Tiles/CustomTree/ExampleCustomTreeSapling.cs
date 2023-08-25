using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content.Tiles.CustomTree
{
	public class ExampleCustomTreeSapling : CustomTreeSapling
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			// Map color with default name from localization file
			AddMapEntry(Color.Gray, DefaultMapNameLocalization);
		}
	}
}