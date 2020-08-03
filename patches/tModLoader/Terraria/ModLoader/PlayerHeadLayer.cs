using Terraria.DataStructures;
using static Terraria.DataStructures.PlayerDrawHeadLayers;

namespace Terraria.ModLoader
{
	/// <summary> This class represents a DrawLayer for the player's map icon, and uses PlayerDrawHeadInfo as its InfoType. Drawing should be done directly through drawInfo.spriteBatch. </summary>
	[Autoload]
	public abstract class PlayerHeadLayer : DrawLayer<PlayerDrawHeadSet>
	{
		/// <summary> Draws the back textures of the player's headgear. </summary>
		public static readonly PlayerHeadLayer BackHelmet = CreateVanillaLayer(nameof(BackHelmet), DrawPlayer_00_BackHelmet);

		/// <summary> Draws the player's head's skin. </summary>
		public static readonly PlayerHeadLayer FaceSkin = CreateVanillaLayer(nameof(FaceSkin), DrawPlayer_01_FaceSkin);
		
		/// <summary> Draws the player's headgear and hair. </summary>
		public static readonly PlayerHeadLayer DrawArmorWithFullHair = CreateVanillaLayer(nameof(DrawArmorWithFullHair), DrawPlayer_02_DrawArmorWithFullHair);
		
		/// <summary> Draws the player's under-headgear hair. </summary>
		public static readonly PlayerHeadLayer HelmetHair = CreateVanillaLayer(nameof(HelmetHair), DrawPlayer_03_HelmetHair);
		
		/// <summary> Draws the player's jungle rose, if they have one equipped. </summary>
		public static readonly PlayerHeadLayer JungleRose = CreateVanillaLayer(nameof(JungleRose), DrawPlayer_04_JungleRose);
		
		/// <summary> Draws the player's tall hat, if they have one equipped. </summary>
		public static readonly PlayerHeadLayer TallHats = CreateVanillaLayer(nameof(TallHats), DrawPlayer_05_TallHats);
		
		/// <summary> Draws the player's normal hat, if they have one equipped. </summary>
		public static readonly PlayerHeadLayer NormalHats = CreateVanillaLayer(nameof(NormalHats), DrawPlayer_06_NormalHats);
		
		/// <summary> Draws the player's hair. </summary>
		public static readonly PlayerHeadLayer JustHair = CreateVanillaLayer(nameof(JustHair), DrawPlayer_07_JustHair);

		/// <summary> Draws the player's face accessories, if they have any. </summary>
		public static readonly PlayerHeadLayer FaceAcc = CreateVanillaLayer(nameof(FaceAcc), DrawPlayer_08_FaceAcc);

		protected override void Register() {

		}

		private static PlayerHeadLayer CreateVanillaLayer(string name, LayerFunction layer) => new LegacyPlayerHeadLayer(null, name, layer);
	}
}
