using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.Exceptions;

public class MissingResourceException : Exception
{
	public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#terrariamodloadermodgettexturestring-name-error";

	public MissingResourceException()
	{
	}

	public MissingResourceException(string message)
		: base(message)
	{
	}

	public MissingResourceException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public MissingResourceException(List<string> reasons, string assetPath, ICollection<string> keys) : this(ProcessMessage(reasons, assetPath, keys))
	{
	}

	public static string ProcessMessage(List<string> reasons, string assetPath, ICollection<string> keys)
	{
		if(reasons.Count > 0) {
			reasons.Insert(0, $"Failed to load asset: \"{assetPath}\"");
			if (reasons.Any(x => x.Contains("Texture2D creation failed! Error Code: The parameter is incorrect."))) {
				reasons.Insert(1, "The most common reason for this \"Texture2D creation failed!\" error is a malformed .png file. Make sure you are saving textures in the .png format and are not just renaming the file extension of your texture files to .png, that does not work.");
			}
			return string.Join(Environment.NewLine, reasons);
		}

		string closestMatch = LevenshteinDistance.FolderAwareEditDistance(assetPath, keys.ToArray());
		if (closestMatch != null && closestMatch != "") {
			// TODO: UIMessageBox still doesn't display long sequences of colored text correct.
			(string a, string b) = LevenshteinDistance.ComputeColorTaggedString(assetPath, closestMatch);
			string message = Language.GetTextValue("tModLoader.LoadErrorResourceNotFoundPathHint", assetPath, closestMatch) + "\n" + a + "\n" + b + "\n";
			if (new System.Diagnostics.StackTrace().ToString().Contains("Terraria.ModLoader.EquipLoader.AddEquipTexture")) {
				// Errors from Extra textures sometimes mislead modders, need to inform them.
				message += $"\n{Language.GetTextValue("tModLoader.LoadErrorResourceNotFoundEquipTextureHint")}\n";
			}
			return message;
		}
		return assetPath;
	}
}

static class LevenshteinDistance
{
	enum Edits
	{
		Keep, Delete, Insert, Substitute, Blank
	}

	internal static string FolderAwareEditDistance(string source, string[] targets)
	{
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

	public static int Compute(string s, string t)
	{
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

	public static (string, string) ComputeColorTaggedString(string s, string t)
	{
		//s = "HYUENDAI";
		//t = "HYUANDAI";

		int n = s.Length;
		int m = t.Length;
		int[,] d = new int[n + 1, m + 1];

		// Step 1
		if (n == 0) {
			return ("", "");
		}

		if (m == 0) {
			return ("", "");
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
				int cost = (t[j - 1] == s[i - 1]) ? 0 : 1; // substitution

				// Step 6
				d[i, j] = Math.Min(
					Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
					d[i - 1, j - 1] + cost);
			}
		}
		// Step 7

		// Backtracking approach adapted from https://stackoverflow.com/a/50864452
		var x = n;
		var y = m;

		var editsFromStoT = new Stack<(Edits, char)>();
		var editsFromTtoS = new Stack<(Edits, char)>();

		while (x != 0 || y != 0) {
			var cost = d[x, y];
			// edge cases
			if (y - 1 < 0) {
				editsFromStoT.Push((Edits.Delete, s[x - 1]));
				editsFromTtoS.Push((Edits.Blank, ' '));
				x--;
				continue;
			}

			if (x - 1 < 0) {
				editsFromStoT.Push((Edits.Insert, t[y - 1]));
				editsFromTtoS.Push((Edits.Blank, ' '));
				y--;
				continue;
			}

			// Middle cases
			var costLeft = d[x, y - 1];
			var costUp = d[x - 1, y];
			var costDiagonal = d[x - 1, y - 1];

			if (costDiagonal <= costLeft && costDiagonal <= costUp && (costDiagonal == cost - 1 || costDiagonal == cost)) {
				if (costDiagonal == cost - 1) {
					editsFromStoT.Push((Edits.Substitute, s[x - 1]));
					editsFromTtoS.Push((Edits.Substitute, t[y - 1]));
					x--;
					y--;
				}
				else {
					editsFromStoT.Push((Edits.Keep, s[x - 1]));
					editsFromTtoS.Push((Edits.Keep, t[y - 1]));
					x--;
					y--;
				}
			}
			else if (costLeft <= costDiagonal && costLeft == cost - 1) {
				editsFromStoT.Push((Edits.Insert, t[y - 1]));
				editsFromTtoS.Push((Edits.Blank, ' '));
				y--;
			}
			else {
				editsFromStoT.Push((Edits.Delete, s[x - 1]));
				editsFromTtoS.Push((Edits.Blank, ' '));
				x--;
			}
		}

		string resultA = FinalizeText(editsFromStoT);
		string resultB = FinalizeText(editsFromTtoS);

		string FinalizeText(Stack<(Edits, char)> results)
		{
			string result = "";
			// Track last edit for efficient color tag usage.
			Edits editCurrent = Edits.Keep;
			while (results.Count > 0) {
				var entry = results.Pop();
				//for (int i = 0; i < results[n, m].Item1.Length; i++) {
				Edits nextEdit = entry.Item1;
				if (editCurrent != nextEdit) {
					if (editCurrent != Edits.Keep && editCurrent != Edits.Blank)
						result += "]";
					if (nextEdit == Edits.Delete) {
						result += "[c/ff0000:";
					}
					else if (nextEdit == Edits.Insert) {
						result += "[c/00ff00:";
					}
					else if (nextEdit == Edits.Substitute) {
						result += "[c/ffff00:";
					}
				}
				result += entry.Item2;
				editCurrent = nextEdit;
			}
			if (editCurrent != Edits.Keep && editCurrent != Edits.Blank)
				result += "]";
			return result;
		}

		return (resultA, resultB);
	}
}
