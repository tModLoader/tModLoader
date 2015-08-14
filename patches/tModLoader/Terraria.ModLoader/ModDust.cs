using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Terraria.ModLoader
{
	public class ModDust
	{
		//in Terraria.Dust add ModDust property (internal set)
		//in Terraria.Dust.NewDust set dust.modDust to null
		//in Terraria.Dust.CloneDust copy modDust property
		public string Name
		{
			get;
			internal set;
		}

		public Texture2D Texture
		{
			get;
			internal set;
		}

		public Mod mod
		{
			get;
			internal set;
		}

		public static int NewDust(Vector2 Position, int Width, int Height, Mod mod, string name, float SpeedX = 0f, float SpeedY = 0f, int Alpha = 0, Color newColor = default(Color), float Scale = 1f)
		{
			int dust = Dust.NewDust(Position, Width, Height, 0, SpeedX, SpeedY, Alpha, newColor, Scale);
			Main.dust[dust].modDust = mod.dusts[name];
			mod.dusts[name].OnSpawn(Main.dust[dust]);
			return dust;
		}
		//in Terraria.Main.DrawDust before universal dust drawing call
		//  if(dust.modDust != null) { dust.modDust.Draw(dust, color5, scale); continue; }
		internal void Draw(Dust dust, Color alpha, float scale)
		{
			Main.spriteBatch.Draw(Texture, dust.position - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(dust.frame), alpha, dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
			if (dust.color != default(Microsoft.Xna.Framework.Color))
			{
				Main.spriteBatch.Draw(Texture, dust.position - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(dust.frame), dust.GetColor(alpha), dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
			}
			if (alpha == Microsoft.Xna.Framework.Color.Black)
			{
				dust.active = false;
			}
		}

		public virtual bool Autoload(ref string name, ref string texture)
		{
			return mod.Properties.Autoload;
		}

		public virtual void OnSpawn(Dust dust)
		{
		}
		//in Terraria.Dust.UpdateDust after incrementing Dust.dCount call
		//  if(dust.modDust != null && !dust.modDust.Update(dust)) { continue; }
		public virtual bool Update(Dust dust)
		{
			return true;
		}
		//in beginning of Terraria.Dust.GetAlpha add
		//  if(this.modDust != null)
		//  {
		//      Color? modColor = this.modDust.GetAlpha(this, newColor);
		//      if(modColor.HasValue)
		//      {
		//          return modColor.Value;
		//      }
		//  }
		public virtual Color? GetAlpha(Dust dust, Color lightColor)
		{
			return null;
		}
	}
}
