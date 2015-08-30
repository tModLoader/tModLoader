using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	public class ModGore
	{
		private static int nextGore = GoreID.Count;
		internal static readonly IDictionary<string, int> gores = new Dictionary<string, int>();
		internal static readonly IDictionary<int, ModGore> modGores = new Dictionary<int, ModGore>();

		internal static int ReserveGoreID()
		{
			int reserveID = nextGore;
			nextGore++;
			return reserveID;
		}

		public static int GetGoreSlot(string texture)
		{
			if (gores.ContainsKey(texture))
			{
				return gores[texture];
			}
			else
			{
				return 0;
			}
		}
		//in Terraria.GameContent.ChildSafety make SafeGore internal and not readonly
		internal static void ResizeAndFillArrays()
		{
			Array.Resize(ref Main.goreLoaded, nextGore);
			Array.Resize(ref Main.goreTexture, nextGore);
			Array.Resize(ref ChildSafety.SafeGore, nextGore);
			for (int k = GoreID.Count; k < nextGore; k++)
			{
				Main.goreLoaded[k] = true;
			}
			foreach (string texture in gores.Keys)
			{
				Main.goreTexture[gores[texture]] = ModLoader.GetTexture(texture);
			}
		}

		internal static void Unload()
		{
			gores.Clear();
			nextGore = GoreID.Count;
		}
		//in Terraria.Gore add modGore property (internal set)
		//in Terraria.Gore.NewGore after resetting properties call ModGore.SetupGore(Main.gore[num]);
		internal static void SetupGore(Gore gore)
		{
			if (modGores.ContainsKey(gore.type))
			{
				gore.modGore = modGores[gore.type];
				gore.modGore.OnSpawn(gore);
			}
			else
			{
				gore.modGore = null;
			}
		}
		//in Terraria.Main.DrawGore and DrawGoreBehind replace type checks with this
		internal static bool DrawBackGore(Gore gore)
		{
			if (gore.modGore != null)
			{
				return gore.modGore.DrawBehind(gore);
			}
			return gore.type >= 706 && gore.type <= 717 && (gore.frame < 7 || gore.frame > 9);
		}

		public virtual void OnSpawn(Gore gore)
		{
		}
		//in Terraria.Gore.Update at beginning of if block checking for active add
		//  if(this.modGore != null && !this.modGore.Update(this)) { return; }
		public virtual bool Update(Gore gore)
		{
			return true;
		}
		//at beginning of Terraria.Gore.Update add
		//  if(this.modGore != null) { Color? modColor = this.modGore.GetAlpha(this, newColor);
		//    if(modColor.HasValue) { return modColor.Value; } }
		public virtual Color? GetAlpha(Gore gore, Color lightColor)
		{
			return null;
		}

		public virtual bool DrawBehind(Gore gore)
		{
			return false;
		}
	}
}
