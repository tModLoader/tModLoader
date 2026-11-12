using System;

namespace Terraria.ID;

/// <summary>
/// Identifies a chest using its tile type and style. Vanilla chest IDs cover all styles in
/// <see cref="TileID.Containers"/> and <see cref="TileID.Containers2"/>.
/// </summary>
public static class ChestID
{
	/// <summary>Returned by <see cref="Chest.type"/> when the chest is not attached to a valid chest tile.</summary>
	public const int None = 0;

	/// <summary>Gets a chest ID for a tile type and style, including modded chest tiles.</summary>
	public static int FromTile(ushort tileType, int style)
	{
		if (style < 0 || style > 0xFFFF)
			throw new ArgumentOutOfRangeException(nameof(style));

		return (tileType << 16) | style;
	}

	public const int Chest = (TileID.Containers << 16) | 0;
	public const int Gold = (TileID.Containers << 16) | 1;
	public const int LockedGold = (TileID.Containers << 16) | 2;
	public const int Shadow = (TileID.Containers << 16) | 3;
	public const int LockedShadow = (TileID.Containers << 16) | 4;
	public const int Barrel = (TileID.Containers << 16) | 5;
	public const int TrashCan = (TileID.Containers << 16) | 6;
	public const int Ebonwood = (TileID.Containers << 16) | 7;
	public const int RichMahogany = (TileID.Containers << 16) | 8;
	public const int Pearlwood = (TileID.Containers << 16) | 9;
	public const int Ivy = (TileID.Containers << 16) | 10;
	public const int Frozen = (TileID.Containers << 16) | 11;
	public const int LivingWood = (TileID.Containers << 16) | 12;
	public const int Skyware = (TileID.Containers << 16) | 13;
	public const int Shadewood = (TileID.Containers << 16) | 14;
	public const int WebCovered = (TileID.Containers << 16) | 15;
	public const int Lihzahrd = (TileID.Containers << 16) | 16;
	public const int Water = (TileID.Containers << 16) | 17;
	public const int Jungle = (TileID.Containers << 16) | 18;
	public const int Corruption = (TileID.Containers << 16) | 19;
	public const int Crimson = (TileID.Containers << 16) | 20;
	public const int Hallowed = (TileID.Containers << 16) | 21;
	public const int Ice = (TileID.Containers << 16) | 22;
	public const int LockedJungle = (TileID.Containers << 16) | 23;
	public const int LockedCorruption = (TileID.Containers << 16) | 24;
	public const int LockedCrimson = (TileID.Containers << 16) | 25;
	public const int LockedHallowed = (TileID.Containers << 16) | 26;
	public const int LockedIce = (TileID.Containers << 16) | 27;
	public const int Dynasty = (TileID.Containers << 16) | 28;
	public const int Honey = (TileID.Containers << 16) | 29;
	public const int Steampunk = (TileID.Containers << 16) | 30;
	public const int PalmWood = (TileID.Containers << 16) | 31;
	public const int Mushroom = (TileID.Containers << 16) | 32;
	public const int BorealWood = (TileID.Containers << 16) | 33;
	public const int Slime = (TileID.Containers << 16) | 34;
	public const int GreenDungeon = (TileID.Containers << 16) | 35;
	public const int LockedGreenDungeon = (TileID.Containers << 16) | 36;
	public const int PinkDungeon = (TileID.Containers << 16) | 37;
	public const int LockedPinkDungeon = (TileID.Containers << 16) | 38;
	public const int BlueDungeon = (TileID.Containers << 16) | 39;
	public const int LockedBlueDungeon = (TileID.Containers << 16) | 40;
	public const int Bone = (TileID.Containers << 16) | 41;
	public const int Cactus = (TileID.Containers << 16) | 42;
	public const int Flesh = (TileID.Containers << 16) | 43;
	public const int Obsidian = (TileID.Containers << 16) | 44;
	public const int Pumpkin = (TileID.Containers << 16) | 45;
	public const int Spooky = (TileID.Containers << 16) | 46;
	public const int Glass = (TileID.Containers << 16) | 47;
	public const int Martian = (TileID.Containers << 16) | 48;
	public const int Meteorite = (TileID.Containers << 16) | 49;
	public const int Granite = (TileID.Containers << 16) | 50;
	public const int Marble = (TileID.Containers << 16) | 51;

	public const int Crystal = (TileID.Containers2 << 16) | 0;
	public const int Golden = (TileID.Containers2 << 16) | 1;
	public const int Spider = (TileID.Containers2 << 16) | 2;
	public const int Lesion = (TileID.Containers2 << 16) | 3;
	public const int GoldChest = (TileID.Containers2 << 16) | 4;
	public const int Solar = (TileID.Containers2 << 16) | 5;
	public const int Vortex = (TileID.Containers2 << 16) | 6;
	public const int Nebula = (TileID.Containers2 << 16) | 7;
	public const int Stardust = (TileID.Containers2 << 16) | 8;
	public const int Golf = (TileID.Containers2 << 16) | 9;
	public const int Desert = (TileID.Containers2 << 16) | 10;
	public const int Bamboo = (TileID.Containers2 << 16) | 11;
	public const int DungeonDesert = (TileID.Containers2 << 16) | 12;
	public const int LockedDungeonDesert = (TileID.Containers2 << 16) | 13;
	public const int Coral = (TileID.Containers2 << 16) | 14;
	public const int Balloon = (TileID.Containers2 << 16) | 15;
	public const int AshWood = (TileID.Containers2 << 16) | 16;
	public const int Aetherium = (TileID.Containers2 << 16) | 17;
	public const int FallenStar = (TileID.Containers2 << 16) | 18;
	public const int Feywood = (TileID.Containers2 << 16) | 19;
	public const int Hallowed2 = (TileID.Containers2 << 16) | 20;
	public const int Gothic = (TileID.Containers2 << 16) | 21;
	public const int Demonite = (TileID.Containers2 << 16) | 22;
	public const int Crimtane = (TileID.Containers2 << 16) | 23;
	public const int Snow = (TileID.Containers2 << 16) | 24;
	public const int FlinxFur = (TileID.Containers2 << 16) | 25;
	public const int Pine = (TileID.Containers2 << 16) | 26;
	public const int Easter = (TileID.Containers2 << 16) | 27;
	public const int Stone = (TileID.Containers2 << 16) | 28;
	public const int Jellyfish = (TileID.Containers2 << 16) | 29;
	public const int Harpy = (TileID.Containers2 << 16) | 30;
	public const int Cloud = (TileID.Containers2 << 16) | 31;
	public const int Moonplate = (TileID.Containers2 << 16) | 32;
	public const int Librarian = (TileID.Containers2 << 16) | 33;
	public const int Spike = (TileID.Containers2 << 16) | 34;
	public const int Office = (TileID.Containers2 << 16) | 35;
	public const int Forbidden = (TileID.Containers2 << 16) | 36;
	public const int Boulder = (TileID.Containers2 << 16) | 37;
}
