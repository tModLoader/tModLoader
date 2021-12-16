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
		public enum Relationship : sbyte
		{
			Neutral,
			Hate,
			Dislike,
			Like,
			Love
		}

		private static readonly IReadOnlyDictionary<int, Relationship> emptyDictionaryDummy = new ReadOnlyDictionary<int, Relationship>(new Dictionary<int, Relationship>());

		internal static readonly Dictionary<int, Dictionary<int, Relationship>> NpcToNpcRelationship = new();
		internal static readonly Dictionary<int, Dictionary<int, Relationship>> NpcToBiomeRelationship = new();

		/// <summary> Allows you to modify the shop price multipliers associated with a (biome/npc type) relationship level. </summary>
		public static readonly Dictionary<Relationship, float> RelationshipToPriceMultiplier = new() {
			{ Relationship.Neutral, 1f },
			{ Relationship.Hate, ShopHelper.hateValue },
			{ Relationship.Dislike, ShopHelper.dislikeValue },
			{ Relationship.Like, ShopHelper.likeValue },
			{ Relationship.Love, ShopHelper.loveValue },
		};

		public readonly int NpcType;

		public IReadOnlyDictionary<int, Relationship> NpcTypeRelationships
			=> NpcToNpcRelationship.TryGetValue(NpcType, out var result) ? result : emptyDictionaryDummy;

		public IReadOnlyDictionary<int, Relationship> BiomeTypeRelationships
			=> NpcToBiomeRelationship.TryGetValue(NpcType, out var result) ? result : emptyDictionaryDummy;

		public NPCHappiness(int npcType) {
			NpcType = npcType;
		}

		public void LoveNPC(int npcId)
			=> SetRelationship(npcId, Relationship.Love, NpcToNpcRelationship);

		public void LikeNPC(int npcId)
			=> SetRelationship(npcId, Relationship.Like, NpcToNpcRelationship);

		public void DislikeNPC(int npcId)
			=> SetRelationship(npcId, Relationship.Dislike, NpcToNpcRelationship);

		public void HateNPC(int npcId)
			=> SetRelationship(npcId, Relationship.Hate, NpcToNpcRelationship);

		public void LoveBiome(int biomeId)
			=> SetRelationship(biomeId, Relationship.Love, NpcToBiomeRelationship);

		public void LikeBiome(int biomeId)
			=> SetRelationship(biomeId, Relationship.Like, NpcToBiomeRelationship);

		public void DislikeBiome(int biomeId)
			=> SetRelationship(biomeId, Relationship.Dislike, NpcToBiomeRelationship);

		public void HateBiome(int biomeId)
			=> SetRelationship(biomeId, Relationship.Hate, NpcToBiomeRelationship);

		private void SetRelationship(int index, Relationship relationship, Dictionary<int, Dictionary<int, Relationship>> dictionaries) {
			bool removal = relationship == 0;

			if (!dictionaries.TryGetValue(NpcType, out var subDictionary)) {
				if (removal) {
					return;
				}

				dictionaries[NpcType] = subDictionary = new();
			}

			if (removal) {
				subDictionary.Remove(index);
				return;
			}

			subDictionary[index] = relationship;
		}

		internal static void ResetRelationships() {
			NpcToNpcRelationship.Clear();
			NpcToBiomeRelationship.Clear();
		}

		internal static void RegisterVanillaNpcRelationships() {
			for (int i = 0; i < NPCID.Count; i++) {
				var npc = ContentSamples.NpcsByNetId[i];

				AllPersonalitiesModifier.RegisterVanillaNpcRelationships(npc);
			}
		}
	}
}
