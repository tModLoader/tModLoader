using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.Systems
{
	public class ExampleMapLayer : ModMapLayer
	{
		public override void Draw(ref MapOverlayDrawContext context, ref string text) {
			const float scaleIfNotSelected = 1f;
			const float scaleIfSelected = scaleIfNotSelected * 2f;
			Main.instance.LoadItem(ItemID.BoneKey);
			var dungeonTexture = TextureAssets.NpcHeadBoss[19].Value;
			if (context.Draw(dungeonTexture, new Vector2(Main.dungeonX, Main.dungeonY), Color.White, new SpriteFrame(1, 1, 0, 0), scaleIfNotSelected, scaleIfSelected, Alignment.Center).IsMouseOver) {
				text = Language.GetTextValue("Bestiary_Biomes.TheDungeon");
			}
		}
	}
}