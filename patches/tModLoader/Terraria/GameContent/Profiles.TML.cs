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
		protected int currentHeadSlot;
		protected Asset<Texture2D> defaultTexture;
		protected Asset<Texture2D> partyTexture;

		public DefaultNPCProfile(string texturePath, int headSlot, string partTexturePath = null)
		{
			currentHeadSlot = headSlot;

			if (Main.dedServ) {
				return;
			}

			defaultTexture = ModContent.Request<Texture2D>(texturePath);
			partyTexture = !string.IsNullOrEmpty(partTexturePath) ? ModContent.Request<Texture2D>(partTexturePath) : defaultTexture;
		}

		public int RollVariation() => 0;
		public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

		public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc)
		{
			if (npc.IsABestiaryIconDummy && !npc.ForcePartyHatOn) {
				return defaultTexture;
			}

			return npc.altTexture == 1 ? partyTexture : defaultTexture;
		}

		public int GetHeadTextureIndex(NPC npc) => currentHeadSlot;
	}
}
