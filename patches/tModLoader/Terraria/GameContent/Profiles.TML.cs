using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.GameContent
{
	public partial class Profiles
	{
		public class AlternateLegacyNPCProfile : ITownNPCProfile
		{
			private string _rootFilePath;
			private int _defaultVariationHeadIndex;
			private Asset<Texture2D> _defaultNoAlt;

			public AlternateLegacyNPCProfile(string npcFileTitleFilePath, int defaultHeadIndex) {
				_rootFilePath = npcFileTitleFilePath;
				_defaultVariationHeadIndex = defaultHeadIndex;
				if (Main.dedServ)
					return;

				_defaultNoAlt = Main.Assets.Request<Texture2D>(npcFileTitleFilePath, AssetRequestMode.DoNotLoad);
			}

			public int RollVariation() => 0;
			public string GetNameForVariant(NPC npc) => NPC.getNewNPCName(npc.type);

			public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => _defaultNoAlt;

			public int GetHeadTextureIndex(NPC npc) => _defaultVariationHeadIndex;
		}
	}
}
