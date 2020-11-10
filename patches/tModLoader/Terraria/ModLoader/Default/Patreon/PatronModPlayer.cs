using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class PatronModPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static PatronModPlayer Player(Player player)
			=> player.GetModPlayer<PatronModPlayer>();

		public bool OrianSet;

		public override void ResetEffects() {
			OrianSet = false;
		}

		public override void PostUpdate() {
			if (OrianSet) {
				var close = Main.npc.Any(x => x.active && !x.friendly && !NPCID.Sets.TownCritter[x.type] && x.type != NPCID.TargetDummy && x.Distance(player.position) <= 300);
				if (close) Lighting.AddLight(player.Center, Color.DeepSkyBlue.ToVector3() * 1.5f);
			}
		}


		// TODO: figure out how this was meant to work. Separate the hair and face layers if necessary, otherwise add a new layer for the arm rendering, pointed at the vanilla layer, and and control the visibility

		/*public override void ModifyDrawLayers(IReadOnlyDictionary<string, IReadOnlyList<PlayerDrawLayer>> layers, PlayerDrawSet drawInfo) {
			if (player.head == Mod.GetEquipSlot("toplayz_Head", EquipType.Head)) {
				// If not falling or swinging frames
				if (player.bodyFrame.Y != 5 * 56
					&& player.bodyFrame.Y != 1 * 56
					&& player.bodyFrame.Y != 2 * 56) {

					PlayerDrawLayer.ArmOverItem.constraint = new PlayerDrawLayer.LayerConstraint(PlayerDrawLayer.Head, true);
				}

				// If falling frame
				// TODO doesnt work, we need to be able to disable drawing the default head.
				// or, the head needs to be separated in a face and hair layer
				//					else if (player.bodyFrame.Y == 5 * 56)
				//					{
				//						// Move head before body frame to prevent clipping
				//						int bodyIndex = layers.IndexOf(bodyLayer);
				//						layers.Remove(headLayer);
				//						layers.Insert(bodyIndex, headLayer);
				//					}
			}
		}*/
	}
}