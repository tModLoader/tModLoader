using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class PatronModPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static PatronModPlayer Player(Player player)
			=> player.GetModPlayer<PatronModPlayer>();

		public bool OrianSet;
		public bool GuildpackSet;
		public bool SaetharSet;

		public override void ResetEffects() {
			OrianSet = false;
			GuildpackSet = false;
			SaetharSet = false;
		}

		public override void PostUpdate() {
			if (OrianSet) {
				var close = Main.npc.Any(x => x.active && !x.friendly && !NPCID.Sets.TownCritter[x.type] && x.type != NPCID.TargetDummy && x.Distance(player.position) <= 300);
				if (close) Lighting.AddLight(player.Center, Color.DeepSkyBlue.ToVector3() * 1.5f);
			}
		}

		public override void ModifyDrawLayers(List<PlayerLayer> layers) {
			if (player.head == mod.GetEquipSlot("toplayz_Head", EquipType.Head)) {
				var headLayer = layers.FirstOrDefault(x => x.Name.Equals("Head"));
				var armsLayer = layers.FirstOrDefault(x => x.Name.Equals("Arms"));
				//				var bodyLayer = layers.FirstOrDefault(x => x.Name.Equals("Body"));

				if (headLayer != null
					&& armsLayer != null)
				//				    && bodyLayer != null)
				{
					// If not falling or swinging frames
					if (player.bodyFrame.Y != 5 * 56
						&& player.bodyFrame.Y != 1 * 56
						&& player.bodyFrame.Y != 2 * 56) {
						// Move arms layer before head layer
						int armsIndex = layers.IndexOf(armsLayer);
						layers.Remove(headLayer);
						layers.Insert(armsIndex, headLayer);
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
			if (GuildpackSet) {
				Guildpack_Head.MiscEffectsBack.visible = true;
				layers.Insert(0, Guildpack_Head.MiscEffectsBack);
				int headLayerIndex = layers.FindIndex(x => x == PlayerLayer.Head);
				if (headLayerIndex != -1)
					layers.Insert(headLayerIndex + 1, Guildpack_Head.EyeGlow);
			}
			if (SaetharSet) {
				int headLayerIndex = layers.FindIndex(x => x == PlayerLayer.Head);
				if (headLayerIndex != -1)
					layers.Insert(headLayerIndex + 1, Saethar_Head.EyeGlow);
			}
		}
	}
}