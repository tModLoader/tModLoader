using ExampleMod.Content.Items.Placeable;
using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ExampleMod.Content.Tiles
{
	/// <summary>
	/// This is a more advanced variation of the <seealso cref="ExamplePylonTile"/> implementation
	/// in tandem with <seealso cref="AdvancedPylonTileEntity"/>, which shows off what advanced techniques you can apply with ModPylons.
	/// If you want to use ModPylons with your own Tile Entities or with multi-tiles that do not conform to vanilla's standards, then
	/// this is the example for you. If you just want normal pylons that act like the ones in vanilla do, check out <seealso cref="ExamplePylonTile"/>.
	/// </summary>
	/// <remarks>
	/// Note that since this is an advanced example, things that were already explained in <seealso cref="ExamplePylonTile"/> will not
	/// be as thoroughly explained. They will still be explained if needed in context.
	/// </remarks>
	public class ExamplePylonTileAdvanced : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> crystalHighlightTexture;
		public Asset<Texture2D> mapIcon;

		public override void Load() {
			// We'll still use the other Example Pylon's sprites, but we need to adjust the texture values first to do so.
			crystalTexture = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_Crystal");
			crystalHighlightTexture = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_CrystalHighlight");
			mapIcon = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			// This time around, we'll have a tile that is 2x3 instead of 3x4.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			// Since we are going to need more in-depth functionality, we can't use vanilla's Pylon TE's OnPlace or CanPlace:
			AdvancedPylonTileEntity advancedEntity = ModContent.GetInstance<AdvancedPylonTileEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(advancedEntity.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(advancedEntity.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName();
			AddMapEntry(Color.Black, pylonName);
		}

		public override NPCShop.Entry GetNPCShopEntry() {
			// Let's say that our pylon is for sale no matter what for any NPC under all circumstances.
			return new NPCShop.Entry(ModContent.ItemType<ExamplePylonItemAdvanced>());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override bool RightClick(int i, int j) {
			Main.mapFullscreen = true;
			SoundEngine.PlaySound(SoundID.MenuOpen);
			return true;
		}

		public override void MouseOver(int i, int j) {
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<ExamplePylonItemAdvanced>();
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			ModContent.GetInstance<AdvancedPylonTileEntity>().Kill(i, j);
		}

		// For the sake of example, we will allow this pylon to always be teleported to as long as it is on, so we make sure these two checks return true.
		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return true;
		}

		public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo) {
			return true;
		}

		// These two steps below are simply determining whether or not either side of the coin is valid, which is to say:
		// Is the destination pylon (the pylon clicked on the map) a valid pylon, and is the pylon the player standing near (the nearby pylon)
		// a valid pylon? If either one of these checks fail, a errorKey wil be set to a custom localization key and a message will go to the player with
		// said text (after its been localized, of course).
		public override void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) {
			// If you are unfamiliar with pattern matching notation, all this is asking is:
			// 1) The Tile Entity at the given position is an AdvancedPylonTileEntity (AKA not null or something else)
			// 2) The Tile Entity's isActive value is false
			if (TileEntity.ByPosition[destinationPylonInfo.PositionInTiles] is AdvancedPylonTileEntity { isActive: false }) {
				// Given that both of these things are true, set the error key to our own special message (check the localization file), and make the destination value invalid (false)
				destinationPylonValid = false;
				errorKey = "Mods.ExampleMod.MessageInfo.UnstablePylonIsOff";
			}
		}

		public override void ValidTeleportCheck_NearbyPostCheck(TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool anyNearbyValidPylon, ref string errorKey) {
			// The next check is determining whether or not the nearby pylon is potentially unstable, and if so, if it's not active, we also prevent teleportation.
			if (TileEntity.ByPosition[nearbyPylonInfo.PositionInTiles] is AdvancedPylonTileEntity { isActive: false }) {
				destinationPylonValid = false;
				errorKey = "Mods.ExampleMod.MessageInfo.NearbyUnstablePylonIsOff";
			}
		}

		public override void ModifyTeleportationPosition(TeleportPylonInfo destinationPylonInfo, ref Vector2 teleportationPosition) {
			// Now, for the fun of it and for the showcase of this hook, let's put a player a bit into the air above the pylon when they teleport.
			teleportationPosition = destinationPylonInfo.PositionInTiles.ToWorldCoordinates(8f, -32f);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			// Same as the basic example, but our light will be the disco color like the crystal
			r = Main.DiscoColor.R / 255f * 0.75f;
			g = Main.DiscoColor.G / 255f * 0.75f;
			b = Main.DiscoColor.B / 255f * 0.75f;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			// This time, we'll ONLY draw the crystal if the pylon is active
			// We need to check the framing here in order to guarantee we that we are trying to grab the TE ONLY when in the top left corner, where it is
			// located. If we don't do this check, we will be attempting to grab the TE in position where it doesn't exist, throwing errors and causing
			// loads of visual bugs.
			if (drawData.tileFrameX % 36 == 0 && drawData.tileFrameY == 0 && TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity entity) && entity is AdvancedPylonTileEntity { isActive: true }) {
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			// This code is essentially identical to how it is in the basic example, but this time the crystal color is the disco (rainbow) color instead
			// Also, since we want the pylon crystal to be drawn at the same height as vanilla (since our tile is one tile smaller), we have to move up the crystal accordingly with the crystalOffset parameter
			DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -18f), Main.DiscoColor * 0.1f, Main.DiscoColor, 1, CrystalVerticalFrameCount);
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			if (!TileEntity.ByPosition.TryGetValue(pylonInfo.PositionInTiles, out var te) || te is not AdvancedPylonTileEntity entity) {
				// If for some reason we don't find the tile entity, we won't draw anything.
				return;
			}

			// Depending on the whether or not the pylon is active, the color of the icon will change;
			// otherwise, it acts as normal.
			drawColor = !entity.isActive ? Color.Gray * 0.5f : drawColor;
			bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1, 1.5f), drawColor, deselectedScale, selectedScale);
			DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<ExamplePylonItemAdvanced>().DisplayName.Key, ref mouseOverText);
		}
	}
}
