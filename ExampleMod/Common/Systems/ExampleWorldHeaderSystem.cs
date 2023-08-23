using ExampleMod.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
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

	public class ExampleWorldHeaderPlayer : ModPlayer {
		public override void OnEnterWorld() {
			// modsGeneratedWith can only be checked in Single Player
			if (Main.netMode == NetmodeID.SinglePlayer ) {
				// Check if this world was generated with at least a specific version of a mod.
				// Tracking mods used to generate a world was added in v2023.8, so if modsGeneratedWith is null we don't know for sure if ExampleMod was enabled when this world was generated.
				if (Main.ActiveWorldFileData.modsGeneratedWith?.Any(x => x.modName == "ExampleMod" && x.modVersion >= new System.Version(1, 0)) == false) {
					Main.NewText(Language.GetTextValue(Mod.GetLocalizationKey("NotPresentDuringWorldGenMessage")), Color.Orange);
				}
			}
		}
	}
}
