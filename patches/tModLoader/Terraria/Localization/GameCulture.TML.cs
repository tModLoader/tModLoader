using System.IO;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.Localization;

public partial class GameCulture
{
	/// <summary>
	/// Derives a culture and shared prefix from a localization file path. Prefix will be found after culture, either separated by an underscore or nested in the folder.
	/// <br/> Some examples:<code>
	/// Localization/en-US_Mods.ExampleMod.hjson
	/// Localization/en-US/Mods.ExampleMod.hjson
	/// en-US_Mods.ExampleMod.hjson
	/// en-US/Mods.ExampleMod.hjson
	/// </code>
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static (GameCulture culture, string prefix) FromPath(string path)
	{
		path = Path.ChangeExtension(path, null);

		GameCulture culture = null;
		string prefix = null;

		string[] splitByFolder = path.Split("/");
		for (int folderSplitIndex = 0; folderSplitIndex < splitByFolder.Length; folderSplitIndex++) {
			string pathPart = splitByFolder[folderSplitIndex];

			string[] splitByUnderscore = pathPart.Split("_");
			for (int underscoreSplitIndex = 0; underscoreSplitIndex < splitByUnderscore.Length; underscoreSplitIndex++) {
				string underscorePart = splitByUnderscore[underscoreSplitIndex];
				GameCulture parsedCulture = _legacyCultures.Values.FirstOrDefault(culture => culture.Name == underscorePart);
				if (parsedCulture != null) {
					culture = parsedCulture;
					continue;
				}
				if(parsedCulture == null && culture != null) {
					prefix = underscorePart;
					return (culture, prefix);
				}
			}
		}
		if(culture != null) {
			return (culture, "");
		}
		/*
		for (int index = split.Length - 1; index >= 0; index--) {
			string pathPart = split[index];
			
			GameCulture culture = _legacyCultures.Values.FirstOrDefault(culture => culture.Name == pathPart);
			if (culture != null)
				return culture;
		}
		*/
		// TODO: Log message warning of localization file erroneously named
		Logging.tML.Warn($"The localization file {path} doesn't match expected file naming patterns, it will load as English");

		return (DefaultCulture, "");
	}
}