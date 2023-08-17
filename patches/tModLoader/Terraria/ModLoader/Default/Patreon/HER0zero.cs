using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
    [AutoloadEquip(EquipType.Head)]
    internal class HER0zero_Head : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 30;
			Item.height = 20;
        }
    }

    [AutoloadEquip(EquipType.Body)]
	internal class HER0zero_Body : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 32;
			Item.height = 20;
        }

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == EquipLoader.GetEquipSlot(Mod, "HER0zero_Head", EquipType.Head)
				&& body == EquipLoader.GetEquipSlot(Mod, "HER0zero_Body", EquipType.Body)
				&& legs == EquipLoader.GetEquipSlot(Mod, "HER0zero_Legs", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			player.GetModPlayer<HER0zeroPlayer>().glowEffect = true;
		}
	}

    [AutoloadEquip(EquipType.Legs)]
	internal class HER0zero_Legs : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 24;
			Item.height = 16;
        }
	}

	internal class HER0zeroPlayer : ModPlayer
	{
		public bool glowEffect = false;

		public override void ResetEffects() {
			glowEffect = false;
		}
	}

	internal class HER0zeroGlowEffect : PlayerDrawLayer
	{
		private Asset<Texture2D>? textureAsset;

		public override Position GetDefaultPosition() {
			return new BeforeParent(PlayerDrawLayers.JimsCloak); // Preferably before everything
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.TryGetModPlayer(out HER0zeroPlayer modPlayer) && modPlayer.glowEffect;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			if (drawInfo.shadow != 0f) {
				return;
			}

			textureAsset ??= ModContent.Request<Texture2D>("ModLoader/Patreon.HER0zero_Effect");

			if (!textureAsset.IsLoaded || textureAsset.Value is not Texture2D texture) {
				return;
			}

			Player player = drawInfo.drawPlayer;
			int frameSize = texture.Height / 4;
			int frame = (player.miscCounter % 40) / 10;
			float alpha = 0.5f;
			Vector2 position = (drawInfo.Position + (player.Size * 0.5f) - Main.screenPosition).ToPoint().ToVector2();
			Rectangle srcRect = new Rectangle(0, frameSize * frame, texture.Width, frameSize);

			drawInfo.DrawDataCache.Add(new DrawData(texture, position, srcRect, Color.White * alpha, 0f, new Vector2(texture.Width, frameSize) * 0.5f, 1f, drawInfo.playerEffect, 0));
		}
	}
}
