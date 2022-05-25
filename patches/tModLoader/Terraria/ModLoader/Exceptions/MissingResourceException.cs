using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.Exceptions
{
	public class MissingResourceException : Exception
	{
		public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#terrariamodloadermodgettexturestring-name-error";

		public MissingResourceException() {
		}

		public MissingResourceException(string message)
			: base(message) {
		}

		public MissingResourceException(string message, Exception inner)
			: base(message, inner) {
		}

		public MissingResourceException(string message, ICollection<string> keys) : this(ProcessMessage(message, keys)) {
		}

		public static string ProcessMessage(string message, ICollection<string> keys) {
			string closestMatch = LevenshteinDistance.FolderAwareEditDistance(message, keys.ToArray());
			if (closestMatch != null && closestMatch != "") {
				// TODO: UIMessageBox still doesn't display long sequences of colored text correct.
				(string a, string b) = LevenshteinDistance.ComputeColorTaggedString(message, closestMatch);
				return Language.GetTextValue("tModLoader.LoadErrorResourceNotFoundPathHint", message, closestMatch) + "\n" + a + "\n" + b + "\n";
			}
			return message;
		}
	}

	static class LevenshteinDistance
	{
		internal static string FolderAwareEditDistance(string source, string[] targets) {
			if (targets.Length == 0) return null;
			var separator = '/';
			var sourceParts = source.Split(separator);
			var sourceFolders = sourceParts.Reverse().Skip(1).ToList();
			var sourceFile = sourceParts.Last();

			int missingFolderPenalty = 4;
			int extraFolderPenalty = 3;

			var scores = targets.Select(target => {
				var targetParts = target.Split(separator);
				var targetFolders = targetParts.Reverse().Skip(1).ToList();
				var targetFile = targetParts.Last();

				var commonFolders = sourceFolders.Where(x => targetFolders.Contains(x));
				var reducedSourceFolders = sourceFolders.Except(commonFolders).ToList();
				var reducedTargetFolders = targetFolders.Except(commonFolders).ToList();

				int score = 0;
				int folderDiff = reducedSourceFolders.Count - reducedTargetFolders.Count;
				if (folderDiff > 0)
					score += folderDiff * missingFolderPenalty;
				else if (folderDiff < 0)
					score += -folderDiff * extraFolderPenalty;

				if (reducedSourceFolders.Count > 0 && reducedSourceFolders.Count >= reducedTargetFolders.Count) {
					foreach (var item in reducedTargetFolders) {
						int min = Int32.MaxValue;
						foreach (var item2 in reducedSourceFolders) {
							min = Math.Min(min, LevenshteinDistance.Compute(item, item2));
						}
						score += min;
					}
				}
				else if (reducedSourceFolders.Count > 0) {
					foreach (var item in reducedSourceFolders) {
						int min = Int32.MaxValue;
						foreach (var item2 in reducedTargetFolders) {
							min = Math.Min(min, LevenshteinDistance.Compute(item, item2));
						}
						score += min;
					}
				}
				score += LevenshteinDistance.Compute(targetFile, sourceFile);

				return new {
					Target = target,
					Score = score
				};
			});
			var b = scores.OrderBy(x => x.Score);
			return scores.OrderBy(x => x.Score).First().Target;
		}

		public static int Compute(string s, string t) {
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0) {
				return m;
			}

			if (m == 0) {
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) {
			}

			for (int j = 0; j <= m; d[0, j] = j++) {
			}

			// Step 3
			for (int i = 1; i <= n; i++) {
				//Step 4
				for (int j = 1; j <= m; j++) {
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 2; // substitution

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 2, d[i, j - 1] + 2),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}

		public static (string, string) ComputeColorTaggedString(string s, string t) {
			//s = "HYUENDAI";
			//t = "HYUANDAI";

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];
			string[,] resultsA = new string[n + 1, m + 1];
			string[,] resultsB = new string[n + 1, m + 1];

			// Step 1
			if (n == 0) {
				return ("", "");
			}

			if (m == 0) {
				return ("", "");
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++) {
				if (i < n) {
					resultsA[i + 1, 0] = "" + s[i];
					resultsB[i + 1, 0] = "" + s[i];
				}
			}

			for (int j = 0; j <= m; d[0, j] = j++) {
				if (j < m) {
					resultsA[0, j + 1] = "" + t[j];
					resultsB[0, j + 1] = "" + t[j];
				}
			}

			// Step 3
			for (int i = 1; i <= n; i++) {
				//Step 4
				for (int j = 1; j <= m; j++) {
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 2; // substitution

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 2, d[i, j - 1] + 2),
						d[i - 1, j - 1] + cost);

					if (d[i, j] == d[i - 1, j - 1] + cost) {
						if (cost == 0) {
							resultsA[i, j] = $"{resultsA[i - 1, j - 1]}{s[i - 1]}";
							resultsB[i, j] = $"{resultsB[i - 1, j - 1]}{t[j - 1]}";
						}
						else {
							resultsA[i, j] = $"{resultsA[i - 1, j - 1]}[c/ffff00:{s[i - 1]}]"; //
							resultsB[i, j] = $"{resultsB[i - 1, j - 1]}[c/ffff00:{t[j - 1]}]"; // 
						}
					}
					else if (d[i, j] == d[i - 1, j] + 2) // deletion
					{
						resultsA[i, j] = $"{resultsA[i - 1, j]} ";
						resultsB[i, j] = $"{resultsB[i - 1, j]}[c/ff0000:{s[i - 1]}]";

						//resultsB[i, j] = $"{resultsA[i - 1, j]}{s[i - 1]}";
					}
					else if (d[i, j] == d[i, j - 1] + 2) // insertion
					{
						resultsA[i, j] = $"{resultsA[i, j - 1]}[c/00ff00:{t[j - 1]}]";
						resultsB[i, j] = $"{resultsB[i, j - 1]} ";

						//resultsA[i, j] = $"{resultsA[i, j - 1]}{t[i]}";
					}
					else {
						Console.WriteLine();
					}
				}
			}
			// Step 7
			return (resultsA[n, m], resultsB[n, m]);
		}
	}
}
