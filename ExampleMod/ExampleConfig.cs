using Terraria.ModLoader;

namespace ExampleMod
{
	class ExampleConfig : ModConfig
	{
		[MyCustom(SomeProperty = "foo bar")]
		public int MyProperty { get; set; }

		[ValueRange(0, 10)]
		public int RangeExample { get; set; }

		public bool CheckboxExample { get; set; }

		[Label("Multiplier for this Mods NPC spawns")]
		[FloatValueRange(0, 10)]
		public float SpawnMultiplier { get; set; }

		[Label("Whether or not to show the GUI button")]
		[MultiplayerMode(Mode = MultiplayerSyncMode.UniquePerPlayer)]
		public bool ShowGUIButton { get; set; }

		[Label("Your favorite Nintendo Character is: ")]
		[OptionStrings(new string[] { "Zelda", "Peach" })]
		public string FavoriteNintendoCharacter { get; set; }

		//public int[] BannedVanillaItemIDs { get; set; }
	}
}
