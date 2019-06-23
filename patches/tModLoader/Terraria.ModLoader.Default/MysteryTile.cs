namespace Terraria.ModLoader.Default
{
	public class MysteryTile : ModTile
	{
		public override void SetDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileFrameImportant[Type] = true;
		}
	}
}
