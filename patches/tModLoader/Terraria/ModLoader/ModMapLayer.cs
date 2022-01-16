using Microsoft.Xna.Framework;
using System;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.Map;

namespace Terraria.ModLoader
{
	public abstract class ModMapLayer : ModType, IMapLayer
	{
		public bool Visible { get; set; } = true;
		public void Hide() => Visible = false;

		public abstract void Draw(ref MapOverlayDrawContext context, ref string text);

		protected sealed override void Register() {
			ModTypeLookup<ModMapLayer>.Register(this);
			Main.MapIcons.AddLayer(this);
		}
	}
}
