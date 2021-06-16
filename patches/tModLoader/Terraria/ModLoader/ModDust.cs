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
	[Autoload(Side = ModSide.Client)]
	public abstract class ModDust : ModTexturedType
	{
		/// <summary> Allows you to choose a type of dust for this type of dust to copy the behavior of. Defaults to -1, which means that no behavior is copied. </summary>
		public int UpdateType { get; set; } = -1;

		/// <summary> The sprite sheet that this type of dust uses. Normally a sprite sheet will consist of a vertical alignment of three 10 x 10 pixel squares, each one containing a possible look for the dust. </summary>
		public Texture2D Texture2D { get; private set; }

		/// <summary> The ID of this type of dust. </summary>
		public int Type { get; internal set; }

		protected override sealed void Register() {
			ModTypeLookup<ModDust>.Register(this);

			DustLoader.dusts.Add(this);

			Type = DustLoader.ReserveDustID();

			Texture2D = !string.IsNullOrEmpty(Texture) ? ModContent.Request<Texture2D>(Texture).Value : TextureAssets.Dust.Value;
		}

		internal void Draw(Dust dust, Color alpha, float scale) {
			Main.spriteBatch.Draw(Texture2D, dust.position - Main.screenPosition, dust.frame, alpha, dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);

			if (dust.color != default) {
				Main.spriteBatch.Draw(Texture2D, dust.position - Main.screenPosition, dust.frame, dust.GetColor(alpha), dust.rotation, new Vector2(4f, 4f), scale, SpriteEffects.None, 0f);
			}

			if (alpha == Color.Black) {
				dust.active = false;
			}
		}

		public sealed override void SetupContent() => SetDefaults();

		/// <summary>
		/// Allows you to set this ModDust's updateType field and modify the Terraria.GameContent.ChildSafety.SafeDust array.
		/// </summary>
		public virtual void SetDefaults() { }

		/// <summary>
		/// Allows you to modify a dust's fields when it is created.
		/// </summary>
		public virtual void OnSpawn(Dust dust) { }

		/// <summary>
		/// Allows you to customize how you want this type of dust to behave. Return true to allow for vanilla dust updating to also take place; will return true by default. Normally you will want this to return false.
		/// </summary>
		public virtual bool Update(Dust dust) => true;

		/// <summary>
		/// Allows you to add behavior to this dust on top of the default dust behavior. Return true if you're applying your own behavior; return false to make the dust slow down by itself. Normally you will want this to return true.
		/// </summary>
		public virtual bool MidUpdate(Dust dust) => false;

		/// <summary>
		/// Allows you to override the color this dust will draw in. Return null to draw it in the normal light color; returns null by default. Note that the dust.noLight field makes the dust ignore lighting and draw in full brightness, and can be set in OnSpawn instead of having to return Color.White here.
		/// </summary>
		public virtual Color? GetAlpha(Dust dust, Color lightColor) => null;
	}
}
