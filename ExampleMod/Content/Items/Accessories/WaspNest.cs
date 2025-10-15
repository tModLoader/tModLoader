using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
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
			WaspNestPlayer waspNestPlayer = player.GetModPlayer<WaspNestPlayer>();
			waspNestPlayer.strongBeesUpgrade = true;
			waspNestPlayer.strongBeesItem = Item;
		}

		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
			// Don't allow Hive Pack and Wasp Nest to be equipped at the same time.
			return incomingItem.type != ItemID.HiveBackpack;
		}

		public override bool ModifyEquipTextureDraw(ref PlayerDrawSet drawInfo, ref DrawData drawData, EquipType type, int slot, string memberName) {
			if (drawInfo.drawPlayer.direction == -1) {
				drawData.color = Color.Green;
			}
			if (drawInfo.drawPlayer.direction == 1 && Main.mouseLeft) {
				drawData.texture = backB.Value;
			}
			if (slot != Item.backSlot) {
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
		public Item strongBeesItem; // Some effects need to refer back to the accessory Item

		public override void ResetEffects() {
			strongBeesUpgrade = false;
			strongBeesItem = null;
		}

		// Spawn a bee when damaged, similar to the Honey Comb accessory effect
		// This is just an example of the concept of storing an Item instance for accessory effects and GetSource_Accessory or GetSource_Accessory_OnHurt,
		// if you actually are making an accessory with the existing Honey Comb effect, just set "player.honeyCombItem = Item;" in UpdateAccessory instead
		public override void OnHurt(Player.HurtInfo info) {
			if (Player.whoAmI != Main.myPlayer) {
				return;
			}

			if (strongBeesItem != null && Main.rand.NextBool(3)) {
				int baseDamage = 20;

				// By storing the Item instance, we can create varying effects for different "tiers" of accessories. 
				if (strongBeesItem.ModItem is WaspNest) {
					baseDamage += 10;
				}

				/*
				if (strongBeesItem.ModItem is WaspNestV2) {
					baseDamage += 30;
				}
				*/

				IEntitySource projectileSource_Accessory = Player.GetSource_Accessory_OnHurt(strongBeesItem, info.DamageSource);
				Projectile.NewProjectile(projectileSource_Accessory, Player.Center, Utils.NextVector2Circular(Main.rand, 3, 3), Player.beeType(), Player.beeDamage(baseDamage), Player.beeKB(0f), Main.myPlayer);
			}
		}
	}
}