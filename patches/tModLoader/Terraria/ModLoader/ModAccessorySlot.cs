using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameInput;
using Terraria.ModLoader.Default;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.Audio;

//TODO: Does not Support: ItemLoader.GetGamepadInstructions
//TODO: Does not Support: Multiplayer

namespace Terraria.ModLoader
{
	/// <summary>
	/// A ModAccessorySlot instance represents a net new accessory slot instance. You can store fields in the ModAccessorySlot class. 
	/// </summary>
	public abstract class ModAccessorySlot : ModType
	{
		internal Player Player => Main.LocalPlayer;
		internal virtual bool skipRegister => false;

		private int num20;
		private Microsoft.Xna.Framework.Color color2;

		public int xLoc = 0;
		public int yLoc = 0;

		internal int slot;
		internal int index;
		internal int xColumn;
		internal int yRow;
		internal static int accessoryPerColumn = 5;
		internal bool flag3;
		internal bool flag4;

		/// <summary>
		/// Called when loading characters from parent ModPlayer.
		/// Requests for a modded Slot, and stores assigned Slot. DefaultPlayer will auto-manage slots.
		/// </summary>
		internal protected void Initialize() {
			if (skipRegister) {
				return;
			}

			int pendingID = ModPlayer.moddedAccSlots.IndexOf(this.FullName);
			if (pendingID < 0) {
				pendingID = ModPlayer.moddedAccSlots.Count;
				ModPlayer.moddedAccSlots.Add(this.FullName);
			}

			this.xColumn = (int)(pendingID / accessoryPerColumn) + 1;
			this.yRow = pendingID % accessoryPerColumn;
			this.slot = pendingID;
		}

		protected sealed override void Register() {
			ModTypeLookup<ModAccessorySlot>.Register(this);
			this.Initialize();
		}

		//TODO: the draw code doesn't take into account window resizing, which borks it.
		/// <summary>
		/// Is run after vanilla draws normal accessory slots. Currently doesn't get called.
		/// Creates new accessory slots in a column to the left of vanilla.  
		/// </summary>
		public bool Draw(int num20, int skip) {
			ModPlayer dPlayer = Main.LocalPlayer.GetModPlayer<DefaultPlayer>();
			
			this.PreDraw(num20, dPlayer, skip);
			
			if (flag3) {
				return false;
			}
			if (flag4) {
				return true;
			}

			this.DrawFunctional(dPlayer);
			this.DrawVanity(dPlayer);
			this.DrawDye(dPlayer);
			return true;
		}

		/// <summary>
		/// Is run first in this.Draw. 
		/// Initializes all fields that are used in subsequent draw events.
		/// For Overriding fields see this.PreDrawCustom
		/// </summary>
		internal void PreDraw(int num20, ModPlayer dPlayer, int skip) {
			this.num20 = num20;
			this.flag3 = !EquipLoader.ModdedIsAValidEquipmentSlotForIteration(slot);
			this.PreDrawCustomization(dPlayer, skip);
		}

		/// <summary>
		/// Is run in this.PreDraw(). Applies Xloc and Yloc data for the slot, based on ModPlayer.scrollSlots
		/// </summary>
		internal void PreDrawCustomization(ModPlayer dPlayer, int skip) {
			flag4 = false;

			if (ModPlayer.scrollSlots) {
				int row = yRow + (xColumn - 1) * accessoryPerColumn - ModPlayer.scrollbarSlotPosition - skip;

				this.yLoc = (int)((float)(num20) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
				int chkMax = (int)((float)(num20) + (float)(((accessoryPerColumn - 1) + 3) * 56) * Main.inventoryScale) + 4;
				int chkMin = (int)((float)(num20) + (float)((0 + 3) * 56) * Main.inventoryScale) + 4;

				if (yLoc > chkMax || yLoc < chkMin) {
					flag4 = true;
					return;
				}

				this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3 - 50;
			}

			else {
				int row = yRow, tempSlot = slot, col = xColumn;
				if (skip > 0) {
					tempSlot -= skip;
					row = tempSlot % accessoryPerColumn;
					col = tempSlot / accessoryPerColumn + 1;
				}

				this.yLoc = (int)((float)(num20) + (float)((row + 3) * 56) * Main.inventoryScale) + 4;
				this.xLoc = Main.screenWidth - 64 - 28 - 47 * 3 * col - 50; // 47*3 is per column, 50 adjusts to not overlap vanilla UI
			}

			this.color2 = new Microsoft.Xna.Framework.Color(80, 80, 80, 80);
		}
		
		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// Also includes creating hidevisibilitybutton.
		/// </summary>
		internal void DrawFunctional(ModPlayer dPlayer) {
			int yLoc2 = yLoc - 2;
			int xLoc1 = xLoc;
			int xLoc2 = xLoc1 - 58 + 64 + 28;
			int context = -10;

			Texture2D value4 = TextureAssets.InventoryTickOn.Value;
			if (dPlayer.exHideAccessory[slot])
				value4 = TextureAssets.InventoryTickOff.Value;

			Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle(xLoc2, yLoc2, value4.Width, value4.Height);
			int num45 = 0;
			if (rectangle.Contains(new Microsoft.Xna.Framework.Point(Main.mouseX, Main.mouseY)) && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				if (Main.mouseLeft && Main.mouseLeftRelease) {
					dPlayer.exHideAccessory[slot] = !dPlayer.exHideAccessory[slot];
					SoundEngine.PlaySound(12);
					if (Main.netMode == 1)
						NetMessage.SendData(4, -1, -1, null, Player.whoAmI); //blindly called, won't work
				}

				num45 = ((!dPlayer.exHideAccessory[slot]) ? 1 : 2);
			}

			else if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {

				Main.armorHide = true;
				Player.mouseInterface = true;
				ItemSlot.OverrideHover(dPlayer.exAccessorySlot, Math.Abs(context), slot);
				if (!flag3 || Main.mouseItem.IsAir)
					ItemSlot.LeftClick(dPlayer.exAccessorySlot, context, slot);

				ItemSlot.MouseHover(dPlayer.exAccessorySlot, Math.Abs(context), slot);
			}
			
			this.DrawRedirect(dPlayer.exAccessorySlot, context, slot, new Vector2(xLoc1, yLoc));
			
			Main.spriteBatch.Draw(value4, new Vector2(xLoc2, yLoc2), Microsoft.Xna.Framework.Color.White * 0.7f);

			if (num45 > 0) {
				Main.HoverItem = new Item();
				Main.hoverItemName = Lang.inter[58 + num45].Value;
			}
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		internal void DrawVanity(ModPlayer dPlayer) {
			bool flag7 = flag3 && !Main.mouseItem.IsAir;
			int vSlot = slot + ModPlayer.moddedAccSlots.Count;
			int xLoc1 = xLoc - 47;
			int context = -11;

			if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(dPlayer.exAccessorySlot, Math.Abs(context), vSlot);
				if (!flag7) {
					ItemSlot.LeftClick(dPlayer.exAccessorySlot, context, vSlot);
					ItemSlot.RightClick(dPlayer.exAccessorySlot, Math.Abs(context), vSlot);
				}

				ItemSlot.MouseHover(dPlayer.exAccessorySlot, Math.Abs(context), vSlot);
			}

			this.DrawRedirect(dPlayer.exAccessorySlot, context, vSlot, new Vector2(xLoc1, yLoc));
		}

		/// <summary>
		/// Is run in this.Draw. 
		/// Generates a significant amount of functionality for the slot, despite being named drawing because vanilla.
		/// At the end, calls this.DrawModded() where you can override to have custom drawing code for visuals.
		/// </summary>
		internal void DrawDye(ModPlayer dPlayer) {
			bool flag8 = flag3 && !Main.mouseItem.IsAir;
			int xLoc1 = xLoc - 47 * 2;
			int context = -12;

			if (Main.mouseX >= xLoc1 && (float)Main.mouseX <= (float)xLoc1 + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale && Main.mouseY >= yLoc
				&& (float)Main.mouseY <= (float)yLoc + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale && !PlayerInput.IgnoreMouseInterface) {
				Player.mouseInterface = true;
				Main.armorHide = true;
				ItemSlot.OverrideHover(dPlayer.exDyesAccessory, Math.Abs(context), slot);
				if (!flag8) {
					if (Main.mouseRightRelease && Main.mouseRight)
						ItemSlot.RightClick(dPlayer.exDyesAccessory, Math.Abs(context), slot);

					ItemSlot.LeftClick(dPlayer.exDyesAccessory, context, slot);
				}

				ItemSlot.MouseHover(dPlayer.exDyesAccessory, Math.Abs(context), slot);
			}
			this.DrawRedirect(dPlayer.exDyesAccessory, context, slot, new Vector2(xLoc1, yLoc));
		}

		internal void DrawRedirect(Item[] inv, int context, int slot, Vector2 position) {
			if (!this.DrawModded(inv, context, slot, position))
				EquipLoader.DefaultDrawModSlots(Main.spriteBatch, inv, context, slot, position); 
		}

		/// <summary>
		/// This function allows for custom textures and colours to be drawn for the accessory slot. Called for Dyes, Vanity, and Functionals.
		/// Runs in place of EquipLoader.DefaultDrawModSlots if this returns true.
		/// Receives data:
		/// <para><paramref name="inv"/> :: the array containing all accessory slots, yours is inv[slot] </para>
		/// <para><paramref name="slot"/> :: which is the index for inventory that you were assigned </para>
		/// <para><paramref name="position"/> :: is the position of where the ItemSlot will be drawn </para>
		/// <para><paramref name="context"/> :: 12 => dye; 11 => vanity; 10 => functional </para>
		/// </summary>
		public virtual bool DrawModded(Item[] inv, int context, int slot, Vector2 position) {
			return false;
		}

		/// <summary>
		/// Override to set conditions on when the slot is available, Example: the demonHeart is consumed and in Expert mode in Vanilla.
		/// </summary>
		public virtual bool CanUseSlot() {
			return true;
		}

		/// <summary>
		/// Override to set conditions on what can go in slot. Return false to prevent the item going in slot. Example: only wings can go in slot.
		/// Receives data:
		/// <para><paramref name="checkItem"/> :: the item that is attempting to enter the slot </para>
		/// </summary>
		public virtual bool LimitWhatCanGoInSlot(Item checkItem) {
			return true;
		}
	}
}
