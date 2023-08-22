using ExampleMod.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Common.Systems
{
	public class ExampleWorldHeaderSystem : ModSystem
	{
		public override void SaveWorldHeader(TagCompound tag) {
			tag["ExampleModExists"] = true;
		}

		public override void Load() {
			On_UIWorldListItem.DrawSelf += (orig, self, spriteBatch) => {
				orig(self, spriteBatch);
				DrawWorldSelectItemOverlay(self, spriteBatch);
			};
		}

		private void DrawWorldSelectItemOverlay(UIWorldListItem uiItem, SpriteBatch spriteBatch) {
			if (MenuLoader.CurrentMenu is not ExampleModMenu)
				return;

			if (!uiItem.Data.TryGetHeaderData(this, out var data) || !data.GetBool("ExampleModExists"))
				return;

			var dims = uiItem.GetInnerDimensions();
			var pos = new Vector2(dims.X + 400, dims.Y);
			Utils.DrawBorderString(spriteBatch, "EM played before", pos, Color.BlueViolet);
		}
	}
}
