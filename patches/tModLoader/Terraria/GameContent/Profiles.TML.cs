using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace Terraria.GameContent;

public partial class Profiles
{
	/// <summary>
	/// Class that is some-what identical to <seealso cref="LegacyNPCProfile"/> that allows for
	/// modded texture usage. Also allows for any potential children classes to mess with the fields.
	/// </summary>
	public class DefaultNPCProfile : ITownNPCProfile
	{
		protected string rootFilePath;
		protected int npcVariationHeadSlot;
		protected Asset<Texture2D> defaultNoAlt;
		protected Asset<Texture2D> defaultParty;

		public DefaultNPCProfile(string npcTexturePath, int npcHeadSlot, string npcPartyTexturePath = null)
		{
			rootFilePath = npcTexturePath;
			npcVariationHeadSlot = npcHeadSlot;

			if (Main.dedServ) {
				return;
			}

			defaultNoAlt = ModContent.Request<Texture2D>(npcTexturePath);
			defaultParty = !string.IsNullOrEmpty(npcPartyTexturePath) ? ModContent.Request<Texture2D>(npcPartyTexturePath) : defaultNoAlt;
		}

		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
		{
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn) {
				return defaultNoAlt;
			}

			return npc.altTexture == 1 ? defaultParty : defaultNoAlt;
		}

		public int GetHeadTextureIndex(NPC npc) => npcVariationHeadSlot;
	}
}
