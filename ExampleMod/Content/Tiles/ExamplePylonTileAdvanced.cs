using ExampleMod.Content.Items.Placeable;
using ExampleMod.Content.TileEntities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.UI;

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
		public const int CrystalHorizontalFrameCount = 2;
		public const int CrystalVerticalFrameCount = 8;

		public Asset<Texture2D> crystalTexture;
		public Asset<Texture2D> mapIcon;

		public readonly Point16 tileOrigin = new Point16(0, 2);

		public override void Load() {
			//We'll still use the other Example Pylon's sprites, but we need to adjust the texture values first to do so.
			crystalTexture = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_Crystal");
			mapIcon = ModContent.Request<Texture2D>(Texture.Replace("Advanced", "") + "_MapIcon");
		}

		public override void SetStaticDefaults() {
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			//This time around, we'll have a tile that is 2x3 instead of 3x4.
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.Origin = tileOrigin;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			//Since we are going to need more in-depth functionality, we can't use vanilla's Pylon TE's OnPlace, but we can still use it to check for CanPlace:
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(TETeleportationPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<AdvancedPylonTileEntity>().Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.PreventsSandfall[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			ModTranslation pylonName = CreateMapEntryName();
			AddMapEntry(Color.Black, pylonName);
		}

		public override int? IsPylonForSale(int npcType, Player player, bool isNPCHappyEnough) {
			//Let's say that our pylon is for sale no matter what for any NPC under all circumstances.
			return ModContent.ItemType<ExamplePylonItemAdvanced>();
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

			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 3, 4, ModContent.ItemType<ExamplePylonItemAdvanced>());
		}

		//For the sake of example, we will allow this pylon to always be teleported to as long as it is on, so we make sure these two checks return true.
		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount) {
			return true;
		}

		public override bool ValidTeleportCheck_AnyDanger(TeleportPylonInfo pylonInfo) {
			return true;
		}

		public override void ValidTeleportCheck_PostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey) {
			//If you are unfamiliar with pattern matching notation, all this is asking is:
			//1) The Tile Entity at the given position is an AdvancedPylonTileEntity (AKA not null or something else)
			//2) The Tile Entity's isActive value is false
			if (TileEntity.ByPosition[destinationPylonInfo.PositionInTiles] is AdvancedPylonTileEntity { isActive: false }) {
				//Given that both of these things are true, set the error key to our own special message (check the localization file), and make the destination value invalid (false)
				destinationPylonValid = false;
				errorKey = "Mods.ExampleMod.MessageInfo.UnstablePylonIsOff";
			}
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			//This time, we'll ONLY draw the crystal if the pylon is active.
			if (drawData.tileFrameX % 36 == 0 && drawData.tileFrameY == 0 && TileEntity.ByPosition[new Point16(i, j)] is AdvancedPylonTileEntity { isActive: true }) {
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			//This code is effectively identical to how it is in ExamplePylonTile, so there will be significantly less comments. If you need more info, check out ExamplePylonTile's SpecialDraw.

			Vector2 offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen) {
				offScreen = Vector2.Zero;
			}

			Point pos = new Point(i, j);
			Tile tile = Main.tile[pos.X, pos.Y];
			if (tile == null || !tile.HasTile) {
				return;
			}

			Texture2D vanillaPylonCrystals = TextureAssets.Extra[181].Value;
			int frameY = (Main.tileFrameCounter[TileID.TeleportationPylon] + pos.X + pos.Y) % 64 / CrystalVerticalFrameCount;

			Rectangle crystalFrame = crystalTexture.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 0, frameY); 
			Rectangle highlightFrame = crystalTexture.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 1, frameY);
			vanillaPylonCrystals.Frame(CrystalHorizontalFrameCount, CrystalVerticalFrameCount, 0, frameY);
			
			Vector2 origin = crystalFrame.Size() / 2f;
			Vector2 centerPos = pos.ToWorldCoordinates(16f, 24f); //Different displacement on the crystal, since the tile itself is smaller
			float centerDisplacement = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 5f);
			Vector2 drawPos = centerPos + offScreen + new Vector2(0f, -28f) + new Vector2(0f, centerDisplacement * 4f); //Different displacement, once again
			
			if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(40)) {
				Rectangle dustBox = Utils.CenteredRectangle(drawPos, crystalFrame.Size());

				int dustIndex = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, Color.Gray, 0.5f);
				Main.dust[dustIndex].velocity *= 0.1f;
				Main.dust[dustIndex].velocity.Y -= 0.2f;
			}

			Color crystalColor = Color.Lerp(Lighting.GetColor(pos.X, pos.Y), Color.Black, 0.8f) ;
			Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, crystalFrame, crystalColor * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2f) / 1f) * 0.2f + 0.8f;
			Color sheenColor = Main.DiscoColor * 0.1f * scale; //This time, however, we'll make the color a disco color instead of white.
			for (float displacement = 0f; displacement < 1f; displacement += 355f / (678f * (float)Math.PI)) {
				Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition + ((float)Math.PI * 2f * displacement).ToRotationVector2() * (6f + centerDisplacement * 2f), crystalFrame, sheenColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}

			int selectionType = 0;
			if (Main.InSmartCursorHighlightArea(pos.X, pos.Y, out bool actuallySelected)) {
				selectionType = 1;

				if (actuallySelected) {
					selectionType = 2;
				}
			}

			if (selectionType == 0) {
				return;
			}

			int colorPotency = (crystalColor.R + crystalColor.G + crystalColor.B) / 3;
			if (colorPotency > 10) {
				Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionType == 2, colorPotency);
				Main.spriteBatch.Draw(crystalTexture.Value, drawPos - Main.screenPosition, highlightFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
			}
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
			if (TileEntity.ByPosition[pylonInfo.PositionInTiles] is not AdvancedPylonTileEntity entity) {
				//If for some reason we don't find the tile entity, we won't draw anything.
				return;
			}

			//Depending on the whether or not the pylon is active, the color of the icon will change;
			//otherwise, it acts as normal.
			drawColor = entity.isActive ? Color.Green : Color.Yellow;

			bool isMouseOver = context.Draw(
				                          mapIcon.Value,
				                          pylonInfo.PositionInTiles.ToVector2() + new Vector2(1f, 1.5f),
				                          drawColor,
				                          new SpriteFrame(1, 1, 0, 0),
				                          deselectedScale,
				                          selectedScale,
				                          Alignment.Center
			                          )
			                          .IsMouseOver;

			if (!isMouseOver) {
				return;
			}

			Main.cancelWormHole = true;
			mouseOverText = Language.GetTextValue("Mods.ExampleMod.ItemName.ExamplePylonItemAdvanced");

			if (Main.mouseLeft && Main.mouseLeftRelease) {
				Main.mouseLeftRelease = false;
				Main.mapFullscreen = false;
				PlayerInput.LockGamepadButtons("MouseLeft");
				Main.PylonSystem.RequestTeleportation(pylonInfo, Main.LocalPlayer);
				SoundEngine.PlaySound(SoundID.Item6);
			}
		}
	}
}
