using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	public class ExampleDefenseBuff : ModBuff
	{
		public override void SetDefaults() {
			DisplayName.SetDefault("Defensive Buff");
			Description.SetDefault("Grants +4 defense.");
			Main.buffNoTimeDisplay[Type] = false; //Set this to true so the remaining buff time should not be displayed (by default it also won't get displayed if buffTime is less than 3).
			Main.debuff[Type] = false; //Set this to true so the nurse doesn't remove the buff when healing.
		}

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense += 4; //Grant a +4 defense boost to the player while the buff is active.
		}

		//Optional
		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
			//You can use this hook to make something special happen when the buff icon is drawn (such as reposition it, pick a different texture, etc.).
			//Here we make the icon have a lime green tint.
			drawParams.drawColor = Color.LimeGreen * Main.buffAlpha[buffIndex];
			
			//Return true to let the game draw the buff icon.
			return true;
		}
	}
}
