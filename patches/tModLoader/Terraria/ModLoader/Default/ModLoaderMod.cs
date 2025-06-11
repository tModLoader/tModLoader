using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.Default.Developer;
using Terraria.ModLoader.Default.Patreon;

namespace Terraria.ModLoader.Default;

internal class ModLoaderMod : Mod
{
	private static PatreonItem[][] PatronSets;
	private static DeveloperItem[][] DeveloperSets;
	private const int ChanceToGetPatreonArmor = 14;
	private const int ChanceToGetDevArmor = 50;

	internal const byte AccessorySlotPacket = 0;
	internal const byte StatResourcesPacket = 1;

	public override string Name => "ModLoader";
	public override Version Version => BuildInfo.tMLVersion;

	internal ModLoaderMod()
	{
		Side = ModSide.NoSync;
		DisplayName = "tModLoader";
		Code = Assembly.GetExecutingAssembly();
	}

	public override IContentSource CreateDefaultContentSource() => new AssemblyResourcesContentSource(Assembly.GetExecutingAssembly(), "Terraria.ModLoader.Default.");

	public override void Load()
	{
		PatronSets = GetContent<PatreonItem>().GroupBy(t => t.InternalSetName).Select(set => set.ToArray()).ToArray();
		DeveloperSets = GetContent<DeveloperItem>().GroupBy(t => t.InternalSetName).Select(set => set.ToArray()).ToArray();
	}

	public override void Unload()
	{
		PatronSets = null;
		DeveloperSets = null;
	}

	internal static bool TryGettingPatreonOrDevArmor(IEntitySource source, Player player)
	{
		if (Main.rand.NextBool(ChanceToGetPatreonArmor)) {
			int randomIndex = Main.rand.Next(PatronSets.Length);

			foreach (var patreonItem in PatronSets[randomIndex]) {
				player.QuickSpawnItem(source, patreonItem.Type);
			}

			return true;
		}

		if (Main.rand.NextBool(ChanceToGetDevArmor)) {
			int randomIndex = Main.rand.Next(DeveloperSets.Length);

			foreach (var developerItem in DeveloperSets[randomIndex]) {
				player.QuickSpawnItem(source, developerItem.Type);
			}

			return true;
		}
		return false;
	}

	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		byte packetType = reader.ReadByte();

		switch (packetType) {
			case AccessorySlotPacket:
				ModAccessorySlotPlayer.NetHandler.HandlePacket(reader, whoAmI);
				break;
			case StatResourcesPacket:
				ConsumedStatIncreasesPlayer.NetHandler.HandlePacket(reader, whoAmI);
				break;
		}
	}

	internal static ModPacket GetPacket(byte packetType)
	{
		ModPacket packet = ModContent.GetInstance<ModLoaderMod>().GetPacket();
		packet.Write(packetType);
		return packet;
	}
}
