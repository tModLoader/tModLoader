using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			var profile = Main.ShopHelper._database.GetOrCreateProfileByNPCID(npcId);
			bool removal = affectionLevel == 0;

			for (int i = 0; i < profile.ShopModifiers.Count; i++) {
				var personalityTrait = profile.ShopModifiers[i];

				if (personalityTrait is not NPCPreferenceTrait npcPreference) {
					continue;
				}

				if (npcPreference.NpcId != npcId) {
					continue;
				}

				if (removal) {
					profile.ShopModifiers.RemoveAt(i--);
				} else {
					npcPreference.Level = affectionLevel;
					removal = true; // Remove every repeating match after this.
				}
			}

			// If this affection level isn't neutral, and an existing personality trait hasn't been found.
			if (!removal) {
				profile.ShopModifiers.Add(new NPCPreferenceTrait {
					NpcId = npcId,
					Level = affectionLevel,
				});
			}
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
