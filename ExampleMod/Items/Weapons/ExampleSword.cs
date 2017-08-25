using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace ExampleMod.Items.Weapons
{
	public class ExampleSword : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("This is a modded sword.");	//The (English) text shown below your weapon's name
		}

		private Vector2 boxSize;
		const int paddingForBox = 10;

		public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
		{
			// Draw a background box
			var texts = lines.Select(z => z.text);
			string longestString = texts.ToList().OrderByDescending(z => z.Length).First();
			int widthForBox = (int)Main.fontMouseText.MeasureString(longestString).X + paddingForBox * 2;
			int heightForBox = (int)texts.ToList().Sum(z => Main.fontMouseText.MeasureString(z).Y) + paddingForBox;
			boxSize = new Vector2(widthForBox, heightForBox);
			Vector2 drawPosForBox = new Vector2(x - paddingForBox, y - paddingForBox);
			Rectangle drawRectForBox = new Rectangle((int)drawPosForBox.X, (int)drawPosForBox.Y, widthForBox, heightForBox);
			Main.spriteBatch.Draw(Main.magicPixel, drawRectForBox, Main.mouseTextColorReal);

			string text = "This drwas on the original position with PreDrawTooltip";
			Vector2 drawPos = new Vector2(x, y - Main.fontMouseText.MeasureString(text).Y);
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, text,
				drawPos, Main.mouseTextColorReal, 0f, Vector2.Zero, Vector2.One, -1f, 2f);

			// Offset the X and Y for fun
			const int offset = 32;
			x += offset;
			y += offset;

			return true;
		}

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
			if (!line.OneDropLogo)
			{
				// Draw item name centered, and some separator
				string sepText = "-----";
				float off = line.font.MeasureString(sepText).Y;

				if (line.text == item.HoverName)
				{
					float boxOff = boxSize.X / 2 - paddingForBox;

					ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.font, sepText,
						new Vector2(line.X + boxOff - line.font.MeasureString(sepText).X/2 , line.Y + off/2), Color.Green, line.rotation, line.origin, line.baseScale);

					line.X += (int) boxOff - (int)line.font.MeasureString(line.text).X/2;
					yOffset = (int) off/3;
				}
				else
				{
					yOffset = 0;
				}
			}
			return true;
		}

		public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
		{
			// Draw some text after the entire tooltip
			string text = "This draws on PostDrawTooltip, affected by the offset in PreDrawTooltip";
			Vector2 drawPos = new Vector2(lines.Last().X, lines.Last().Y) + new Vector2(0, Main.fontMouseText.MeasureString(text).Y);
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText, text,
				drawPos, Main.mouseTextColorReal, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
		}

		public override void PostDrawTooltipLine(DrawableTooltipLine line)
		{
			// Duplicate every line to the right, consider using it for item comparison windows
			if (!line.OneDropLogo)
			{
				Vector2 drawPos = new Vector2(line.X + boxSize.X, line.Y);
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontMouseText,
					line.text,
					drawPos, line.Color, 0f, Vector2.Zero, Vector2.One, -1f, 2f);
			}
		}

		public override void SetDefaults()
		{
			item.damage = 50;			//The damage of your weapon
			item.melee = true;			//Is your weapon a melee weapon?
			item.width = 40;			//Weapon's texture's width
			item.height = 40;			//Weapon's texture's height
			item.useTime = 20;			//The time span of using the weapon. Remember in terraria, 60 frames is a second.
			item.useAnimation = 20;			//The time span of the using animation of the weapon, suggest set it the same as useTime.
			item.useStyle = 1;			//The use style of weapon, 1 for swinging, 2 for drinking, 3 act like shortsword, 4 for use like life crystal, 5 for use staffs or guns
			item.knockBack = 6;			//The force of knockback of the weapon. Maximum is 20
			item.value = 10000;			//The value of the weapon
			item.rare = 2;				//The rarity of the weapon, from -1 to 13
			item.UseSound = SoundID.Item1;		//The sound when the weapon is using
			item.autoReuse = true;			//Whether the weapon can use automatically by pressing mousebutton
		}

		public override void AddRecipes()
		{
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(null, "ExampleItem", 10);
			recipe.AddTile(null, "ExampleWorkbench");
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void MeleeEffects(Player player, Rectangle hitbox)
		{
			if (Main.rand.Next(3) == 0)
			{
				int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, mod.DustType("Sparkle"));
				//Emit dusts when swing the sword
			}
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
		{
			target.AddBuff(BuffID.OnFire, 60);		//Add Onfire buff to the NPC for 1 second
		}
	}
}
