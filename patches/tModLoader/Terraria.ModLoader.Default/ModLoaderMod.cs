using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Default.Developer;
using Terraria.ModLoader.Default.Patreon;

namespace Terraria.ModLoader.Default
{
	internal class ModLoaderMod : Mod
	{
		internal static ModLoaderMod Instance;

		// If new types arise (probably not), change the format:
		// head, body, legs, wings, <new>
		private static PatreonItem[][] PatronSets;
		private static DeveloperItem[][] DeveloperSets;
		private const int ChanceToGetPatreonArmor = 20;
		private const int ChanceToGetDevArmor = 30;

		public override string Name => "ModLoader";
		public override Version Version => ModLoader.version;

		internal ModLoaderMod() {
			Side = ModSide.NoSync;
			DisplayName = "tModLoader";
		}

		public override void Load() {
			Instance = this;
			if (!Main.dedServ) {
				AddTexture("MysteryItem", ReadTexture("MysteryItem"));
				AddTexture("StartBag", ReadTexture("StartBag"));
				AddTexture("MysteryTile", ReadTexture("MysteryTile"));
			}
			AddItem("MysteryItem", new MysteryItem());
			AddGlobalItem("MysteryGlobalItem", new MysteryGlobalItem());
			AddItem("StartBag", new StartBag());
			AddItem("AprilFools", new AprilFools());
			AddTile("MysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTile("PendingMysteryTile", new MysteryTile(), "ModLoader/MysteryTile");
			AddTileEntity("MysteryTileEntity", new MysteryTileEntity());
			AddPlayer("MysteryPlayer", new MysteryPlayer());
			AddModWorld("MysteryWorld", new MysteryWorld());
			AddModWorld("MysteryTilesWorld", new MysteryTilesWorld());
			AddCommand("HelpCommand", new HelpCommand());
			AddCommand("ModlistCommand", new ModlistCommand());
			AddPatronSets();
			AddPlayer("PatronModPlayer", new PatronModPlayer());
			AddDeveloperSets();
			AddPlayer("DeveloperPlayer", new DeveloperPlayer());
		}

		public override void Unload() {
			PatronSets = null;
			DeveloperSets = null;
		}

		private void AddPatronSets() {
			PatronSets = new[] {
				new PatreonItem[] { new toplayz_Head(), new toplayz_Body(), new toplayz_Legs() },
				new PatreonItem[] { new KittyKitCatCat_Head(), new KittyKitCatCat_Body(), new KittyKitCatCat_Legs() },
				new PatreonItem[] { new Polyblank_Head(), new Polyblank_Body(), new Polyblank_Legs() },
				new PatreonItem[] { new dinidini_Head(), new dinidini_Body(), new dinidini_Legs(), new dinidini_Wings() },
				new PatreonItem[] { new Remeus_Head(), new Remeus_Body(), new Remeus_Legs() },
				new PatreonItem[] { new Saethar_Head(), new Saethar_Body(), new Saethar_Legs(), new Saethar_Wings() },
				new PatreonItem[] { new Orian_Head(), new Orian_Body(), new Orian_Legs() },
				new PatreonItem[] { new Glory_Head(), new Glory_Body(), new Glory_Legs() },
				new PatreonItem[] { new POCKETS_Head(), new POCKETS_Body(), new POCKETS_Legs(), new POCKETS_Wings() },
				new PatreonItem[] { new Guildpack_Head(), new Guildpack_Body(), new Guildpack_Legs() },
				new PatreonItem[] { new Elfinlocks_Head(), new Elfinlocks_Body(), new Elfinlocks_Legs() },
				new PatreonItem[] { new AetherBreaker_Head(), new AetherBreaker_Body(), new AetherBreaker_Legs(), new AetherBreaker_Wings() , new WitchDaggah_Head() },
				new PatreonItem[] { new Sailing_Squid_Head(), new Sailing_Squid_Body(), new Sailing_Squid_Legs(), new Sailing_Squid_Wings() },
				new PatreonItem[] { new Coolmike5000_Head(), new Coolmike5000_Body(), new Coolmike5000_Legs(), new Coolmike5000_Wings() },
				new PatreonItem[] { new Zeph_Head(), new Zeph_Body(), new Zeph_Legs(), new Zeph_Wings() },
				new PatreonItem[] { new dschosen_Head(), new dschosen_Body(), new dschosen_Legs(), new dschosen_Wings() },
				new PatreonItem[] { new AlejandroAkbal_Head(), new AlejandroAkbal_Body(), new AlejandroAkbal_Legs(), new AlejandroAkbal_Back() },
				new PatreonItem[] { new Tantamount_Head(), new Tantamount_Body(), new Tantamount_Legs(), new Tantamount_Wings() },
			};

			foreach (var patronItem in PatronSets.SelectMany(x => x)) {
				AddItemAndEquipType(patronItem, "Patreon", patronItem.SetName, patronItem.ItemEquipType);
			}
			if (!Main.dedServ) 
			{
				AddTexture($"Patreon.Guildpack_Aura", ReadTexture($"Patreon.Guildpack_Aura"));
				AddTexture($"Patreon.Guildpack_Head_Glow", ReadTexture($"Patreon.Guildpack_Head_Glow"));
				AddTexture($"Patreon.Saethar_Head_Glow", ReadTexture($"Patreon.Saethar_Head_Glow"));
			}
		}

		private void AddDeveloperSets() {
			DeveloperSets = new[] {
				new DeveloperItem[] { new PowerRanger_Head(), new PowerRanger_Body(), new PowerRanger_Legs() }
			};

			foreach (var developerItem in DeveloperSets.SelectMany(x => x)) {
				AddItemAndEquipType(developerItem, "Developer", developerItem.SetName, developerItem.ItemEquipType);
			}
		}

		// Adds the given patreon item to ModLoader, and handles loading its assets automatically
		private void AddItemAndEquipType(ModItem item, string prefix, string name, EquipType equipType) {
			// If a client, we need to add several textures
			if (!Main.dedServ) {
				AddTexture($"{prefix}.{name}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}"));
				AddTexture($"{prefix}.{name}_{equipType}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}_{equipType}"));
				if (equipType == EquipType.Body) // If a body, add the arms texture
				{
					AddTexture($"{prefix}.{name}_{equipType}_Arms", ReadTexture($"{prefix}.{name}_{equipType}_Arms"));
				}
			}

			// Adds the item to ModLoader, as well as the normal assets
			AddItem($"{name}_{equipType}", item);
			// AddEquipTexture adds the arms and female body assets automatically, if EquipType is Body
			AddEquipTexture(item, equipType, item.Name, item.Texture + '_' + equipType, item.Texture + "_Arms", item.Texture + "_FemaleBody");
		}

		internal static Texture2D ReadTexture(string file) {
			Assembly assembly = Assembly.GetExecutingAssembly();
			// if someone set the type or name wrong, the stream will be null.
			Stream stream = assembly.GetManifestResourceStream("Terraria.ModLoader.Default." + file + ".png");

			// [sanity check, makes it easier to know what's wrong]
			if (stream == null) {
				throw new ArgumentException("Given EquipType for PatreonItem or name is not valid. It is possible either does not match up with the classname. If you added a new EquipType, modify GetEquipTypeSuffix() and AddPatreonItemAndEquipType() first.");
			}

			return Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
		}

		internal static bool TryGettingPatreonOrDevArmor(Player player) {
			if (Main.rand.NextBool(ChanceToGetPatreonArmor)) {
				int randomIndex = Main.rand.Next(PatronSets.Length);

				foreach (var patreonItem in PatronSets[randomIndex]) {
					player.QuickSpawnItem(patreonItem.item.type);
				}

				return true;
			}

			if (Main.rand.NextBool(ChanceToGetDevArmor)) {
				int randomIndex = Main.rand.Next(DeveloperSets.Length);

				foreach (var developerItem in DeveloperSets[randomIndex]) {
					player.QuickSpawnItem(developerItem.item.type);
				}

				return true;
			}
			return false;
		}
	}
}
