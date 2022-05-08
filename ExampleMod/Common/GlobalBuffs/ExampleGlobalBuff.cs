﻿using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalBuffs
{
	// Showcases how to work with all buffs
	public class ExampleGlobalBuff : GlobalBuff
	{
		public override void Update(int type, Player player, ref int buffIndex) {
			// If the player gets the Chilled debuff while he already has more than 5 other buffs/debuffs, limit the max duration to 3 seconds
			if (type == BuffID.Chilled && buffIndex >= 5) {
				int limit = 3 * 60;
				if (player.buffTime[buffIndex] > limit) {
					player.buffTime[buffIndex] = limit;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int type, int buffIndex, ref BuffDrawParams drawParams) {
			// Make the campfire buff have a different color and shake slightly
			if (type == BuffID.Campfire) {
				drawParams.DrawColor = Main.DiscoColor * Main.buffAlpha[buffIndex];

				Vector2 shake = new Vector2(Main.rand.Next(-2, 3), Main.rand.Next(-2, 3));

				drawParams.Position += shake;
				drawParams.TextPosition += shake;
			}

			// If the buff is not drawn in the hook/mount/pet equip page, and the buff is one of the three specified:
			if (Main.EquipPage != 2 && (type == BuffID.Regeneration || type == BuffID.Ironskin || type == BuffID.Swiftness)) {
				// Make text go up and down 6 pixels on each buff, offset by 4 ticks for each
				int interval = 60;
				float time = ((int)Main.GameUpdateCount + 4 * buffIndex) % interval / (float)interval;

				int offset = (int)(6 * time);

				ref Vector2 textPos = ref drawParams.TextPosition; // You can use ref locals to keep modifying the same variable
				textPos.Y += offset;
			}

			// Return true to let the game draw the buff icon.
			return true;
		}

		public override void ModifyBuffTip(int type, ref string tip, ref int rare) {
			// This code adds a more extensible remaining time tooltip for suitable buffs
			Player player = Main.LocalPlayer;

			int buffIndex = player.FindBuffIndex(type);
			if (buffIndex < 0 || buffIndex >= player.buffTime.Length) {
				return;
			}

			if (Main.TryGetBuffTime(buffIndex, out int buffTimeValue) && buffTimeValue > 2) {
				string text = Lang.LocalizedDuration(new System.TimeSpan(0, 0, buffTimeValue / 60), abbreviated: false, showAllAvailableUnits: true);
				tip += $"\n[{nameof(ExampleGlobalBuff)}] Remaining time: " + text;
			}
		}

		public override bool RightClick(int type, int buffIndex) {
			// This code makes it so while the player is standing still, he cannot remove the "ExampleDefenseBuff" by right clicking the icon
			if (type == ModContent.BuffType<ExampleDefenseBuff>() && Main.LocalPlayer.velocity == Vector2.Zero) {
				Main.NewText("Cannot cancel this buff while stationary!");
				return false;
			}

			return base.RightClick(type, buffIndex);
		}
	}
}
