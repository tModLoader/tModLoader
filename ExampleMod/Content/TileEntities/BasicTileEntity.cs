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
	public class BasicTileEntity : ModTileEntity
	{
		private bool syncNeeded; // Used to track when a network sync is needed

		// This water barrel will fill up in 1 minute of rain, or 3600 game updates.
		private const int MaxFill = 3600;
		private const int SyncInterval = MaxFill / 10;

		// We use a property to consolidate logic clamping water level value and triggering a network sync.
		private int waterFillLevel;
		public int WaterFillLevel {
			get { return waterFillLevel; }
			set {
				int newFillLevel = Math.Clamp(value, 0, MaxFill);
				if (waterFillLevel / SyncInterval != newFillLevel / SyncInterval) {
					// To reduce network spam while raining, we only sync this tile entity at 10% fill intervals.
					// This may or may not be the correct approach for other ModTileEntity, depending on how accurate of
					// data the client will need access to.
					syncNeeded = true;
				}
				waterFillLevel = newFillLevel;
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
			syncNeeded = false;
		}

		public override void NetReceive(BinaryReader reader) {
			WaterFillLevel = reader.ReadInt32();
			// Debugging messages can help verify that the data is properly syncing to clients.
			/*
			Main.NewText($"NetReceive called, Position: ({Position.X}, {Position.Y}), WaterFillLevel: {WaterFillLevel}");
			*/
		}

		// Tile Entities update every game update. Regular tiles can only ever update at random intervals.
		public override void Update() {
			if (Main.raining) {
				WaterFillLevel += 1;
			}
			// Update does not run for multiplayer clients, changes other clients need requires syncing the data.
			if (syncNeeded) {
				// The TileEntitySharing message will trigger NetSend, manually syncing the changed data.
				NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
			}
		}

		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<BasicTileEntityTile>();
		}
	}

	// BasicTileEntityTile is the Tile that BasicTileEntityTile attaches to.
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

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);

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
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_TileInteraction(i, j), ItemID.WaterBucket);
				tileEntity.WaterFillLevel = 0;
				SoundEngine.PlaySound(SoundID.Drown);

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					// The server is in charge of the Tile Entity, we need to inform the server that we took all the water so that the data stays in sync on other clients.
					var packet = Mod.GetPacket();
					packet.Write((byte)ExampleMod.MessageType.BasicTileEntityClaimWater);
					packet.Write(tileEntity.ID);
					packet.Send();
				}
			}
			Main.NewText(StatusText.Format(tileEntity.WaterFillPercentage));

			return true;
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
