using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class represents a type of dust that is added by a mod. Only one instance of this class will ever exist for each type of dust you add.
	/// </summary>
	public class ModDust
	{
		private static int nextDust = DustID.Count;
		internal static readonly IList<ModDust> dusts = new List<ModDust>();
		/// <summary>Allows you to choose a type of dust for this type of dust to copy the behavior of. Defaults to -1, which means that no behavior is copied.</summary>
		public int updateType = -1;

		/// <summary>
		/// The internal name of this type of dust.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The sprite sheet that this type of dust uses. Normally a sprite sheet will consist of a vertical alignment of three 10 x 10 pixel squares, each one containing a possible look for the dust.
		/// </summary>
		public Texture2D Texture {
			get;
			internal set;
		}

		/// <summary>
		/// The mod that added this type of dust.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The ID of this type of dust.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		internal static int DustCount => nextDust;

		/// <summary>
		/// Gets the ModDust instance with the given type. Returns null if no ModDust with the given type exists.
		/// </summary>
		public static ModDust GetDust(int type) {
			return type >= DustID.Count && type < DustCount ? dusts[type - DustID.Count] : null;
		}

		internal static int ReserveDustID() {
			int reserveID = nextDust;
			nextDust++;
			return reserveID;
		}
		//make Terraria.GameContent.ChildSafety.SafeDust public and not readonly
		internal static void ResizeArrays() {
			Array.Resize(ref ChildSafety.SafeDust, nextDust);
			for (int k = DustID.Count; k < nextDust; k++) {
				ChildSafety.SafeDust[k] = true;
			}
		}

		internal static void Unload() {
			dusts.Clear();
			nextDust = DustID.Count;
		}
		//in Terraria.Dust.NewDust after initializing dust properties call ModDust.SetupDust(dust);
		internal static void SetupDust(Dust dust) {
			ModDust modDust = GetDust(dust.type);
			if (modDust != null) {
				dust.frame.X = 0;
				dust.frame.Y %= 30;
				modDust.OnSpawn(dust);
			}
		}
		//in Terraria.Dust.UpdateDust after incrementing Dust.dCount call this
		internal static void SetupUpdateType(Dust dust) {
			ModDust modDust = GetDust(dust.type);
			if (modDust != null && modDust.updateType >= 0) {
				dust.realType = dust.type;
				dust.type = modDust.updateType;
			}
		}
		//in Terraria.Dust.UdpateDust at end of dust update code call this
		internal static void TakeDownUpdateType(Dust dust) {
			if (dust.realType >= 0) {
				dust.type = dust.realType;
				dust.realType = -1;
			}
		}
		//in Terraria.Main.DrawDust before universal dust drawing call
		//  ModDust modDust = ModDust.GetDust(dust.type);
		//  if(modDust != null) { modDust.Draw(dust, color5, scale); continue; }
		internal void Draw(Dust dust, Color alpha, float scale) {
			Main.spriteBatch.Draw(Texture, dust.position - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(dust.frame), alpha, dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
			if (dust.color != default(Microsoft.Xna.Framework.Color)) {
				Main.spriteBatch.Draw(Texture, dust.position - Main.screenPosition, new Microsoft.Xna.Framework.Rectangle?(dust.frame), dust.GetColor(alpha), dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
			}
			if (alpha == Microsoft.Xna.Framework.Color.Black) {
				dust.active = false;
			}
		}

		/// <summary>
		/// Allows you to automatically add a type of dust without having to use Mod.AddDust. By default returns the mod's Autoload property. Return true to automatically add the dust. Name will be initialized to the dust's class name, and Texture will be initialized to the dust's namespace and overriding class name with periods replaced with slashes. The name parameter determines the internal name and the texture parameter determines the texture path.
		/// </summary>
		public virtual bool Autoload(ref string name, ref string texture) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// Allows you to set this ModDust's updateType field and modify the Terraria.GameContent.ChildSafety.SafeDust array.
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to modify a dust's fields when it is created.
		/// </summary>
		public virtual void OnSpawn(Dust dust) {
		}

		//in Terraria.Dust.UpdateDust after setting up update type add
		//  ModDust modDust = ModDust.GetDust(dust.type);
		//  if(modDust != null && !modDust.Update(dust)) { ModDust.TakeDownUpdateType(dust); continue; }
		/// <summary>
		/// Allows you to customize how you want this type of dust to behave. Return true to allow for vanilla dust updating to also take place; will return true by default. Normally you will want this to return false.
		/// </summary>
		public virtual bool Update(Dust dust) {
			return true;
		}

		/// <summary>
		/// Allows you to add behavior to this dust on top of the default dust behavior. Return true if you're applying your own behavior; return false to make the dust slow down by itself. Normally you will want this to return true.
		/// </summary>
		public virtual bool MidUpdate(Dust dust) {
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
		/// <summary>
		/// Allows you to override the color this dust will draw in. Return null to draw it in the normal light color; returns null by default. Note that the dust.noLight field makes the dust ignore lighting and draw in full brightness, and can be set in OnSpawn instead of having to return Color.White here.
		/// </summary>
		public virtual Color? GetAlpha(Dust dust, Color lightColor) {
			return null;
		}
	}
}
