using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Head : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == Mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == Mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == Mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			DeveloperPlayer.GetPlayer(player).AndromedonEffect.HasSetBonus = true;
		}

		private static Asset<Texture2D> _glowTexture;
		private static Asset<Texture2D> _shaderTexture;

		public static PlayerDrawLayer GlowLayer = CreateGlowLayer("AndromedonHeadGlow", true, PlayerDrawLayer.Head, drawInfo => {
			_glowTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Head_Head_Glow");

			return GetHeadDrawDataInfo(drawInfo, _glowTexture.Value);
		});

		public static PlayerDrawLayer ShaderLayer = CreateShaderLayer("AndromedonHeadShader", true, PlayerDrawLayer.Torso, drawInfo => {
			_shaderTexture ??= ModContent.GetTexture("ModLoader/Developer.Jofairden.PowerRanger_Head_Head_Shader");

			return GetHeadDrawDataInfo(drawInfo, _shaderTexture.Value);
		});
	}
}
