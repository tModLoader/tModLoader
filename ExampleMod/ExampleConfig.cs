using System;
using System.ComponentModel;
using Terraria.ModLoader;

namespace ExampleMod
{
	class ExampleConfig : ModConfig
	{

		[MyCustom(SomeProperty = "foo bar")]
		[DefaultValue(5)]    
		public int FieldExample = 11;

		public bool FieldBoolExample = true;

		private bool PrivateFieldBoolExample = false;

		private bool InternalFieldBoolExample = false;

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

		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		//public int[] BannedVanillaItemIDs { get; set; }
	}

	class ExampleConfig2 : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		public bool CheckboxExample { get; set; }

		public override bool NeedsReload(ModConfig old)
		{
			return CheckboxExample != (old as ExampleConfig2).CheckboxExample;
		}
	}

	class ExampleConfig3 : ModConfig
	{
		public override MultiplayerSyncMode Mode
		{
			get
			{
				return MultiplayerSyncMode.UniquePerPlayer;
			}
		}

		public bool CheckboxExample { get; set; }
	}
}
