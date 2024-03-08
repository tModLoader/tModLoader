using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.PlayerTexture)]
#endif
partial class PlayerTextureID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(PlayerTextureID), typeof(int));
}
