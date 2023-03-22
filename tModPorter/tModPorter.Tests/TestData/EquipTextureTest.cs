using System;
using Terraria;
using Terraria.ModLoader;

public class EquipTextureTest : EquipTexture
{
	void Method() {
		Console.WriteLine(mod);
		Console.WriteLine(item);
	}

	public override void UpdateVanity(Player player, EquipType type) { /* Empty */ }

	public override bool DrawHead() { return true; /* Empty */ }

	public override bool DrawBody() { return true; /* Empty */ }

	public override bool DrawLegs() { return true; /* Empty */ }

	public override void DrawHands(ref bool drawHands, ref bool drawArms) { /* Empty */ }

	public override void DrawHair(ref bool drawHair, ref bool drawAltHair) { /* Empty */ }
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
		int slot = mod.AddEquipTexture(item, equipType, equipName, pathToTexture);
		slot = mod.AddEquipTexture(null, equipType, equipName, pathToTexture);

		slot = AddEquipTexture(item, equipType, equipName, pathToTexture);
		slot = AddEquipTexture(null, equipType, equipName, pathToTexture);

		slot = mod.AddEquipTexture(equipTexture, item, equipType, equipName, pathToTexture);
		slot = mod.AddEquipTexture(equipTexture, null, equipType, equipName, pathToTexture);

		slot = AddEquipTexture(equipTexture, item, equipType, equipName, pathToTexture);
		slot = AddEquipTexture(equipTexture, null, equipType, equipName, pathToTexture);

		slot = mod.AddEquipTexture(null, equipType, equipName, pathToTexture, "arms", "female");
		slot = AddEquipTexture(equipTexture, null, equipType, equipName, pathToTexture, femaleTexture: "female");

		// 1.3 EquipTexture Mod.GetEquipTexture(string name, EquipType type)
		// 1.4 EquipTexture EquipLoader.GetEquipTexture(Mod mod, string name, EquipType type)
		equipTexture = mod.GetEquipTexture(equipName, equipType);
		equipTexture = GetEquipTexture(equipName, equipType);

		// 1.3 int Mod.GetEquipSlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type)
		slot = mod.GetEquipSlot(equipName, equipType);
		slot = GetEquipSlot(equipName, equipType);

		// 1.3 sbyte Mod.GetAccessorySlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type)
		sbyte sSlot = mod.GetAccessorySlot(equipName, equipType);
		sSlot = GetAccessorySlot(equipName, equipType);
	}
}