namespace Terraria.GameContent.UI.Elements;

public partial class UICyclingImage
{
	//Added to allow modded seeds to modify texture frames.
	internal int CurrentTextureIndex => _currentTextureIndex;
	internal int TextureCount => _textureAssets.Count;
	internal int FramesCounted => _framesCounted;
}