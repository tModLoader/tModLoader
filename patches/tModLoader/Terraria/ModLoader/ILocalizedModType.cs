using System.Runtime.CompilerServices;
using Terraria.Localization;

namespace Terraria.ModLoader;

public interface ILocalizedModType : IModType
{
	public abstract string Category { get; }
}

public static class ILocalizedModTypeExtensions
{
	// Idea: use [CallerMemberName]? 
	public static LocalizedText GetLocalizedText(this ILocalizedModType self, string suffix)
	{
		string key = $"Mods.{self.Mod.Name}.{self.Category}.{self.Name}.{suffix}";

		if(!LanguageManager.Instance.Exists(key))
			LanguageManager.Instance._localizedTexts.Add(key, new LocalizedText(key, key));

		return LanguageManager.Instance.GetText(key);
	}
}
