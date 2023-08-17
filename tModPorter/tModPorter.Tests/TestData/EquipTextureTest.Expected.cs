using System;
using Terraria;
using Terraria.ModLoader;

public class EquipTextureTest : EquipTexture
{
	void Method() {
#if COMPILE_ERROR
		Console.WriteLine(mod/* tModPorter Note: Removed. */);
#endif
		Console.WriteLine(Item);
	}

	public override void FrameEffects(Player player, EquipType type) { /* Empty */ }

#if COMPILE_ERROR
	public override bool DrawHead()/* tModPorter Note: Removed. After registering this as EquipType.Head, use ArmorIDs.Head.Sets.DrawHead[slot] = false if you returned false */ { return true; /* Empty */ }

	public override bool DrawBody()/* tModPorter Note: Removed. After registering this as EquipType.Body, use ArmorIDs.Body.Sets.HidesTopSkin[slot] = true if you returned false */ { return true; /* Empty */ }

	public override bool DrawLegs()/* tModPorter Note: Removed. After registering this as EquipType.Legs or Shoes, use ArmorIDs.Legs.Sets.HidesBottomSkin[slot] = true if you returned false for EquipType.Legs, and ArmorIDs.Shoe.Sets.OverridesLegs[slot] = true if you returned false for EquipType.Shoes */ { return true; /* Empty */ }

	public override void DrawHands(ref bool drawHands, ref bool drawArms)/* tModPorter Note: Removed. After registering this as EquipType.Body, use ArmorIDs.Body.Sets.HidesHands[slot] = false if you had drawHands set to true. If you had drawArms set to true, you don't need to do anything */ { /* Empty */ }

	public override void DrawHair(ref bool drawHair, ref bool drawAltHair)/* tModPorter Note: Removed. After registering this as EquipType.Head, use ArmorIDs.Head.Sets.DrawFullHair[slot] = true if you had drawHair set to true, and ArmorIDs.Head.Sets.DrawHatHair[slot] = true if you had drawAltHair set to true */ { /* Empty */ }
#endif
}

public class EquipTextureModTest : Mod
{
	void Method() {
		Mod mod = this;
		ModItem item = null;
		EquipType equipType = EquipType.Legs;
		string equipName = "EquipName";
		string pathToTexture = "PathToTexture";
		EquipTexture equipTexture = new EquipTexture();

		// 1.3 int Mod.AddEquipTexture(ModItem item, EquipType type, string name, string texture, string armTexture = "", string femaleTexture = "")
		// 1.3 int Mod.AddEquipTexture(EquipTexture equipTexture, ModItem item, EquipType type, string name, string texture, string armTexture = "", string femaleTexture = "")
		// 1.4 int EquipLoader.AddEquipTexture(Mod mod, string texture, EquipType type, ModItem item = null, string name = null, EquipTexture equipTexture = null)
		// ignore armTexture and femaleTexture, those are obsolete
		int slot = EquipLoader.AddEquipTexture(mod, pathToTexture, equipType, item, equipName);
		slot = EquipLoader.AddEquipTexture(mod, pathToTexture, equipType, name: equipName);

		slot = EquipLoader.AddEquipTexture(this, pathToTexture, equipType, item, equipName);
		slot = EquipLoader.AddEquipTexture(this, pathToTexture, equipType, name: equipName);

		slot = EquipLoader.AddEquipTexture(mod, pathToTexture, equipType, item, equipName, equipTexture);
		slot = EquipLoader.AddEquipTexture(mod, pathToTexture, equipType, name: equipName, equipTexture: equipTexture);

		slot = EquipLoader.AddEquipTexture(this, pathToTexture, equipType, item, equipName, equipTexture);
		slot = EquipLoader.AddEquipTexture(this, pathToTexture, equipType, name: equipName, equipTexture: equipTexture);

		slot = EquipLoader.AddEquipTexture(mod, pathToTexture, equipType, name: equipName)/* tModPorter Note: armTexture and femaleTexture now part of new spritesheet. https://github.com/tModLoader/tModLoader/wiki/Armor-Texture-Migration-Guide */;
		slot = EquipLoader.AddEquipTexture(this, pathToTexture, equipType, name: equipName, equipTexture: equipTexture)/* tModPorter Note: armTexture and femaleTexture now part of new spritesheet. https://github.com/tModLoader/tModLoader/wiki/Armor-Texture-Migration-Guide */;

		// 1.3 EquipTexture Mod.GetEquipTexture(string name, EquipType type)
		// 1.4 EquipTexture EquipLoader.GetEquipTexture(Mod mod, string name, EquipType type)
		equipTexture = EquipLoader.GetEquipTexture(mod, equipName, equipType);
		equipTexture = EquipLoader.GetEquipTexture(this, equipName, equipType);

		// 1.3 int Mod.GetEquipSlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type)
		slot = EquipLoader.GetEquipSlot(mod, equipName, equipType);
		slot = EquipLoader.GetEquipSlot(this, equipName, equipType);

		// 1.3 sbyte Mod.GetAccessorySlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type)
#if COMPILE_ERROR // sbyte -> int
		sbyte sSlot = EquipLoader.GetEquipSlot(mod, equipName, equipType);
		sSlot = EquipLoader.GetEquipSlot(this, equipName, equipType);
#endif
	}
}