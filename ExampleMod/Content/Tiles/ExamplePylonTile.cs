using ExampleMod.Common;
using ExampleMod.Common.Systems;
using ExampleMod.Content.Items.Placeable;
using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	/// <summary>
	/// An example for creating a Pylon, identical to how they function in Vanilla. Shows off <seealso cref="ModPylon"/>, an abstract
	/// extension of <seealso cref="ModTile"/> that has additional functionality for Pylon specific tiles.
	/// <br>
	/// If you are going to make multiple pylons that all act the same (like in Vanilla), it is recommended you make a base class
	/// with override functionality in order to prevent writing boilerplate. (For example, making a "CrystalTexture" property that you can
	/// override in order to streamline that process.)
	/// </br>
	/// </summary>
	public class ExamplePylonTile : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> crystalHighlightTexture;
		public Asset<Texture2D> mapIcon;

		public override void Load() {
			// We'll need these textures for later, it's best practice to cache them on load instead of continually requesting every draw call.
			crystalTexture = ModContent.Request<Texture2D>(Texture + "_Crystal");
			crystalHighlightTexture = ModContent.Request<Texture2D>(Texture + "_CrystalHighlight");
			mapIcon = ModContent.Request<Texture2D>(Texture + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			VanillaFallbackOnModDeletion = TileID.TeleportationPylon;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			// These definitions allow for vanilla's pylon TileEntities to be placed.
			// tModLoader has a built in Tile Entity specifically for modded pylons, which we must extend (see SimplePylonTileEntity)
			TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;

			// Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName(); // Name is in the localization file
			AddMapEntry(Color.White, pylonName);
		}

		public override NPCShop.Entry GetNPCShopEntry() {
			// In this method we can customize the shop entry for the pylon item.
			// The default method, base.GetNPCShopEntry(), generates a shop entry for the pylon item with the typical pylon conditions: Condition.HappyEnoughToSellPylons, Condition.AnotherTownNPCNearby, and Condition.NotInEvilBiome
			NPCShop.Entry shopEntry = base.GetNPCShopEntry();

			// We will take that shop entry and add an additional condition to check for ExampleBiome, as this is typical for biome pylons
			// This does not affect the teleport conditions, only the sale conditions
			shopEntry.AddCondition(ExampleConditions.InExampleBiome);

			// and finally we return the shop entry
			return shopEntry;
		}

		public override void MouseOver(int i, int j) {
			// Show a little pylon icon on the mouse indicating we are hovering over it.
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ExamplePylonItem>();
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// We need to clean up after ourselves, since this is still a "unique" tile, separate from Vanilla Pylons, so we must kill the TileEntity.
			ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			// Let's say for fun sake that no NPCs need to be nearby in order for this pylon to function. If you want your pylon to function just like vanilla,
			// you don't need to override this method at all.
			return true;
		}

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
			// Right before this hook is called, the sceneData parameter exports its information based on wherever the destination pylon is,
			// and by extension, it will call ALL ModSystems that use the TileCountsAvailable method. This means, that if you determine biomes
			// based off of tile count, when this hook is called, you can simply check the tile threshold, like we do here. In the context of ExampleMod,
			// something is considered within the Example Surface/Underground biome if there are 40 or more example blocks at that location.

			return ModContent.GetInstance<ExampleBiomeTileCount>().exampleBlockCount >= 40;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// Pylons in vanilla light up, which is just a simple functionality we add using ModTile's ModifyLight.
			// Let's just add a simple white light for our pylon:
			r = g = b = 0.75f;
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			// We want to draw the pylon crystal the exact same way vanilla does, so we can use this built in method in ModPylon for default crystal drawing:
			// For the sake of example, lets make our pylon create a bit more dust by decreasing the dustConsequent value down to 1. If you want your dust spawning to be identical to vanilla, set dustConsequent to 4.
			// We also multiply the pylonShadowColor in order to decrease its opacity, so it actually looks like a "shadow"
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			// Just like in SpecialDraw, we want things to be handled the EXACT same way vanilla would handle it, which ModPylon also has built in methods for:
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<ExamplePylonItem>().DisplayName.Key, ref mouseOverText);
		}
	}
}
