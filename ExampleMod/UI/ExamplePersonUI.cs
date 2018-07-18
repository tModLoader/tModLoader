using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace ExampleMod.UI
{
	// This class represents the UIState for our ExamplePerson Awesomeify chat function. It is similar to the Goblin Tinkerer's Reforge function, except it only gives Awesome and ReallyAwesome prefixes. 
	class ExamplePersonUI : UIState
	{
		VanillaItemSlotWrapper vanillaItemSlot;

		public override void OnInitialize()
		{
			vanillaItemSlot = new VanillaItemSlotWrapper(ItemSlot.Context.ChestItem, 0.85f);
			vanillaItemSlot.Left.Pixels = 50;
			vanillaItemSlot.Top.Pixels = 270;
			// Here we limit the items that can be placed in the slot. We are fine with placing an empty item in or a non-empty item that can be prefixed. Calling Prefix(-3) is the way to know if the item in question can take a prefix or not.
			vanillaItemSlot.validItem = item => item.IsAir || !item.IsAir && item.Prefix(-3);
			Append(vanillaItemSlot);
		}

		// OnDeactivate is called when the UserInterface switches to a different state. In this mod, we switch between no state (null) and this state (ExamplePersonUI).
		// Using OnDeactivate is useful for clearing out Item slots and returning them to the player, as we do here.
		public override void OnDeactivate()
		{
			if (!vanillaItemSlot.item.IsAir)
			{
				// QuickSpawnClonedItem will preserve mod data of the item. QuickSpawnItem will just spawn a fresh version of the item, losing the prefix.
				Main.LocalPlayer.QuickSpawnClonedItem(vanillaItemSlot.item, vanillaItemSlot.item.stack);
				// Now that we've spawned the item back onto the player, we reset the item by turning it into air.
				vanillaItemSlot.item.TurnToAir();
			}
			// Note that in ExamplePerson we call .SetState(new UI.ExamplePersonUI());, thereby creating a new instance of this UIState each time. 
			// You could go with a different design, keeping around the same UIState instance if you wanted. This would preserve the UIState between opening and closing. Up to you.
		}

		// Update is called on a UIState while it is the active state of the UserInterface.
		// We use Update to handle automatically closing our UI when the player is no longer talking to our Example Person NPC.
		public override void Update(GameTime gameTime)
		{
			// Don't delete this or the UIElements attached to this UIState will cease to function.
			base.Update(gameTime);

			// talkNPC is the index of the NPC the player is currently talking to. By checking talkNPC, we can tell when the player switches to another NPC or closes the NPC chat dialog.
			if (Main.LocalPlayer.talkNPC == -1 || Main.npc[Main.LocalPlayer.talkNPC].type != ExampleMod.instance.NPCType("Example Person"))
			{
				// When that happens, we can set the state of our UserInterface to null, thereby closing this UIState. This will trigger OnDeactivate above.
				ExampleMod.instance.examplePersonUserInterface.SetState(null);
			}
		}

		bool tickPlayed = false;
		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);

			// Here we have a lot of code. This code is mainly adapted from the vanilla code for the reforge option.
			// This code draws "Place an item here" when no item is in the slot and draws the reforge cost and a reforge button when an item is in the slot.
			// This code could possibly be better as different UIElements that are added and removed, but that's not the main point of this example.
			// If you are making a UI, add UIElements in OnInitialize that act on your ItemSlot or other inputs rather than the non-UIElement approach you see below.

			int slotX = 50;
			int slotY = 270;
			if (!vanillaItemSlot.item.IsAir)
			{
				int awesomePrice = Item.buyPrice(0, 1, 0, 0);

				string costText = Lang.inter[46].Value + ": ";
				string coinsText = "";
				int[] coins = Utils.CoinsSplit(awesomePrice);
				if (coins[3] > 0)
				{
					coinsText = coinsText + "[c/" + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3() + ":" + coins[3] + " " + Lang.inter[15].Value + "] ";
				}
				if (coins[2] > 0)
				{
					coinsText = coinsText + "[c/" + Colors.AlphaDarken(Colors.CoinGold).Hex3() + ":" + coins[2] + " " + Lang.inter[16].Value + "] ";
				}
				if (coins[1] > 0)
				{
					coinsText = coinsText + "[c/" + Colors.AlphaDarken(Colors.CoinSilver).Hex3() + ":" + coins[1] + " " + Lang.inter[17].Value + "] ";
				}
				if (coins[0] > 0)
				{
					coinsText = coinsText + "[c/" + Colors.AlphaDarken(Colors.CoinCopper).Hex3() + ":" + coins[0] + " " + Lang.inter[18].Value + "] ";
				}
				ItemSlot.DrawSavings(Main.spriteBatch, slotX + 130, Main.instance.invBottom, true);
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, costText, new Vector2((slotX + 50), slotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, coinsText, new Vector2((slotX + 50) + Main.fontMouseText.MeasureString(costText).X, (float)slotY), Color.White, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
				int reforgeX = slotX + 70;
				int reforgeY = slotY + 40;
				bool hoveringOverReforgeButton = Main.mouseX > reforgeX - 15 && Main.mouseX < reforgeX + 15 && Main.mouseY > reforgeY - 15 && Main.mouseY < reforgeY + 15 && !PlayerInput.IgnoreMouseInterface;
				Texture2D reforgeTexture = Main.reforgeTexture[hoveringOverReforgeButton ? 1 : 0];
				Main.spriteBatch.Draw(reforgeTexture, new Vector2(reforgeX, reforgeY), null, Color.White, 0f, reforgeTexture.Size() / 2f, 0.8f, SpriteEffects.None, 0f);
				if (hoveringOverReforgeButton)
				{
					Main.hoverItemName = Lang.inter[19].Value;
					if (!tickPlayed)
					{
						Main.PlaySound(12, -1, -1, 1, 1f, 0f);
					}
					tickPlayed = true;
					Main.LocalPlayer.mouseInterface = true;
					if (Main.mouseLeftRelease && Main.mouseLeft && Main.LocalPlayer.CanBuyItem(awesomePrice, -1) && ItemLoader.PreReforge(vanillaItemSlot.item))
					{
						Main.LocalPlayer.BuyItem(awesomePrice, -1);
						bool favorited = vanillaItemSlot.item.favorited;
						int stack = vanillaItemSlot.item.stack;
						Item reforgeItem = new Item();
						reforgeItem.netDefaults(vanillaItemSlot.item.netID);
						reforgeItem = reforgeItem.CloneWithModdedDataFrom(vanillaItemSlot.item);
						// This is the main effect of this slot. Giving the Awesome prefix 90% of the time and the ReallyAwesome prefix the other 10% of the time. All for a constant 1 gold. Useless, but informative.
						if (Main.rand.NextBool(10))
						{
							reforgeItem.Prefix(ExampleMod.instance.PrefixType("ReallyAwesome"));
						}
						else
						{
							reforgeItem.Prefix(ExampleMod.instance.PrefixType("Awesome"));
						}
						vanillaItemSlot.item = reforgeItem.Clone();
						vanillaItemSlot.item.position.X = Main.player[Main.myPlayer].position.X + (float)(Main.player[Main.myPlayer].width / 2) - (float)(vanillaItemSlot.item.width / 2);
						vanillaItemSlot.item.position.Y = Main.player[Main.myPlayer].position.Y + (float)(Main.player[Main.myPlayer].height / 2) - (float)(vanillaItemSlot.item.height / 2);
						vanillaItemSlot.item.favorited = favorited;
						vanillaItemSlot.item.stack = stack;
						ItemLoader.PostReforge(vanillaItemSlot.item);
						ItemText.NewText(vanillaItemSlot.item, vanillaItemSlot.item.stack, true, false);
						Main.PlaySound(SoundID.Item37, -1, -1);
					}
				}
				else
				{
					tickPlayed = false;
				}
			}
			else
			{
				string message = "Place an item here to Awesomeify";
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, message, new Vector2(slotX + 50, slotY), new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), 0f, Vector2.Zero, Vector2.One, -1f, 2f);
			}
		}
	}
}
