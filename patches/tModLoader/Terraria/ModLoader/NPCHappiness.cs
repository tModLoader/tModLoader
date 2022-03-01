using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary> This struct provides access to an NPC type's NPC &amp; Biome relationships. </summary>
	public readonly struct NPCHappiness
	{
		/*
		private static readonly IReadOnlyDictionary<int, AffectionLevel> emptyDictionaryDummy = new ReadOnlyDictionary<int, AffectionLevel>(new Dictionary<int, AffectionLevel>());

		internal static readonly Dictionary<int, Dictionary<int, AffectionLevel>> NpcToNpcAffection = new();
		internal static readonly Dictionary<int, Dictionary<int, AffectionLevel>> NpcToBiomeRelationship = new();
		*/

		/// <summary> Allows you to modify the shop price multipliers associated with a (biome/npc type) relationship level. </summary>
		public static readonly Dictionary<AffectionLevel, float> AffectionLevelToPriceMultiplier = new() {
			{ AffectionLevel.Hate, ShopHelper.hateValue },
			{ AffectionLevel.Dislike, ShopHelper.dislikeValue },
			{ AffectionLevel.Like, ShopHelper.likeValue },
			{ AffectionLevel.Love, ShopHelper.loveValue },
		};

		public readonly int NpcType;

		/*
		public IReadOnlyDictionary<int, AffectionLevel> NpcTypeAffectionLevels
			=> NpcToNpcAffection.TryGetValue(NpcType, out var result) ? result : emptyDictionaryDummy;

		public IReadOnlyDictionary<int, AffectionLevel> BiomeTypeRelationships
			=> NpcToBiomeRelationship.TryGetValue(NpcType, out var result) ? result : emptyDictionaryDummy;
		*/

		private NPCHappiness(int npcType) {
			NpcType = npcType;
		}

		/*
		public void LoveNPC(int npcId)
			=> SetAffection(npcId, AffectionLevel.Love, NpcToNpcAffection);

		public void LikeNPC(int npcId)
			=> SetAffection(npcId, AffectionLevel.Like, NpcToNpcAffection);

		public void DislikeNPC(int npcId)
			=> SetAffection(npcId, AffectionLevel.Dislike, NpcToNpcAffection);

		public void HateNPC(int npcId)
			=> SetAffection(npcId, AffectionLevel.Hate, NpcToNpcAffection);

		public void LoveBiome(int biomeId)
			=> SetRelationship(biomeId, AffectionLevel.Love, NpcToBiomeRelationship);

		public void LikeBiome(int biomeId)
			=> SetRelationship(biomeId, AffectionLevel.Like, NpcToBiomeRelationship);

		public void DislikeBiome(int biomeId)
			=> SetRelationship(biomeId, AffectionLevel.Dislike, NpcToBiomeRelationship);

		public void HateBiome(int biomeId)
			=> SetRelationship(biomeId, AffectionLevel.Hate, NpcToBiomeRelationship);
		*/

		public void SetNpcAffection(int npcId, AffectionLevel affectionLevel) {
			var profile = Main.ShopHelper._database.GetOrCreateProfileByNPCID(NpcType);
			var shopModifiers = profile.ShopModifiers;

			var existingEntry = (NPCPreferenceTrait)shopModifiers.SingleOrDefault(t => t is NPCPreferenceTrait npcPreference && npcPreference.NpcId == npcId);

			if (existingEntry != null) {
				if (affectionLevel == 0) {
					shopModifiers.Remove(existingEntry);
					return;
				}

				existingEntry.Level = affectionLevel;
				return;
			}

			shopModifiers.Add(new NPCPreferenceTrait {
				NpcId = npcId,
				Level = affectionLevel,
			});
		}

		public void SetBiomeAffection(IShoppingBiome biome, AffectionLevel affectionLevel) {
			var profile = Main.ShopHelper._database.GetOrCreateProfileByNPCID(NpcType);
			var shopModifiers = profile.ShopModifiers;
			bool removal = affectionLevel != 0;

			var biomePreferenceList = (BiomePreferenceListTrait)shopModifiers.SingleOrDefault(t => t is BiomePreferenceListTrait);

			if (biomePreferenceList == null) {
				if (removal)
					return;

				shopModifiers.Add(biomePreferenceList = new BiomePreferenceListTrait());
			}

			var biomePreferences = biomePreferenceList.Preferences;
			var existingEntry = biomePreferenceList.SingleOrDefault(p => p.Biome == biome);

			if (existingEntry != null) {
				if (removal) {
					biomePreferences.Remove(existingEntry);
					return;
				}

				existingEntry.Affection = affectionLevel;
				return;
			}

			biomePreferenceList.Add(affectionLevel, biome);
		}

		public static NPCHappiness Get(int npcType) => new(npcType);

		internal static void ResetRelationships() {
			/*
			NpcToNpcAffection.Clear();
			NpcToBiomeRelationship.Clear();
			*/
		}

		internal static void RegisterVanillaNpcRelationships() {
			for (int i = 0; i < NPCID.Count; i++) {
				var npc = ContentSamples.NpcsByNetId[i];

				AllPersonalitiesModifier.RegisterVanillaNpcRelationships(npc);
			}
		}
	}
}
