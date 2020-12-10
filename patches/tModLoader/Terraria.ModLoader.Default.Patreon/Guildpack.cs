using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Guildpack_Head : PatreonItem
	{
		public override string SetName => "Guildpack";
		public override EquipType ItemEquipType => EquipType.Head;

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			PatronModPlayer.Player(player).GuildpackSet = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}

		public static readonly PlayerLayer MiscEffectsBack = new PlayerLayer("ModLoader", "MiscEffectsBack", PlayerLayer.MiscEffectsBack, delegate (PlayerDrawInfo drawInfo) {
			if (drawInfo.shadow != 0f) {
				return;
			}
			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("ModLoader");
			Texture2D texture = mod.GetTexture("Patreon.Guildpack_Aura");
			int frameSize = texture.Height / 3;
			int drawX = (int)(drawInfo.position.X + drawPlayer.width / 2f - Main.screenPosition.X);
			int drawY = (int)(drawInfo.position.Y + drawPlayer.height / 2f - Main.screenPosition.Y);
			int frame = (int)(DateTime.Now.Millisecond / 167 % 3);
			DrawData data = new DrawData(texture, new Vector2(drawX, drawY), new Rectangle(0, frameSize * frame, texture.Width, frameSize), Color.White, 0f, new Vector2(texture.Width / 2f, frameSize / 2f), 1f, drawInfo.spriteEffects, 0);
			Main.playerDrawData.Add(data);
		});

		public static readonly PlayerLayer EyeGlow = new PlayerLayer("ModLoader", "EyeGlow", PlayerLayer.Head, delegate (PlayerDrawInfo drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("ModLoader");
			Texture2D texture = mod.GetTexture("Patreon.Guildpack_Head_Glow");
			DrawData data = new DrawData(texture,
				new Vector2((int)(drawInfo.position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2)), (int)(drawInfo.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.headPosition + drawInfo.headOrigin, drawPlayer.bodyFrame, Color.White, drawPlayer.headRotation, drawInfo.headOrigin, 1f, drawInfo.spriteEffects, 0);
			Main.playerDrawData.Add(data);
		});
	}

	internal class Guildpack_Body : PatreonItem
	{
		public override string SetName => "Guildpack";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	internal class Guildpack_Legs : PatreonItem
	{
		public override string SetName => "Guildpack";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
