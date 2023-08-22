using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader;

public static partial class NPCShopDatabase
{
	internal const bool TestingEnabled = false;

	private class ConditionTest
	{
		public IEnumerable<Condition> Conditions;
		public string Name;
		public bool Tested;

		public ConditionTest(IEnumerable<Condition> conditions)
		{
			Conditions = conditions.OrderBy(c => c.Description.Value).ToList();
			Name = string.Join(" && ", conditions.Select(c => c.Description));
		}

		public string Status => $"[{(Tested ? '#' : ' ')}] {Name}";
	}

	private static List<ConditionTest> tests = new();
	private static HashSet<string> mismatches = new();
	private static readonly string TestFilePath = "TestedShopConditions.txt";

	private static void InitShopTestSystem()
	{
		if (!TestingEnabled)
			return;

		tests.Clear();
		foreach (var shop in AllShops.OfType<NPCShop>()) {
			foreach (var entry in shop.Entries) {
				var test = new ConditionTest(entry.Conditions);
				if (!tests.Any(t => Enumerable.SequenceEqual(t.Conditions, test.Conditions)))
					tests.Add(test);
			}
		}

		tests = tests.OrderBy(t => t.Name).ToList();

		if (File.Exists(TestFilePath)) {
			foreach (var line in File.ReadAllLines(TestFilePath)) {
				var key = line[4..];
				bool tested = line[1] == '#';
				if (tested && tests.FirstOrDefault(t => t.Name == key) is { } test)
					test.Tested = true;
			}
		}
	}

	internal static void Test()
	{
		if (!TestingEnabled || (int)Main.time % 30 == 0)
			return;

		// Some known issues:
		//   Golf score exactly 500/1000/2000, some vanilla conditions use > and some use >=. tML can just use >=
		//   Player y zone height. Some vanilla conditions use player.position.Y, others use player.center.Y and others use ZoneSkyHeight etc
		//     minor differences between these are fine to ignore

		// Some helper codes
		// Main.moonPhase = (Main.moonPhase + 1) % 8;
		// Main.LocalPlayer.golferScoreAccumulated = 2001;

		var chest = new Chest();
		string ChestToString() => string.Join(" ", chest.item.Select(item => item.type));

		bool allPass = true;
		for (int i = 1; i < _vanillaShopNames.Length; i++) {
			chest.VanillaSetupShop(i);
			var vanillaString = ChestToString();

			chest.SetupShop(i);
			var tMLString = ChestToString();

			if (vanillaString != tMLString) {
				var header = $"Shop mismatch! {_vanillaShopNames[i]}";
				var msg = $"{header}\nVanilla: {vanillaString}\nTML:     {tMLString}";
				if (mismatches.Add(msg)) {
					Main.NewText(header, Color.Orange);
					Logging.tML.Warn(msg);
				}
				allPass = false;
			}
		}

		if (!allPass)
			return;

		bool newCondsTested = false;
		int testedCount = 0;
		foreach (var test in tests) {
			if (test.Tested) {
				testedCount++;
				continue;
			}

			if (test.Conditions.All(e => e.IsMet())) {
				test.Tested = true;
				testedCount++;
				newCondsTested = true;
			}
		}

		if (newCondsTested) {
			Main.NewText($"Shop Conditions Tested: {testedCount}/{tests.Count}", Microsoft.Xna.Framework.Color.Orange);
			File.WriteAllLines(TestFilePath, tests.Select(t => t.Status));
		}
	}
}