using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Terraria;
using Terraria.ID;

namespace SyntaxRewriterTestProgram
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("hi");
			Item item = new Item();
			item.type = 5;
			item.createTile = 5;
			item.shoot = 3 /*test trivia*/;
			item.SetDefaults(2, false);
			item.SetDefaults(3);
			Projectile.NewProjectile(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
			Item[] items = new Item[1];
			items[0] = new Item();
			items[0].type = 2;

			if (item.type != 4 && item.type >= 4 || item.type == 4) {
			}
			if (item.createTile == 4) {
			}

			switch (item.createTile) {
				case 1:
				case 3:
				case 2:
				default:
					break;
			}

			int projectileToSpawn = item.shoot;
			switch (projectileToSpawn) {
				case 1:
				case 3:
				case 2:
				default:
					break;
			}

#if DEBUG
			Console.WriteLine("DEBUG True.");
#else
			Console.WriteLine("DEBUG False.");
#endif
			Console.WriteLine("Done.");
			Console.ReadLine();
		}
	}
}

namespace Terraria
{
	class Item
	{
		public int type;
		public int createTile;
		public int shoot;
		public void SetDefaults(int type, bool something) { }
		public void SetDefaults(int type) { }

		public void Test() {
			this.type = 3;
			type = 2;
			if (type != 2) {
			}
			switch (createTile) {
				case 1:
					break;
			}

		}
		public void netDefaults(int type) {
			if (type < 0) {
				switch (type) {
					case -1:
						SetDefaults(1);
						break;
				}
			}
		}
	}

	class Projectile
	{
		public static int NewProjectile(float x, float y, float sx, float sy, int Type, int damage, float knockback, int owner, float ai0, float ai1) {
			return 0;
		}
	}
}

namespace Terraria.ID
{
	public class ItemID
	{
		public const short None = 0;
		public const short IronPickaxe = 1;
		public const short DirtBlock = 2;
		public const short StoneBlock = 3;
		public const short IronBroadsword = 4;
		public const short Mushroom = 5;
	}

	public class TileID
	{
		public const ushort Dirt = 0;
		public const ushort Stone = 1;
		public const ushort Grass = 2;
		public const ushort Plants = 3;
		public const ushort Torches = 4;
		public const ushort Trees = 5;
	}
	public class ProjectileID
	{
		public const short None = 0;
		public const short WoodenArrowFriendly = 1;
		public const short FireArrow = 2;
		public const short Shuriken = 3;
		public const short UnholyArrow = 4;
		public const short JestersArrow = 5;
		public const short EnchantedBoomerang = 6;
		public const short VilethornBase = 7;
	}
}
