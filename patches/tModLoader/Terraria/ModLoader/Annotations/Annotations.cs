using System;

namespace Terraria.ModLoader.Annotations;

[AttributeUsage(
	AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
	AllowMultiple = false
)]
public sealed class IDTypeAttribute : Attribute
{
	public const string AchievementHelper_Events = nameof(AchievementHelper_Events);
	public const string AchievementHelper_Special = nameof(AchievementHelper_Special);
	public const string Ammo = nameof(Ammo);
	public const string Animation = nameof(Animation);
	public const string Armor_Head = nameof(Armor_Head);
	public const string Armor_Body = nameof(Armor_Body);
	public const string Armor_Legs = nameof(Armor_Legs);
	public const string Armor_HandOn = nameof(Armor_HandOn); // Not supported yet due to duplicate IDs.
	public const string Armor_HandOff = nameof(Armor_HandOff); // Not supported yet due to duplicate IDs.
	public const string Armor_Back = nameof(Armor_Back);
	public const string Armor_Front = nameof(Armor_Front);
	public const string Armor_Shoe = nameof(Armor_Shoe); // Not supported yet due to duplicate IDs.
	public const string Armor_Waist = nameof(Armor_Waist); // Not supported yet due to duplicate IDs.
	public const string Armor_Wing = nameof(Armor_Wing);
	public const string Armor_Shield = nameof(Armor_Shield);
	public const string Armor_Neck = nameof(Armor_Neck);
	public const string Armor_Face = nameof(Armor_Face);
	public const string Armor_Balloon = nameof(Armor_Balloon);
	public const string Armor_RocketBoots = nameof(Armor_RocketBoots);
	public const string Armor_Beard = nameof(Armor_Beard);
	public const string BiomeConversion = nameof(BiomeConversion);
	public const string Buff = nameof(Buff);
	public const string Chain = nameof(Chain);
	public const string Cloud = nameof(Cloud);
	public const string CursorOverride = nameof(CursorOverride);
	public const string Dust = nameof(Dust); // Not supported yet due to duplicate IDs.
	public const string Emote = nameof(Emote);
	public const string EmoteCategory = nameof(EmoteCategory);
	public const string Extras = nameof(Extras);
	public const string GameEventCleared = nameof(GameEventCleared);
	public const string GameMode = nameof(GameMode);
	public const string GlowMask = nameof(GlowMask);
	public const string Gore = nameof(Gore);
	public const string HousingCategory = nameof(HousingCategory);
	public const string ImmunityCooldown = nameof(ImmunityCooldown);
	public const string Invasion = nameof(Invasion);
	public const string ItemAlternativeFunction = nameof(ItemAlternativeFunction);
	public const string ItemHoldStyle = nameof(ItemHoldStyle);
	public const string Item = nameof(Item);
	public const string ItemRarity = nameof(ItemRarity);
	public const string ItemUseStyle = nameof(ItemUseStyle);
	public const string Lang = nameof(Lang);
	public const string Liquid = nameof(Liquid);
	public const string Message = nameof(Message);
	public const string Mount = nameof(Mount);
	public const string Music = nameof(Music);
	public const string Netmode = nameof(Netmode);
	public const string NPCAIStyle = nameof(NPCAIStyle);
	public const string NPCHead = nameof(NPCHead);
	public const string NPC = nameof(NPC);
	public const string PaintCoating = nameof(PaintCoating);
	public const string PlayerDifficulty = nameof(PlayerDifficulty);
	public const string PlayerTexture = nameof(PlayerTexture);
	public const string PlayerVariant = nameof(PlayerVariant);
	public const string Prefix = nameof(Prefix);
	public const string ProjAIStyle = nameof(ProjAIStyle);
	public const string Projectile = nameof(Projectile);
	public const string RecipeGroup = nameof(RecipeGroup);
	public const string Status = nameof(Status);
	public const string SurfaceBackground = nameof(SurfaceBackground);
	public const string TeleportationStyle = nameof(TeleportationStyle);
	public const string Tile = nameof(Tile);
	public const string Torch = nameof(Torch);
	public const string Wall = nameof(Wall);
	public const string WaterStyle = nameof(WaterStyle);

	public string Type { get; }

	public IDTypeAttribute(string type)
	{
		Type = type;
	}
}
