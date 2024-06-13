using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Back)]
	public class WaspNest : ModItem
	{
		private static Asset<Texture2D> backB;

		// Only gets run once per type
		public override void Load() {
			IL_Player.beeType += HookBeeType;

			backB = ModContent.Request<Texture2D>($"{Texture}_{EquipType.Back}_B");
		}

		// This IL editing (Intermediate Language editing) example is walked through in the guide: https://github.com/tModLoader/tModLoader/wiki/Expert-IL-Editing#example---hive-pack-upgrade
		private static void HookBeeType(ILContext il) {
			try {
				ILCursor c = new ILCursor(il);

				// Try to find where 566 is placed onto the stack
				c.GotoNext(i => i.MatchLdcI4(566));

				// Move the cursor after 566 and onto the ret op.
				c.Index++;
				// Push the Player instance onto the stack
				c.Emit(OpCodes.Ldarg_0);
				// Call a delegate using the int and Player from the stack.
				c.EmitDelegate<Func<int, Player, int>>((returnValue, player) => {
					// Regular c# code
					if (player.GetModPlayer<WaspNestPlayer>().strongBeesUpgrade && Main.rand.NextBool(10) && Main.ProjectileUpdateLoopIndex == -1) {
						return ProjectileID.Beenade;
					}

					return returnValue;
				});
			}
			catch (Exception e) {
				// If there are any failures with the IL editing, this method will dump the IL to Logs/ILDumps/{Mod Name}/{Method Name}.txt
				MonoModHooks.DumpIL(ModContent.GetInstance<ExampleMod>(), il);

				// If the mod cannot run without the IL hook, throw an exception instead. The exception will call DumpIL internally
				// throw new ILPatchFailureException(ModContent.GetInstance<ExampleMod>(), il, e);
			}
		}

		public override void SetDefaults() {
			int realBackSlot = Item.backSlot;
			Item.CloneDefaults(ItemID.HiveBackpack);
			Item.value = Item.sellPrice(0, 5);
			// CloneDefaults will clear out the autoloaded Back slot, so we need to preserve it this way.
			Item.backSlot = realBackSlot;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// The original Hive Pack sets strongBees.
			player.strongBees = true;
			// Here we add an additional effect
			player.GetModPlayer<WaspNestPlayer>().strongBeesUpgrade = true;
		}

		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
			// Don't allow Hive Pack and Wasp Nest to be equipped at the same time.
			return incomingItem.type != ItemID.HiveBackpack;
		}

		public override bool ModifyEquipTextureDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, EquipType type, int slot, string memberName) {
			if (drawInfo.drawPlayer.direction == -1) {
				drawData.color = Color.Blue;
			}
			if(drawInfo.drawPlayer.direction == 1 && Main.mouseLeft) {
				drawData.texture = backB.Value;
			}
			if(slot != Item.backSlot) {
				// We can detect Non-default equiptexture via slot parameter, in case an item has multiple.
			}

			if (Main.mouseRight) {
				// Example taken from Chromatic Cloak as a test.

				Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
				Rectangle value = bodyFrame;
				value.Width = 2;
				int num2 = 0;
				int num3 = bodyFrame.Width / 2;
				int num4 = 2;
				if (drawInfo.playerEffect.HasFlag(SpriteEffects.FlipHorizontally)) {
					num2 = bodyFrame.Width - 2;
					num4 = -2;
				}

				for (int i = 0; i < num3; i++) {
					value.X = bodyFrame.X + 2 * i;
					Color immuneAlpha = drawInfo.drawPlayer.GetImmuneAlpha(LiquidRenderer.GetShimmerGlitterColor(top: true, (float)i / 16f, 0f), drawInfo.shadow);
					immuneAlpha *= (float)(int)drawInfo.colorArmorBody.A / 255f;
					DrawData item = new DrawData(backB.Value, drawData.position + new Vector2(num2 + i * num4, -50f), value, immuneAlpha, drawInfo.drawPlayer.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect);
					item.shader = drawInfo.cBack;
					drawInfo.DrawDataCache.Add(item);
				}
				return false;
			}

			return true;
		}
	}

	public class WaspNestPlayer : ModPlayer
	{
		public bool strongBeesUpgrade;

		public override void ResetEffects() {
			strongBeesUpgrade = false;
		}
	}
}