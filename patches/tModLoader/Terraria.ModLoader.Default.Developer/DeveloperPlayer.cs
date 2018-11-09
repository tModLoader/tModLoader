using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer
{
	internal class DeveloperPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static DeveloperPlayer GetPlayer(Player player)
			=> player.GetModPlayer<DeveloperPlayer>();

		public bool AndromedonSet;

		public override void ResetEffects()
		{
			AndromedonSet = false;
		}

		public override void UpdateDead()
		{
			AndromedonSet = false;
		}

		public override void PostUpdate()
		{
			if (!AndromedonSet)
			{
				if (AndromedonItem.LayerStrength >= 0
				    && Main.rand.NextBool(2))
				{
					AndromedonItem.LayerStrength -= 0.02f;
				}
			}
			else
			{
				if (AndromedonItem.LayerStrength <= 1
				    && Main.rand.NextBool(2))
				{
					AndromedonItem.LayerStrength += 0.02f;
				}
			}

			if (AndromedonItem.LayerStrength >= 0f)
			{
				Lighting.AddLight(
					player.Center,
					Main.DiscoColor.ToVector3() * AndromedonItem.LayerStrength * ((float)Main.time%2) * (float)Math.Abs(Math.Log10(Main.essScale * 0.75f)));
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
		{
			//if (AndromedonSet)
			//{
			//	drawInfo.legColor = Main.DiscoColor;
			//	drawInfo.bodyColor = Main.DiscoColor;
			//	drawInfo.faceColor = Main.DiscoColor;
			//}
		}

		public override void ModifyDrawLayers(List<PlayerLayer> layers)
		{
			if (AndromedonItem.LayerStrength >= 0f)
			{
				PowerRanger_Head.GlowLayer.visible = true;
				PowerRanger_Head.ShaderLayer.visible = true;
				int i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Head"));
				if (i != -1)
				{
					layers.Insert(i - 1, PowerRanger_Head.ShaderLayer);
					layers.Insert(i + 2, PowerRanger_Head.GlowLayer);
				}

				PowerRanger_Body.GlowLayer.visible = true;
				PowerRanger_Body.ShaderLayer.visible = true;
				i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Body"));
				if (i != -1)
				{
					layers.Insert(i - 1, PowerRanger_Body.ShaderLayer);
				}

				i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Arms"));
				if (i != -1)
				{
					
					layers.Insert(i + 1, PowerRanger_Body.GlowLayer);
				}

				PowerRanger_Legs.GlowLayer.visible = true;
				PowerRanger_Legs.ShaderLayer.visible = true;
				i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Legs"));
				if (i != -1)
				{
					layers.Insert(i - 1, PowerRanger_Legs.ShaderLayer);
					layers.Insert(i + 2, PowerRanger_Legs.GlowLayer);
				}
			}
		}
	}
}
