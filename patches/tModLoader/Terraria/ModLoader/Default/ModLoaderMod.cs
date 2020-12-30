using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Default.Developer;
using Terraria.ModLoader.Default.Patreon;

namespace Terraria.ModLoader.Default
{
	internal class ModLoaderMod : Mod
	{
		private static PatreonItem[][] PatronSets;
		private static DeveloperItem[][] DeveloperSets;
		private const int ChanceToGetPatreonArmor = 20;
		private const int ChanceToGetDevArmor = 30;

		public override string Name => "ModLoader";
		public override Version Version => ModLoader.version;

		internal ModLoaderMod() {
			Side = ModSide.NoSync;
			DisplayName = "tModLoader";
			Code = Assembly.GetExecutingAssembly();
		}

		public override void SetupAssetRepository(IList<IContentSource> sources, AssetReaderCollection assetReaderCollection, IList<Type> delayedLoadTypes)
		{
			sources.Clear();
			sources.Add(new AssemblyResourcesContentSource(Assembly.GetExecutingAssembly(), "Terraria.ModLoader.Default."));
		}

		public override void Load() {
			AddContent(new UnloadedTile());
			AddContent(new UnloadedTile("PendingUnloadedTile"));

			AddContent(new UnloadedNonSolidTile());
			AddContent(new UnloadedNonSolidTile("PendingUnloadedNonSolidTile"));

			AddContent(new UnloadedChest());
			AddContent(new UnloadedChest("PendingUnloadedChest"));

			AddContent(new UnloadedWall());
			AddContent(new UnloadedWall("PendingUnloadedWall"));

			PatronSets = GetContent<PatreonItem>().GroupBy(t => t.InternalSetName).Select(set => set.ToArray()).ToArray();
			DeveloperSets = GetContent<DeveloperItem>().GroupBy(t => t.InternalSetName).Select(set => set.ToArray()).ToArray();
		}

		public override void Unload() {
			PatronSets = null;
			DeveloperSets = null;
		}

		internal static Texture2D ReadTexture(string file) => ModContent.GetInstance<ModLoaderMod>().GetTexture(file).Value;

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
