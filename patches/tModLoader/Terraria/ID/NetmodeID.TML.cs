using ReLogic.Reflection;

namespace Terraria.ID;

#if TMLCODEASSIST
[tModCodeAssist.IDType.Sets.AssociatedName(ModLoader.Annotations.IDTypeAttribute.Netmode)]
#endif
partial class NetmodeID
{
	public static readonly IdDictionary Search = IdDictionary.Create(typeof(NetmodeID), typeof(int));
}
