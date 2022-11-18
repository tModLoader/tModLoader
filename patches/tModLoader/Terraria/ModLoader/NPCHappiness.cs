using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary> This struct provides access to an NPC type's NPC &amp; Biome relationships. </summary>
public readonly struct NPCHappiness
{
	/// <summary> Allows you to modify the shop price multipliers associated with a (biome/npc type) relationship level. </summary>
	public static readonly Dictionary<AffectionLevel, float> AffectionLevelToPriceMultiplier = new() {
		{ AffectionLevel.Hate, ShopHelper.hateValue },
		{ AffectionLevel.Dislike, ShopHelper.dislikeValue },
		{ AffectionLevel.Like, ShopHelper.likeValue },
		{ AffectionLevel.Love, ShopHelper.loveValue },
	};

	public readonly int NpcType;

	private NPCHappiness(int npcType)
	{
		NpcType = npcType;
	}

	public NPCHappiness SetNPCAffection<T>(AffectionLevel affectionLevel) where T : ModNPC
		=> SetNPCAffection(ModContent.GetInstance<T>().Type, affectionLevel);

	public NPCHappiness SetNPCAffection(int npcId, AffectionLevel affectionLevel)
	{
		var profile = Main.ShopHelper._database.GetOrCreateProfileByNPCID(NpcType);
		var shopModifiers = profile.ShopModifiers;

		var existingEntry = (NPCPreferenceTrait)shopModifiers.SingleOrDefault(t => t is NPCPreferenceTrait npcPreference && npcPreference.NpcId == npcId);

		if (existingEntry != null) {
			if (affectionLevel == 0) {
				shopModifiers.Remove(existingEntry);
				return this;
			}

			existingEntry.Level = affectionLevel;
			return this;
		}

		shopModifiers.Add(new NPCPreferenceTrait {
			NpcId = npcId,
			Level = affectionLevel,
		});

		return this;
	}

	public NPCHappiness SetBiomeAffection<T>(AffectionLevel affectionLevel) where T : class, ILoadable, IShoppingBiome
		=> SetBiomeAffection(ModContent.GetInstance<T>(), affectionLevel);

	public NPCHappiness SetBiomeAffection(IShoppingBiome biome, AffectionLevel affectionLevel)
	{
		var profile = Main.ShopHelper._database.GetOrCreateProfileByNPCID(NpcType);
		var shopModifiers = profile.ShopModifiers;
		bool removal = affectionLevel == 0;

		var biomePreferenceList = (BiomePreferenceListTrait)shopModifiers.SingleOrDefault(t => t is BiomePreferenceListTrait);

		if (biomePreferenceList == null) {
			if (removal)
				return this;

			shopModifiers.Add(biomePreferenceList = new BiomePreferenceListTrait());
		}

		var biomePreferences = biomePreferenceList.Preferences;
		var existingEntry = biomePreferenceList.SingleOrDefault(p => p.Biome == biome);

		if (existingEntry != null) {
			if (removal) {
				biomePreferences.Remove(existingEntry);
				return this;
			}

			existingEntry.Affection = affectionLevel;
			return this;
		}

		biomePreferenceList.Add(affectionLevel, biome);

		return this;
	}

	public static NPCHappiness Get(int npcType) => new(npcType);

	internal static void RegisterVanillaNpcRelationships()
	{
		for (int i = 0; i < NPCID.Count; i++) {
			var npc = ContentSamples.NpcsByNetId[i];

			AllPersonalitiesModifier.RegisterVanillaNpcRelationships(npc);
		}
	}
}
