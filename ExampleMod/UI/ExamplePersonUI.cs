using System.Text;
using ExampleMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.UI
{
	// This class represents the UIState for our ExamplePerson Awesomeify chat function. It is similar to the Goblin Tinkerer's Reforge function, except it only gives Awesome and ReallyAwesome prefixes. 
	internal class ExamplePersonUI : UIState
	{
		private VanillaItemSlotWrapper _vanillaItemSlot;

		public override void OnInitialize() {
			_vanillaItemSlot = new VanillaItemSlotWrapper(ItemSlot.Context.BankItem, 0.85f) {
				Left = { Pixels = 50 },
				Top = { Pixels = 270 },
				ValidItemFunc = item => item.IsAir || !item.IsAir && item.Prefix(-3)
			};

			// Here we limit the items that can be placed in the slot. We are fine with placing an empty item in or a non-empty item that can be prefixed. Calling Prefix(-3) is the way to know if the item in question can take a prefix or not.
			Append(_vanillaItemSlot);
		}

		// OnDeactivate is called when the UserInterface switches to a different state. In this mod, we switch between no state (null) and this state (ExamplePersonUI).
		// Using OnDeactivate is useful for clearing out Item slots and returning them to the player, as we do here.
		public override void OnDeactivate() {
			if (_vanillaItemSlot.Item.IsAir) {
				return;
			}

			// QuickSpawnClonedItem will preserve mod data of the item. QuickSpawnItem will just spawn a fresh version of the item, losing the prefix.
			Main.LocalPlayer.QuickSpawnClonedItem(_vanillaItemSlot.Item, _vanillaItemSlot.Item.stack);

			// Now that we've spawned the item back onto the player, we reset the item by turning it into air.
			_vanillaItemSlot.Item.TurnToAir();

			// Note that in ExamplePerson we call .SetState(new UI.ExamplePersonUI());, thereby creating a new instance of this UIState each time. 
			// You could go with a different design, keeping around the same UIState instance if you wanted. This would preserve the UIState between opening and closing. Up to you.
		}

		// Update is called on a UIState while it is the active state of the UserInterface.
		// We use Update to handle automatically closing our UI when the player is no longer talking to our Example Person NPC.
		public override void Update(GameTime gameTime) {
			// Don't delete this or the UIElements attached to this UIState will cease to function.
			base.Update(gameTime);

			// talkNPC is the index of the NPC the player is currently talking to. By checking talkNPC, we can tell when the player switches to another NPC or closes the NPC chat dialog.
			if (Main.LocalPlayer.talkNPC == -1 || Main.npc[Main.LocalPlayer.talkNPC].type != NPCType<ExamplePerson>()) {
				// When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
				GetInstance<ExampleMod>().ExamplePersonUserInterface.SetState(null);
			}
		}

		private bool tickPlayed;

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);

			// This will hide the crafting menu similar to the reforge menu. For best results this UI is placed before "Vanilla: Inventory" to prevent 1 frame of the craft menu showing.
			Main.HidePlayerCraftingMenu = true;

			// Here we have a lot of code. This code is mainly adapted from the vanilla code for the reforge option.
			// This code draws "Place an item here" when no item is in the slot and draws the reforge cost and a reforge button when an item is in the slot.
			// This code could possibly be better as different UIElements that are added and removed, but that's not the main point of this example.
			// If you are making a UI, add UIElements in OnInitialize that act on your ItemSlot or other inputs rather than the non-UIElement approach you see below.

			const int SlotX = 50;
			const int SlotY = 270;

			if (_vanillaItemSlot.Item.IsAir) {
				const string Message = "Place an item here to Awesomeify";

				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, Message, new Vector2(SlotX + 50, SlotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				return;
			}

			int awesomePrice = Item.buyPrice(0, 1, 0, 0);
			string costText = Language.GetTextValue("LegacyInterface.46") + ": ";
			int[] coins = Utils.CoinsSplit(awesomePrice);
			var coinsText = new StringBuilder();

			for (int i = 0; i < 4; i++) {
				coinsText.Append($"[c/{Colors.AlphaDarken(Colors.CoinPlatinum).Hex3()}:{coins[3 - i]} {Language.GetTextValue($"LegacyInterface.{15 + i}")}]");
			}

			ItemSlot.DrawSavings(Main.spriteBatch, SlotX + 130, Main.instance.invBottom, true);
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, costText, new Vector2(SlotX + 50, SlotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, coinsText.ToString(), new Vector2(SlotX + 50 + FontAssets.MouseText.Value.MeasureString(costText).X, (float)SlotY), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);

			int reforgeX = SlotX + 70;
			int reforgeY = SlotY + 40;
			bool hoveringOverReforgeButton = Main.mouseX > reforgeX - 15 && Main.mouseX < reforgeX + 15 && Main.mouseY > reforgeY - 15 && Main.mouseY < reforgeY + 15 && !PlayerInput.IgnoreMouseInterface;
			Texture2D reforgeTexture = TextureAssets.Reforge[hoveringOverReforgeButton ? 1 : 0].Value;

			Main.spriteBatch.Draw(reforgeTexture, new Vector2(reforgeX, reforgeY), null, Color.White, 0f, reforgeTexture.Size() / 2f, 0.8f, SpriteEffects.None, 0f);

			if (!hoveringOverReforgeButton) {
				tickPlayed = false;
				return;
			}

			Main.hoverItemName = Language.GetTextValue("LegacyInterface.19");

			if (!tickPlayed) {
				SoundEngine.PlaySound(SoundID.MenuTick, -1, -1, 1, 1f, 0f);
			}

			tickPlayed = true;
			Main.LocalPlayer.mouseInterface = true;

			if (!Main.mouseLeftRelease || !Main.mouseLeft || !Main.LocalPlayer.CanBuyItem(awesomePrice, -1) || !ItemLoader.PreReforge(_vanillaItemSlot.Item)) {
				return;
			}

			Main.LocalPlayer.BuyItem(awesomePrice, -1);

			bool favorited = _vanillaItemSlot.Item.favorited;
			int stack = _vanillaItemSlot.Item.stack;

			Item reforgeItem = new Item();

			reforgeItem.netDefaults(_vanillaItemSlot.Item.netID);

			reforgeItem = reforgeItem.CloneWithModdedDataFrom(_vanillaItemSlot.Item);

			// This is the main effect of this slot. Giving the Awesome prefix 90% of the time and the ReallyAwesome prefix the other 10% of the time. All for a constant 1 gold. Useless, but informative.
			if (Main.rand.NextBool(10)) {
				reforgeItem.Prefix(GetInstance<ExampleMod>().PrefixType("ReallyAwesome"));
			}
			else {
				reforgeItem.Prefix(GetInstance<ExampleMod>().PrefixType("Awesome"));
			}

			_vanillaItemSlot.Item = reforgeItem.Clone();
			_vanillaItemSlot.Item.position.X = Main.LocalPlayer.position.X + (float)(Main.LocalPlayer.width / 2) - (float)(_vanillaItemSlot.Item.width / 2);
			_vanillaItemSlot.Item.position.Y = Main.LocalPlayer.position.Y + (float)(Main.LocalPlayer.height / 2) - (float)(_vanillaItemSlot.Item.height / 2);
			_vanillaItemSlot.Item.favorited = favorited;
			_vanillaItemSlot.Item.stack = stack;

			ItemLoader.PostReforge(_vanillaItemSlot.Item);
			PopupText.NewText(PopupTextContext.RegularItemPickup, _vanillaItemSlot.Item, _vanillaItemSlot.Item.stack, true, false);
			SoundEngine.PlaySound(SoundID.Item37, -1, -1);
		}
	}
}
