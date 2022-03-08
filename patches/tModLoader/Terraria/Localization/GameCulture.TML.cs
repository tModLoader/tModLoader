using System.IO;
using System.Linq;

namespace Terraria.Localization;

public partial class GameCulture
{
	public static GameCulture FromPath(string path) {
		path = Path.ChangeExtension(path, null);

		string[] split = path.Split("/");
		for (int index = split.Length - 1; index >= 0; index--) {
			string pathPart = split[index];
			GameCulture culture = _legacyCultures.Values.FirstOrDefault(culture => culture.Name == pathPart);
			if (culture != null)
				return culture;
		}

		return DefaultCulture;
	}
}