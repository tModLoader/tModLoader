using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Default.Developer;
using Terraria.ModLoader.Default.Developer.Jofairden;
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


		public override void SetupAssetRepository(IList<IContentSource> sources, AssetReaderCollection assetReaderCollection, IList<Type> delayedLoadTypes)
		{
			sources.Clear();
			sources.Add(new AssemblyResourcesContentSource(Assembly.GetExecutingAssembly(), "Terraria.ModLoader.Default."));
		}

		public override void Load() {
			Instance = this;

			/*if (!Main.dedServ) {
				AddTexture("UnloadedItem", ReadTexture("UnloadedItem"));
				AddTexture("StartBag", ReadTexture("StartBag"));
				AddTexture("UnloadedTile", ReadTexture("UnloadedTile"));
			}*/
			
			AddContent<UnloadedItem>();
			AddContent<UnloadedGlobalItem>();
			AddContent<StartBag>();
			AddContent<AprilFools>();
			AddContent(new UnloadedTile());
			AddContent(new UnloadedTile("PendingUnloadedTile"));
			AddContent<UnloadedTileEntity>();
			AddContent<UnloadedPlayer>();
			AddContent<UnloadedWorld>();
			AddContent<UnloadedTilesWorld>();
			AddContent<HelpCommand>();
			AddContent<ModlistCommand>();
			/*AddPatronSets();
			AddPlayer("PatronModPlayer", new PatronModPlayer());
			AddDeveloperSets();
			AddPlayer("DeveloperPlayer", new DeveloperPlayer());*/
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
			};

			foreach (var patronItem in PatronSets.SelectMany(x => x)) {
				AddItemAndEquipType(patronItem, patronItem.ItemEquipType);
			}
		}

		private void AddDeveloperSets() {
			DeveloperSets = new[] {
				new DeveloperItem[] { new PowerRanger_Head(), new PowerRanger_Body(), new PowerRanger_Legs() }
			};

			foreach (var developerItem in DeveloperSets.SelectMany(x => x)) {
				AddItemAndEquipType(developerItem, developerItem.ItemEquipType);
			}
		}

		// Adds the given patreon item to ModLoader, and handles loading its assets automatically
		private void AddItemAndEquipType(ModItem item, EquipType equipType) {
			// If a client, we need to add several textures
			/*if (!Main.dedServ) {
				AddTexture($"{prefix}.{name}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}"));
				AddTexture($"{prefix}.{name}_{equipType}_{equipType}", ReadTexture($"{prefix}.{name}_{equipType}_{equipType}"));
				if (equipType == EquipType.Body) // If a body, add the arms texture
				{
					AddTexture($"{prefix}.{name}_{equipType}_Arms", ReadTexture($"{prefix}.{name}_{equipType}_Arms"));
				}
			}*/

			// Adds the item to ModLoader, as well as the normal assets
			AddContent(item);
			// AddEquipTexture adds the arms and female body assets automatically, if EquipType is Body
			AddEquipTexture(item, equipType, item.Texture + '_' + equipType);
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
					player.QuickSpawnItem(patreonItem.Type);
				}

				return true;
			}

			if (Main.rand.NextBool(ChanceToGetDevArmor)) {
				int randomIndex = Main.rand.Next(DeveloperSets.Length);

				foreach (var developerItem in DeveloperSets[randomIndex]) {
					player.QuickSpawnItem(developerItem.Type);
				}

				return true;
			}
			return false;
		}
	}
}
