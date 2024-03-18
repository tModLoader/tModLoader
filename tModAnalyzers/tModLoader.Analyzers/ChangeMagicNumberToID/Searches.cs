using System.Collections.Generic;
using ReLogic.Reflection;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace tModLoader.Analyzers.ChangeMagicNumberToID;

public static class Searches
{
	private static readonly Dictionary<string, IdDictionary> dictByMetadataName;

	static Searches()
	{
		dictByMetadataName = new Dictionary<string, IdDictionary> {
			[typeof(AchievementHelperID.Events).FullName] = AchievementHelperID.Events.Search,
			[typeof(AchievementHelperID.Special).FullName] = AchievementHelperID.Special.Search,
			[typeof(AmmoID).FullName] = AmmoID.Search,
			[typeof(ArmorIDs.Head).FullName] = ArmorIDs.Head.Search,
			[typeof(ArmorIDs.Body).FullName] = ArmorIDs.Body.Search,
			[typeof(ArmorIDs.Legs).FullName] = ArmorIDs.Legs.Search,
			[typeof(ArmorIDs.HandOn).FullName] = ArmorIDs.HandOn.Search,
			[typeof(ArmorIDs.HandOff).FullName] = ArmorIDs.HandOff.Search,
			[typeof(ArmorIDs.Back).FullName] = ArmorIDs.Back.Search,
			[typeof(ArmorIDs.Front).FullName] = ArmorIDs.Front.Search,
			[typeof(ArmorIDs.Shoe).FullName] = ArmorIDs.Shoe.Search,
			[typeof(ArmorIDs.Waist).FullName] = ArmorIDs.Waist.Search,
			[typeof(ArmorIDs.Wing).FullName] = ArmorIDs.Wing.Search,
			[typeof(ArmorIDs.Shield).FullName] = ArmorIDs.Shield.Search,
			[typeof(ArmorIDs.Neck).FullName] = ArmorIDs.Neck.Search,
			[typeof(ArmorIDs.Face).FullName] = ArmorIDs.Face.Search,
			[typeof(ArmorIDs.Balloon).FullName] = ArmorIDs.Balloon.Search,
			[typeof(ArmorIDs.RocketBoots).FullName] = ArmorIDs.RocketBoots.Search,
			[typeof(ArmorIDs.Beard).FullName] = ArmorIDs.Beard.Search,
			[typeof(BiomeConversionID).FullName] = BiomeConversionID.Search,
			[typeof(BuffID).FullName] = BuffID.Search,
			[typeof(ChainID).FullName] = ChainID.Search,
			[typeof(CloudID).FullName] = CloudID.Search,
			[typeof(CursorOverrideID).FullName] = CursorOverrideID.Search,
			[typeof(DashID).FullName] = DashID.Search,
			[typeof(DustID).FullName] = DustID.Search,
			[typeof(EmoteID).FullName] = EmoteID.Search,
			[typeof(EmoteID.Category).FullName] = EmoteID.Category.Search,
			[typeof(ExtrasID).FullName] = ExtrasID.Search,
			[typeof(GameEventClearedID).FullName] = GameEventClearedID.Search,
			[typeof(GameModeID).FullName] = GameModeID.Search,
			[typeof(GlowMaskID).FullName] = GameModeID.Search,
			[typeof(GoreID).FullName] = GoreID.Search,
			[typeof(HousingCategoryID).FullName] = HousingCategoryID.Search,
			[typeof(ImmunityCooldownID).FullName] = ImmunityCooldownID.Search,
			[typeof(InvasionID).FullName] = InvasionID.Search,
			[typeof(ItemAlternativeFunctionID).FullName] = ItemAlternativeFunctionID.Search,
			[typeof(ItemHoldStyleID).FullName] = ItemHoldStyleID.Search,
			[typeof(ItemID).FullName] = ItemID.Search,
			[typeof(ItemRarityID).FullName] = ItemRarityID.Search,
			[typeof(ItemUseStyleID).FullName] = ItemUseStyleID.Search,
			[typeof(LangID).FullName] = LangID.Search,
			[typeof(LiquidID).FullName] = LiquidID.Search,
			[typeof(MessageID).FullName] = MessageID.Search,
			[typeof(MountID).FullName] = MountID.Search,
			[typeof(MusicID).FullName] = MusicID.Search,
			[typeof(NetmodeID).FullName] = NetmodeID.Search,
			[typeof(NPCAIStyleID).FullName] = NPCAIStyleID.Search,
			[typeof(NPCHeadID).FullName] = NPCHeadID.Search,
			[typeof(NPCID).FullName] = NPCID.Search,
			[typeof(PaintCoatingID).FullName] = PaintCoatingID.Search,
			[typeof(PlayerDifficultyID).FullName] = PlayerDifficultyID.Search,
			[typeof(PlayerTextureID).FullName] = PlayerTextureID.Search,
			[typeof(PlayerVariantID).FullName] = PlayerVariantID.Search,
			[typeof(PrefixID).FullName] = PrefixID.Search,
			[typeof(ProjAIStyleID).FullName] = ProjAIStyleID.Search,
			[typeof(ProjectileID).FullName] = ProjectileID.Search,
			[typeof(RecipeGroupID).FullName] = RecipeGroupID.Search,
			[typeof(StatusID).FullName] = StatusID.Search,
			[typeof(SurfaceBackgroundID).FullName] = SurfaceBackgroundID.Search,
			[typeof(TeleportationStyleID).FullName] = TeleportationStyleID.Search,
			[typeof(TileID).FullName] = TileID.Search,
			[typeof(TorchID).FullName] = TorchID.Search,
			[typeof(WallID).FullName] = WallID.Search,
			[typeof(WaterStyleID).FullName] = WaterStyleID.Search,
		};
	}

	public static bool TryGetByMetadataName(string metadataName, out IdDictionary search)
	{
		return dictByMetadataName.TryGetValue(metadataName, out search);
	}
}
