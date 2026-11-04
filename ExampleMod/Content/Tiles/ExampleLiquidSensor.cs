using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	//Here is an example of a liquid sensor for all liquid types
	//There isn't too much code here related to sensors, which is why it's recommended that modders look at TEModLogicSensors for the main sensor code
	public class ExampleLiquidSensor : ModTile
	{
		public override void SetStaticDefaults() {
			TileID.Sets.AllowsSaveCompressionBatching[Type] = true;
			TileID.Sets.DoesntGetReplacedWithTileReplacement[Type] = true;
			Main.tileFrameImportant[Type] = true;
			//Using TileObjectData we use HookPostPlaceMyPlayer to spawn a Tile Entity, using the ModTileEntity's Hook_AfterPlacement to spawn a TEModLogicSensors
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<TEModLogicSensors>().Hook_AfterPlacement, -1, 0, processedCoordinates: true);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(200, 200, 200));
		}
	}
}
