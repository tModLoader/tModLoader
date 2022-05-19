/*
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class PatronModPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static PatronModPlayer Player(Player player)
			=> player.GetModPlayer<PatronModPlayer>();


		// TODO: figure out how this was meant to work. Separate the hair and face layers if necessary, otherwise add a new layer for the arm rendering, pointed at the vanilla layer, and and control the visibility

		public override void ModifyDrawLayers(IReadOnlyDictionary<string, IReadOnlyList<PlayerDrawLayer>> layers, PlayerDrawSet drawInfo) {
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
		}
	}
}
*/