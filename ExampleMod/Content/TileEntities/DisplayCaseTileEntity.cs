using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.Tile_Entities;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.UI;

namespace ExampleMod.Content.TileEntities
{
	// This example shows a few advanced Tile Entity features that are commonly asked about. This example is basically a copy of the Hat Rack tile except it holds potions instead of hat items. Players can place potions on this tile to display them. 
	// The first concept is item storage. This tile entity stores 6 items and shows the player a chest like interface when interacted with. Shift click an....
	public class DisplayCaseTileEntity : ModTileEntity
	{
		private const int Capacity = 6;
		internal Item[] items;

		private static int hatTargetSlot;

		public DisplayCaseTileEntity() {
			items = new Item[Capacity];
			for (int i = 0; i < Capacity; i++) {
				items[i] = new Item();
			}
		}

		public override void SaveData(TagCompound tag) {
			tag[nameof(items)] = items;
		}

		public override void LoadData(TagCompound tag) {
			items = tag.Get<Item[]>(nameof(items));
		}

		public override void NetSend(BinaryWriter writer) {
			for (int i = 0; i < Capacity; i++) {
				ItemIO.Send(items[i], writer, writeStack: true);
			}
		}

		public override void NetReceive(BinaryReader reader) {
			for (int i = 0; i < Capacity; i++) {
				items[i] = ItemIO.Receive(reader, readStack: true);
			}

			// Debugging messages can help verify that the data is properly syncing to clients.

			string data = $"NetReceive called, Position: ({Position.X}, {Position.Y}), items: {string.Join(",", items.Select(x => x.HoverName))}";
			Main.NewText(data);
			Console.WriteLine(data);
		}

		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<DisplayCaseTile>();
		}

		public override void OnPlayerUpdate(Player player) {
			// If the player leaves the interaction range, opens a chest, or talks to an npc, we stop interacting with this Tile Entity.
			if (!DisplayCaseTile.InInteractionRange(player, player.tileEntityAnchor.X, player.tileEntityAnchor.Y, TileReachCheckSettings.Simple) || player.chest != -1 || player.talkNPC != -1) {
				if (player.chest == -1 && player.talkNPC == -1) {
					SoundEngine.PlaySound(SoundID.MenuClose);
				}

				player.tileEntityAnchor.Clear();
				Recipe.FindRecipes();
			}
		}

		public override void OnInventoryDraw(Player player, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[player.tileEntityAnchor.X, player.tileEntityAnchor.Y];
			if (tile.TileType != ModContent.TileType<DisplayCaseTile>()) {
				player.tileEntityAnchor.Clear();
				Recipe.FindRecipes();
			}
			else {
				int style = TileObjectData.GetTileStyle(tile);
				DrawInventory(player, spriteBatch, style);
			}
		}

		private void DrawInventory(Player player, SpriteBatch spriteBatch, int style) {
			// This UI code is an example of what is referred to as "immediate mode" UI. This means the UI rendering and interaction is done directly in the code. This is in contrast to what is referred to as "retained mode" UI, where a UI is constructed from objects (UIElement) and persist in memory.
			// See https://en.wikipedia.org/wiki/Immediate_mode_(computer_graphics) and https://en.wikipedia.org/wiki/Retained_mode for more information.
			// Both approaches have merit, and both are used in Terraria. Most of the gameplay UI in Terraria is "immediate mode", while most of the menus are "retained mode".
			// This UI could certainly be done with UIState and UIElements instead, but this example shows the "immediate mode" approach to keep things simple and matching vanilla code as much as possible.

			Main.inventoryScale = 0.72f;
			// Draw a inventory background panel and a custom background for the tile style
			int top = (int)(Main.instance.invBottom + 1.5f * 56f * Main.inventoryScale);
			var inventoryBackgroundPosition = new Vector2(180, top);
			var backgroundTextureScale = new Vector2(3);
			var backgroundTextureFrame = DisplayCaseTile.inventoryBackgroundTexture.Frame(2, 1, style, 0, -2);
			var inventoryBackgroundDimensions = new Rectangle((int)inventoryBackgroundPosition.X, (int)inventoryBackgroundPosition.Y, (int)(backgroundTextureFrame.Width * backgroundTextureScale.X), (int)(backgroundTextureFrame.Height * backgroundTextureScale.Y));
			inventoryBackgroundDimensions.Inflate(20, 20);
			Utils.DrawInvBG(spriteBatch, inventoryBackgroundDimensions);
			spriteBatch.Draw(DisplayCaseTile.inventoryBackgroundTexture.Value, inventoryBackgroundPosition, backgroundTextureFrame, Color.White * 0.6f, 0,Vector2.Zero, backgroundTextureScale, SpriteEffects.None, 0);

			// Then draw each item slot
			DrawSlots(player, spriteBatch, 6, 0, style, inventoryBackgroundPosition, backgroundTextureScale, ItemSlot.Context.ChestItem /*26*/);
		}

		private void DrawSlots(Player player, SpriteBatch spriteBatch, int slotsToShowLine, int slotsArrayOffset, int tileStyle, Vector2 inventoryBackgroundPosition, Vector2 backgroundTextureScale, int itemSlotContext) {

			var InventoryBack = TextureAssets.InventoryBack;
			float itemSlotWidth = InventoryBack.Width() * Main.inventoryScale;
			float itemSlotHeight = InventoryBack.Height() * Main.inventoryScale;

			Item[] items = this.items;

			for (int i = 0; i < slotsToShowLine; i++) {
				// Draw each item slot mirroring the placement of the items on our display. For a more typical approach, just set x and y based on some math derived from i and how many items per row you want.
				DisplayCaseTile.Placement placement = DisplayCaseTile.placements[tileStyle][i];
				int x = (int)(inventoryBackgroundPosition.X + placement.offset.X * backgroundTextureScale.X);
				int y = (int)(inventoryBackgroundPosition.Y + placement.offset.Y * backgroundTextureScale.Y);

				// Adjust ItemSlot position based off of PlacementAlignment
				Rectangle frame = new Rectangle(0, 0, (int)itemSlotWidth, (int)itemSlotHeight);
				Vector2 drawOrigin = placement.alignment switch {
					DisplayCaseTile.PlacementAlignment.Centered => frame.Center(),
					DisplayCaseTile.PlacementAlignment.Sitting => frame.Bottom(),
					DisplayCaseTile.PlacementAlignment.Hanging => frame.Top(),
					_ => throw new NotImplementedException(),
				};
				x -= (int)drawOrigin.X;
				y -= (int)drawOrigin.Y;

				// ItemSlot.Handle and ItemSlot.Draw handle drawing and updating the slot and the item contained in the slot.
				if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, x, y, itemSlotWidth, itemSlotHeight) && !PlayerInput.IgnoreMouseInterface) {
					player.mouseInterface = true;
					// Handles clicks and hover. OverrideItemSlotHover and OverrideItemSlotLeftClick will be called if this slot is hovered or clicked. They will also be called for inventory slots in a similar manner.
					ItemSlot.Handle(items, itemSlotContext, i + slotsArrayOffset);
				}

				ItemSlot.Draw(spriteBatch, items, itemSlotContext, i + slotsArrayOffset, new Vector2(x, y));
			}
		}

		// While the player is interacting with this Tile Entity, this method will be called for any ItemSlot that is hovered while the shift key is held down.
		public override bool OverrideItemSlotHover(Item[] inv, int context = 0, int slot = 0) {
			Item item = inv[slot];

			// Check if an inventory item slot with a non-favorited item is being hovered. If it is and our tile accepts the item in that slot, we set Main.cursorOverride accordingly and return true to bypass the vanilla logic and checks.
			// Note that we don't check for empty space in the tile entity item array in this example because we swap with existing item slots if there is no room remaining. This mirrors the behavior of Hat Rack and Mannequin. If making a more typical chest, you'll want to check for item space here as well.
			if (!item.IsAir && !inv[slot].favorited && context == ItemSlot.Context.InventoryItem && FitsDisplayCase(item)) {
				Main.cursorOverride = CursorOverrideID.InventoryToChest;
				return true;
			}

			// How would it be HatRackHat?
			if (!item.IsAir && (context == ItemSlot.Context.HatRackHat || context == ItemSlot.Context.HatRackDye) && Main.player[Main.myPlayer].ItemSpace(inv[slot]).CanTakeItemToPersonalInventory) {
				Main.cursorOverride = CursorOverrideID.ChestToInventory;
				return true;
			}

			// Return false to allow vanilla item slot logic to run.
			// Importantly, this includes setting Main.cursorOverride to CursorOverrideID.ChestToInventory if the slot context is ChestItem (which it is for our slots) and there is space in the player's inventory for the item. 
			return false;
		}

		// While the player is interacting with this Tile Entity, this method will be called for any ItemSlot that is clicked.
		public override bool OverrideItemSlotLeftClick(Item[] inv, int context = 0, int slot = 0) {
			// TODO: This is all a bit weird, I wonder if we can't use ChestItem or if we need to handle the item stacking outselves (mouse to displaycase slot stack combine).
			// TODO: Right click stack split will also do this I think...darn.
			// Maybe we need a custom context. We need update to check for item changes and sync too, or after ItemSlot.Handle check for item changes.

			if (!ItemSlot.ShiftInUse) {
				if (context == ItemSlot.Context.ChestItem && inv == items) {
					// TODO: I don't think this is syncing changes when true.
					return !(Main.mouseItem.IsAir || FitsDisplayCase(Main.mouseItem));
				}
				return false;
				return context == ItemSlot.Context.ChestItem && inv == items && !FitsDisplayCase(Main.mouseItem);
				return false;
			}

			if (Main.cursorOverride == CursorOverrideID.InventoryToChest && context == ItemSlot.Context.InventoryItem) {
				Item item = inv[slot];
				if (Main.cursorOverride == CursorOverrideID.InventoryToChest && !item.IsAir && !item.favorited && context == 0 && FitsDisplayCase(item))
					return TryFitting(inv, context, slot);
			}

			if ((Main.cursorOverride == CursorOverrideID.ChestToInventory && context == ItemSlot.Context.DisplayDollArmor) || context == ItemSlot.Context.HatRackHat || context == ItemSlot.Context.HatRackDye) {
				inv[slot] = Main.player[Main.myPlayer].GetItem(Main.myPlayer, inv[slot], GetItemSettings.InventoryEntityToPlayerInventorySettings);
				if (Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.TEHatRackItemSync, -1, -1, null, Main.myPlayer, ID, slot);

				return true;
			}

			return false;
		}

		public override string GetItemGamepadInstructions(int slot = 0) {
			return base.GetItemGamepadInstructions(slot);
		}

		public override bool TryGetItemGamepadOverrideInstructions(Item[] inv, int context, int slot, out string instruction) {
			return base.TryGetItemGamepadOverrideInstructions(inv, context, slot, out instruction);
		}

		public static bool FitsDisplayCase(Item item) {
			return true;
			return item.UseSound?.IsTheSameAs(SoundID.Item3) == true;

			//if (item.maxStack > 1)
			//	return false;

			//return item.headSlot > 0;
		}

		private bool TryFitting(Item[] inv, int context = 0, int slot = 0, bool justCheck = false) {
			if (!FitsDisplayCase(inv[slot]))
				return false;

			if (justCheck)
				return true;

			int num = hatTargetSlot;
			hatTargetSlot++;
			for (int i = 0; i < Capacity; i++) {
				if (items[i].IsAir) {
					num = i;
					hatTargetSlot = i + 1;
					break;
				}
			}

			for (int j = 0; j < Capacity; j++) {
				if (inv[slot].type == items[j].type)
					num = j;
			}

			if (hatTargetSlot >= Capacity)
				hatTargetSlot = 0;

			SoundEngine.PlaySound(SoundID.Grab);
			Utils.Swap(ref items[num], ref inv[slot]);
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.TEHatRackItemSync, number: Main.myPlayer, number2: ID, number3: num);
			}

			return true;
		}
	}



	public class DisplayCaseTile : ModTile
	{
		// Represents how an item at a specific slot will be rendered
		internal class Placement
		{
			public Vector2 offset;
			public PlacementAlignment alignment;
			// TODO: available space.
			public Placement(Vector2 offset, PlacementAlignment align) {
				this.offset = offset;
				this.alignment = align;
			}
		}

		internal enum PlacementAlignment
		{
			Centered,
			Sitting,
			Hanging,
		}

		static int[] capacity = [6, 6];
		internal static Placement[][] placements = [
			[
				new (new(10, 24), PlacementAlignment.Sitting),
				new (new Vector2(24, 24), PlacementAlignment.Sitting),
				new (new Vector2(38, 24), PlacementAlignment.Sitting),
				new (new Vector2(12, 44), PlacementAlignment.Sitting),
				new (new Vector2(24, 44), PlacementAlignment.Sitting),
				new (new Vector2(36, 44), PlacementAlignment.Sitting),
			],
			[
				new (new Vector2(8, 7), PlacementAlignment.Centered),
				new (new Vector2(35, 9), PlacementAlignment.Sitting),
				new (new Vector2(6, 26), PlacementAlignment.Centered),
				new (new Vector2(38, 29), PlacementAlignment.Hanging),
				new (new Vector2(9, 48), PlacementAlignment.Hanging),
				new (new Vector2(37, 48), PlacementAlignment.Hanging),
			],
		];

		public static LocalizedText StatusText { get; private set; }

		internal static Asset<Texture2D> inventoryBackgroundTexture;

		public override void Load() {
			inventoryBackgroundTexture = ModContent.Request<Texture2D>(Texture + "_InventoryBackground");
		}

		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);

			// Tell the tile to place the Tile Entity on the tile after placing it.
			//TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DisplayCaseTileEntity>().Hook_AfterPlacement, -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<DisplayCaseTileEntity>().Generic_HookPostPlaceMyPlayer;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.RandomStyleRange = 2;
			TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<DisplayCaseItem>(), 1);

			// Etc
			//AddMapEntry(new Color(200, 200, 200), CreateMapEntryName(), MapHoverText);

			StatusText = this.GetLocalization(nameof(StatusText));
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) {
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			// When the tile is removed, we need to remove the Tile Entity as well.
			ModContent.GetInstance<DisplayCaseTileEntity>().Kill(i, j);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			//if (TryGetBasicTileEntity(i, j, out DisplayCaseTileEntity tileEntity) && tileEntity.WaterFillPercentage == 100) {
			//	fail = true;

			//	if (Main.netMode != NetmodeID.MultiplayerClient) {
			//		//Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_TileInteraction(i, j), ItemID.WaterBucket);
			//		Item.NewItem(new EntitySource_TileBreak(i, j), tileEntity.Position.X * 16, tileEntity.Position.Y * 16, 32, 32, ItemID.WaterBucket);
			//		tileEntity.WaterFillLevel = 0;
			//	}

			//	if (Main.netMode != NetmodeID.Server) {
			//		Main.LocalPlayer.InterruptItemUsageIfOverTile(Type);
			//	}

			//	SoundEngine.PlaySound(SoundID.Drown);
			//}
		}

		// The following hooks all show accessing the Tile Entity and using it to adjust the behavior and look of this Tile.

		/*public static string MapHoverText(string name, int i, int j) {
			if (TryGetBasicTileEntity(i, j, out DisplayCaseTileEntity tileEntity)) {
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
		}*/

		//public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
		//	if (TryGetBasicTileEntity(i, j, out DisplayCaseTileEntity tileEntity)) {
		//		tileFrameY = (short)(tileFrameY + (tileEntity.WaterFillStage * 38));

		//		// We can uncomment this code to spawn dust at the tile entity position for debugging purposes. Some developer mods also have tools to visualize tile entities.
		//		/*
		//		if (TileObjectData.IsTopLeft(i, j)) {
		//			Dust.QuickDust(tileEntity.Position.X, tileEntity.Position.Y, Color.Green);
		//		}
		//		*/
		//	}
		//}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<DisplayCaseItem>();
		}

		public override bool RightClick(int i, int j) {
			if (!TileEntity.TryGet(i, j, out DisplayCaseTileEntity tileEntity)) {
				return true;
			}

			// BasicOpenCloseInteraction will set the interaction anchor (player.tileEntityAnchor), indicating that the player is interacting with a specific Tile Entity and has exclusive access to it in multiplayer.
			Point16 topLeft = TileObjectData.TopLeft(i, j);
			int interactionX = topLeft.X + 1; // We adjust the interaction point to the center of the tile so that the interaction range is properly centered.
			int interactionY = topLeft.Y + 1;
			TileEntity.BasicOpenCloseInteraction(Main.LocalPlayer, interactionX, interactionY, tileEntity.ID);

			return true;
		}


		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
			if (TileObjectData.IsTopLeft(i, j)) {
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
			if (!TileEntity.TryGet(i, j, out DisplayCaseTileEntity tileEntity)) {
				return;
			}

			Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Tile tile = Main.tile[i, j];
			int tileStyle = TileObjectData.GetTileStyle(tile);

			for (int k = 0; k < 6; k++) {
				Item item = tileEntity.items[k];
				if (item.IsAir) {
					continue;
				}
				Main.instance.LoadItem(item.type);
				Texture2D itemTexture = TextureAssets.Item[item.type].Value;
				Rectangle itemFrame = Main.itemAnimations[item.type]?.GetFrame(itemTexture) ?? itemTexture.Frame(1, 1, 0, 0);

				var placement = placements[tileStyle][k];
				Vector2 offset = placement.offset;

				Vector2 drawOrigin = placement.alignment switch {
					PlacementAlignment.Centered => itemFrame.Center(),
					PlacementAlignment.Sitting => itemFrame.Bottom(),
					PlacementAlignment.Hanging => itemFrame.Top(),
					_ => throw new NotImplementedException(),
				};

				int itemWidth = itemFrame.Width;
				int itemHeight = itemFrame.Height;
				float drawScale = 1f;
				int availableX = 14;
				if (itemWidth > availableX || itemHeight > availableX) {
					if (itemWidth > itemHeight) {
						drawScale = availableX / (float)itemWidth;
					}
					else {
						drawScale = availableX / (float)itemHeight;
					}
				}
				if (itemWidth > itemHeight && placement.alignment == PlacementAlignment.Sitting) {
					offset.Y = offset.Y + drawScale * ((itemWidth - itemHeight) / 2);
				}
				drawScale *= item.scale;
				SpriteEffects effects = SpriteEffects.None;
				Color lightingColor = Lighting.GetColor(i, j);
				Color color20 = lightingColor;
				float scale = 1f;
				ItemSlot.GetItemLight(ref color20, ref scale, item, false);
				drawScale *= scale;
				Main.spriteBatch.Draw(itemTexture, offset + new Vector2((i * 16 - (int)Main.screenPosition.X), (j * 16 - (int)Main.screenPosition.Y)) + zero, itemFrame, color20, 0f, drawOrigin, drawScale, effects, 0f);
				if (item.color != default) {
					Main.spriteBatch.Draw(itemTexture, offset + new Vector2(i * 16 - (int)Main.screenPosition.X + 16, j * 16 - (int)Main.screenPosition.Y + 16) + zero, itemFrame, item.GetColor(lightingColor), 0f, new Vector2(itemWidth / 2, itemHeight / 2), drawScale, effects, 0f);
				}
			}
		}

		public static bool InInteractionRange(Player player, int interactX, int interactY, TileReachCheckSettings settings) {
			(int playerX, int playerY) = player.Center.ToTileCoordinates();
			Tile tile = Main.tile[interactX, interactY];
			settings.GetRanges(player, out var x, out var y);

			// There is an existing Player.InInteractionRange method that we could use, but we made our own to customize the ranges here to account for the width and height of this tile.
			if (playerX >= interactX - x - 1 && playerX <= interactX + x + 1 && playerY >= interactY - y - 1 && playerY <= interactY + y + 3) {
				return true;
			}

			return false;
		}
	}

	// The item placing the tile does not require any custom logic.
	public class DisplayCaseItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<DisplayCaseTile>());
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 9)
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
