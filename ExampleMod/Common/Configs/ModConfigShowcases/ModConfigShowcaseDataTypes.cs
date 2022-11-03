using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using ExampleMod.Common.Configs.CustomDataTypes;

// This file contains fake ModConfig class that showcase creating config section
// by using fields with various data types.

// Because this config was designed to show off various UI capabilities,
// this config have no effect on the mod and provides purely teaching example.
namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	[BackgroundColor(144, 252, 249)]
	[Label("ModConfig Showcase A: Data Types")]
	public class ModConfigShowcaseDataTypes : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Value Types
		public bool SomeBool;
		public int SomeInt;
		public float SomeFloat;
		public string SomeString;
		public EquipType SomeEnum;
		public byte SomeByte;
		public uint SomeUInt;

		// Structs - These require special code. We've implemented Color and Vector2 so far.
		public Color SomeColor;
		public Vector2 SomeVector2;
		public Point SomePoint; // notice the not implemented message.

		// Data Structures (Reference Types)
		public int[] SomeArray = new int[] { 25, 70, 12 }; // Arrays have a specific length and need a default value specified.
		public List<int> SomeList = new List<int>() { 1, 3, 5 }; // Initializers can be used to declare defaults for data structures.
		public Dictionary<string, int> SomeDictionary = new Dictionary<string, int>();
		public HashSet<string> SomeSet = new HashSet<string>();

		// Classes (Reference Types) - Classes are automatically implemented in the UI.
		public SimpleData SomeClassA;
		// EntityDefinition classes store the identity of an Entity (Item, NPC, Projectile, etc) added by a mod or vanilla. Only the identity is preserved, not other mod data or stack.
		// When using XDefinition classes, you can the .Type property to get the ID of the item. You can use .IsUnloaded to check if the item in question is loaded.
		public ItemDefinition itemDefinitionExample;
		public NPCDefinition npcDefinitionExample = new NPCDefinition(NPCID.Bunny);
		public ProjectileDefinition projectileDefinitionExample = new ProjectileDefinition("ExampleMod", nameof(Content.Projectiles.ExampleHomingProjectile));

		// Data Structures of reference types
		/* TODO: Add this back in once ExampleMod adds a ModPrefix example.
		public Dictionary<PrefixDefinition, float> SomeClassE = new Dictionary<PrefixDefinition, float>() {
			[new PrefixDefinition("ExampleMod", "Awesome")] = 0.5f,
			[new PrefixDefinition("ExampleMod", "ReallyAwesome")] = 0.8f
		};
		*/

		// TODO: Not working at the moment.
		// Using a custom class as a key in a Dictionary. When used as a Dictionary Key, special code must be used.
		public Dictionary<ClassUsedAsKey, Color> CustomKey = new Dictionary<ClassUsedAsKey, Color>();

		public ModConfigShowcaseDataTypes() {
			// Doing the initialization of defaults for reference types in a constructor is also acceptable.
			SomeClassA = new SimpleData() {
				percent = .85f
			};

			CustomKey.Add(new ClassUsedAsKey() {
				SomeBool = true,
				SomeNumber = 42
			},
			new Color(1, 2, 3, 4));

			itemDefinitionExample = new ItemDefinition("Terraria/GoldOre"); // EntityDefinition uses ItemID field names rather than the numbers themselves for readability.
		}
	}
}
