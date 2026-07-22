namespace Terraria.GameContent.UI.Elements;

public partial class UICyclingImage
{
	//These are exposed to allow for modded special seeds to add their own textures.
	internal int CurrentTextureIndex { get => _currentTextureIndex; set => _currentTextureIndex = value; }
	internal int FramesCounted { get => _framesCounted; set => _framesCounted = value; }
	internal int TextureCount => _textureAssets.Count;
}