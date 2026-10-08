using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader;

public class ModPylonMapIconTest : ModPylon
{
	public Asset<Texture2D> mapIcon;

	public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
	{
		bool mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale, out bool onScreen);
		DefaultMapClickHandle(mouseOver, onScreen, pylonInfo, "Mods.TestData.MapIcon", ref mouseOverText);
	}
}
