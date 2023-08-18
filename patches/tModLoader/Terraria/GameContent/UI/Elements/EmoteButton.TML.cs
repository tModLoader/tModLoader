using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.GameContent.UI.Elements;

public partial class EmoteButton
{
	public ref int FrameCounter => ref _frameCounter;
	public bool Hovered => _hovered;
	public Asset<Texture2D> BubbleTexture => _bubbleTexture;
	public Asset<Texture2D> EmoteTexture => _texture;
	public Asset<Texture2D> BorderTexture => _textureBorder;
}
