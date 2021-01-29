using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDefenseBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Defensive Buff");
			Description.SetDefault("Grants +4 defense.");
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false; //Set this to true so the nurse doesn't remove the buff when healing
		}

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense += 4; //Grant a +4 defense boost to the player while the buff is active.
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref Vector2 drawPosition, ref Rectangle sourceRectangle, ref Rectangle mouseRectangle, ref Color drawColor) {

			drawColor = Color.LimeGreen * (Main.buffAlpha[buffIndex] / 255f);

			return true;
		}
	}
}