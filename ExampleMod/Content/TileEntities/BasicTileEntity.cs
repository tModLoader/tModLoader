using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ExampleMod.Content.TileEntities
{
	// This file contains all the code necessary for a basic Tile Entity. It is intended to showcase a fully working example while being simple enough to be easily followed and adapted if needed. Follow the https://github.com/tModLoader/tModLoader/wiki/Basic-Tile-Entity guide for more information, this example is essentially the result of following the guide.

	// A tile entity is an object bound to a placed tile to allow it additional capabilities normal tiles are unable to have. For example, normal tiles can't store data and can't run code every game update. This is the main purpose of tile entities.
	// This file contains the ModTile, ModItem, and ModTileEntity classes to demonstrate this fully self-contained example:
	// - ModTileEntity - the tile entity attached to the tile. It provides the additional logic and data storage.
	// - ModTile - the tile that the ModTileEntity will be attached to.
	// - ModItem - places the tile.

	// BasicTileEntity is essentially a water barrel that collects water while it is raining.
	// Once full, the tile can be mined or right clicked to produce 1 free Water Bucket item. 
	public class BasicTileEntity : ModTileEntity
	{
		// This water barrel will fill up in 1 minute of rain, or 3600 game updates.
		private const int MaxFill = 3600;
		private const int SyncInterval = MaxFill / 10;

		// We use a property to consolidate logic clamping water level value and triggering a network sync.
		private int waterFillLevel;
		public int WaterFillLevel {
			get { return waterFillLevel; }
			set {
				int newFillLevel = Math.Clamp(value, 0, MaxFill);
				bool syncNeeded = waterFillLevel / SyncInterval != newFillLevel / SyncInterval;
				waterFillLevel = newFillLevel;
				if (syncNeeded) {
					// To reduce network spam while raining, we only sync this tile entity at 10% fill intervals.
					// This may or may not be the correct approach for other ModTileEntity, depending on how accurate of
					// data the client will need access to.
					if (Main.netMode == NetmodeID.Server) {
						// The TileEntitySharing message will trigger NetSend, manually syncing the changed data.
						NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
					}
				}
			}
		}

		// The water capacity expressed as a percent value.
		public int WaterFillPercentage => WaterFillLevel * 100 / MaxFill;

		// Which tile sprite to show. We have 5 states for 0%, 25%, 50%, 75%, and 100% fill levels.
		public int WaterFillStage => WaterFillLevel * 4 / MaxFill;

		// Tile Entities can store data. This data most likely needs to be synced to connected clients.
		public override void SaveData(TagCompound tag) {
			tag[nameof(WaterFillLevel)] = WaterFillLevel;
		}

		public override void LoadData(TagCompound tag) {
			WaterFillLevel = tag.GetInt(nameof(WaterFillLevel));
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(WaterFillLevel);
		}

		public override void NetReceive(BinaryReader reader) {
			WaterFillLevel = reader.ReadInt32();
			// Debugging messages can help verify that the data is properly syncing to clients.
			/*
			Main.NewText($"NetReceive called, Position: ({Position.X}, {Position.Y}), WaterFillLevel: {WaterFillLevel}");
			*/
		}

		// Tile Entities update every game update. Regular tiles can only ever update at random intervals. This is one of the main reasons for making a tile entity.
		public override void Update() {
			if (Main.raining) {
				WaterFillLevel += 1;
				// Update does not run for multiplayer clients. Changes to data that other clients need requires syncing the data to them. In this example this happens in the WaterFillLevel setter.
			}
		}

		// IsTileValidForEntity is required, usually you can use this code directly.
		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<BasicTileEntityTile>();
		}
	}

	// BasicTileEntityTile is the Tile that BasicTileEntity attaches to.
	// The most important parts are the TileObjectData.newTile.HookPostPlaceMyPlayer assignment and the KillMultiTile method.
	// All of the other methods show very commonly desired functionality, but are not required for a working Tile Entity.
	public class BasicTileEntityTile : ModTile
	{
		public static LocalizedText StatusText { get; private set; }

		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			// These 3 are used to prevent the tile from being killed by accident. Item holding tiles usually do this to prevent losing the item or having to place the tile again, which would feel bad for the player.
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			// Tell the tile to place the Tile Entity on the tile after placing it.
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<BasicTileEntity>().Generic_HookPostPlaceMyPlayer;
			// The additional "states" in BasicTileEntityTile.png are laid out vertically. If additional styles were added to this example later we'd want those placed horizontally.
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.addTile(Type);

			// Etc
			AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapHoverText);
			StatusText = this.GetLocalization(nameof(StatusText));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// When the tile is removed, we need to remove the Tile Entity as well.
			ModContent.GetInstance<BasicTileEntity>().Kill(i, j);
		}

		// The following hooks all show accessing the Tile Entity and using it to adjust the behavior and look of this Tile. 

		// Changing the visuals of tile according to the corresponding data.
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			if (TileEntity.TryGet(i, j, out BasicTileEntity tileEntity)) {
				tileFrameY = (short)(tileFrameY + (tileEntity.WaterFillStage * 38));

				// We can uncomment this code to spawn dust at the tile entity position for debugging purposes. Some developer mods also have tools to visualize tile entities.
				/*
				if (TileObjectData.IsTopLeft(i, j)) {
					Dust.QuickDust(tileEntity.Position.X, tileEntity.Position.Y, Color.Green);
				}
				*/
			}
		}

		// The text shown when hovering over this tile on the fullscreen map.
		public static string MapHoverText(string name, int i, int j) {
			if (TileEntity.TryGet(i, j, out BasicTileEntity tileEntity)) {
				return StatusText.Format(tileEntity.WaterFillPercentage);
			}
			else {
				// Note that it is possible for a map entry to be queried for a tile location that doesn't have a TileEntity anymore.
				// This can happen in multiplayer when a world section hasn't been synced yet or when a players map hasn't been updated to match changes to the world.
				// This code shows detecting those situations, but the basic lesson is don't assume a TileEntity will always be present.
				Point16 topLeft = TileObjectData.TopLeft(i, j);
				if (!Main.sectionManager.TileLoaded(topLeft.X, topLeft.Y)) {
					return $"{name}: World section not loaded yet";
				}
				return $"{name}: No TileEntity found at coordinate";
			}
		}

		public override void MouseOver(int i, int j) {
			if (TileEntity.TryGet(i, j, out BasicTileEntity tileEntity)) {
				Player player = Main.LocalPlayer;
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = -1;
				player.cursorItemIconText = StatusText.Format(tileEntity.WaterFillPercentage);
			}
		}

		public override bool RightClick(int i, int j) {
			if (!TileEntity.TryGet(i, j, out BasicTileEntity tileEntity)) {
				return true;
			}
			if (tileEntity.WaterFillPercentage == 100) {
				/* While it is tempting to spawn the item like this, this risks duplicating the item in multiplayer. See below for the correct approach.
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_TileInteraction(i, j), ItemID.WaterBucket);
				tileEntity.WaterFillLevel = 0;
				*/
				SoundEngine.PlaySound(SoundID.Drown);

				// This example follows the convention of similar vanilla tile entities that drop items: When right clicked, the tile will be "mined" and the item is released by the KillTile code running on the server, which will automatically spawn the item and adjust the tile entity data on the server preventing various potential network syncing issues.
				WorldGen.KillTile(i, j, fail: true);
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// This is sending the KillTile manipulation for the (i, j) coordinates. number4 being 1f means KillTile should fail. This is what will cause KillTile to run on the server, ultimately spawning the Water Bucket item and setting WaterFillLevel back to 0. 
					NetMessage.SendData(MessageID.TileManipulation, number: 0, number2: i, number3: j, number4: 1f);
				}
				// If you don't want to use TileManipulation and KillTile, you may send a custom packet instead. Whatever you do, running the code on the server ensures that multiple users interacting with the tile does not result in duplicate item spawns due to network latency or desync.
			}
			else {
				// If not full, show a chat message showing the fill percentage.
				Main.NewText(StatusText.Format(tileEntity.WaterFillPercentage));
			}

			return true;
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			// Some tile entities that hold items, like Weapon Rack and Item Frame, release their item when hit with a pickaxe. When doing this they will not be destroyed on the first hit no matter how strong the pickaxe is.
			// Others like Hat Rack and Mannequin do not release the item and prevent the tile from being mined at all.

			// This example releases the item.
			if (TileEntity.TryGet(i, j, out BasicTileEntity tileEntity) && tileEntity.WaterFillPercentage == 100) {
				// When KillTile is called on this tile but we have a Water Bucket to give, we prevent the tile from being mined by setting fail to true.
				fail = true;
				// Note that fail might already be set to true, such as when RightClick or the TileManipulation network message sent from RightClick calls KillTile. We still check for WaterFillPercentage though, to ensure that this tile should still drop the item.

				// The item is only spawned on the server or in single player. In multiplayer the KillTile is synced so this code will automatically spawn the item on the server and sync the WaterFillLevel changes to the clients.
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					// By convention, EntitySource_TileBreak is used for TileEntity dropping items instead of EntitySource_TileInteraction
					Item.NewItem(new EntitySource_TileBreak(i, j), tileEntity.Position.X * 16, tileEntity.Position.Y * 16, 32, 32, ItemID.WaterBucket);
					tileEntity.WaterFillLevel = 0;
				}

				if (Main.netMode != NetmodeID.Server) {
					// This helps prevent accidentally mining the tile completely in one mouse click. The user will be forced to click again if they want to actually mine the tile as well.
					Main.LocalPlayer.InterruptItemUsageIfOverTile(Type);
				}

				SoundEngine.PlaySound(SoundID.Drown);
			}
		}
	}

	// The item placing the tile does not require any custom logic.
	public class BasicTileEntityItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<BasicTileEntityTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 9)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
