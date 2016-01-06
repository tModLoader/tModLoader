using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class ModDust
	{
		private static int nextDust = DustID.Count;
		internal static readonly IDictionary<int, ModDust> dusts = new Dictionary<int, ModDust>();
		public int updateType = -1;

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

		public int Type
		{
			get;
			internal set;
		}

		public static ModDust GetDust(int type)
		{
			if (dusts.ContainsKey(type))
			{
				return dusts[type];
			}
			return null;
		}

		internal static int ReserveDustID()
		{
			int reserveID = nextDust;
			nextDust++;
			return reserveID;
		}
		//make Terraria.GameContent.ChildSafety.SafeDust public and not readonly
		internal static void ResizeArrays()
		{
			Array.Resize(ref ChildSafety.SafeDust, nextDust);
			for (int k = DustID.Count; k < nextDust; k++)
			{
				ChildSafety.SafeDust[k] = true;
			}
		}

		internal static void Unload()
		{
			dusts.Clear();
			nextDust = DustID.Count;
		}
		//in Terraria.Dust.NewDust after initializing dust properties call ModDust.SetupDust(dust);
		internal static void SetupDust(Dust dust)
		{
			ModDust modDust = GetDust(dust.type);
			if (modDust != null)
			{
				dust.frame.X = 0;
				dust.frame.Y %= 30;
				modDust.OnSpawn(dust);
			}
		}
		//in Terraria.Dust.UpdateDust after incrementing Dust.dCount call this
		internal static void SetupUpdateType(Dust dust)
		{
			ModDust modDust = GetDust(dust.type);
			if (modDust != null && modDust.updateType >= 0)
			{
				dust.realType = dust.type;
				dust.type = modDust.updateType;
			}
		}
		//in Terraria.Dust.UdpateDust at end of dust update code call this
		internal static void TakeDownUpdateType(Dust dust)
		{
			if (dust.realType >= 0)
			{
				dust.type = dust.realType;
				dust.realType = -1;
			}
		}
		//in Terraria.Main.DrawDust before universal dust drawing call
		//  ModDust modDust = ModDust.GetDust(dust.type);
		//  if(modDust != null) { modDust.Draw(dust, color5, scale); continue; }
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

		public virtual void SetDefaults()
		{
		}

		public virtual void OnSpawn(Dust dust)
		{
		}
		//in Terraria.Dust.UpdateDust after setting up update type add
		//  ModDust modDust = ModDust.GetDust(dust.type);
		//  if(modDust != null && !modDust.Update(dust)) { ModDust.TakeDownUpdateType(dust); continue; }
		public virtual bool Update(Dust dust)
		{
			return true;
		}

		public virtual bool MidUpdate(Dust dust)
		{
			return false;
		}
		//in beginning of Terraria.Dust.GetAlpha add
		//  ModDust modDust = ModDust.GetDust(this.type);
		//  if(modDust != null)
		//  {
		//      Color? modColor = modDust.GetAlpha(this, newColor);
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
