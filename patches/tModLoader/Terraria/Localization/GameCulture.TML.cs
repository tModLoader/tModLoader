using System.Linq;

namespace Terraria.Localization;

public partial class GameCulture
{
	public static GameCulture FromPath(string path) {
		int dotIndex = path.LastIndexOf('.');
		if (dotIndex >= 0)
			path = path[..dotIndex];

		foreach (string pathPart in path.Split("/")) {
			GameCulture culture = _legacyCultures.Values.FirstOrDefault(culture => culture.Name == pathPart);
			if (culture != null)
				return culture;
		}

		return DefaultCulture;
	}
}