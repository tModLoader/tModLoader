using System;
using Terraria;
using Terraria.ModLoader;

public class EquipTextureTest : EquipTexture
{
	void Method() {
#if COMPILE_ERROR
		Console.WriteLine(mod/* Suggestion: Removed */);
#endif
		Console.WriteLine(Item);
	}
	public override void FrameEffects(Player player, EquipType type) { /* Empty */ }
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

		// 1.3 EquipTexture Mod.GetEquipTexture(string name, EquipType type)
		// 1.4 EquipTexture EquipLoader.GetEquipTexture(Mod mod, string name, EquipType type)
		equipTexture = EquipLoader.GetEquipTexture(mod, equipName, equipType);
		equipTexture = EquipLoader.GetEquipTexture(this, equipName, equipType);

		// 1.3 int Mod.GetEquipSlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type)
		slot = EquipLoader.GetEquipSlot(mod, equipName, equipType);
		slot = EquipLoader.GetEquipSlot(this, equipName, equipType);

		// 1.3 sbyte Mod.GetAccessorySlot(string name, EquipType type)
		// 1.4 int EquipLoader.GetEquipSlot(Mod mod, string name, EquipType type) cast to sbyte
		sbyte sSlot = (sbyte)EquipLoader.GetEquipSlot(mod, equipName, equipType);
		sSlot = (sbyte)EquipLoader.GetEquipSlot(this, equipName, equipType);
	}
}
