using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Saethar_Head : PatreonItem
	{
		public override string SetName => "Saethar";
		public override EquipType ItemEquipType => EquipType.Head;

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			PatronModPlayer.Player(player).SaetharSet = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(34);
		}

		public static readonly PlayerLayer EyeGlow = new PlayerLayer("ModLoader", "EyeGlow", PlayerLayer.Head, delegate (PlayerDrawInfo drawInfo) {
			Player drawPlayer = drawInfo.drawPlayer;
			Mod mod = ModLoader.GetMod("ModLoader");
			Texture2D texture = mod.GetTexture("Patreon.Saethar_Head_Glow");
			DrawData data = new DrawData(texture,
				new Vector2((int)(drawInfo.position.X - Main.screenPosition.X - (float)(drawPlayer.bodyFrame.Width / 2) + (float)(drawPlayer.width / 2)), (int)(drawInfo.position.Y - Main.screenPosition.Y + (float)drawPlayer.height - (float)drawPlayer.bodyFrame.Height + 4f)) + drawPlayer.headPosition + drawInfo.headOrigin, drawPlayer.bodyFrame, Color.White, drawPlayer.headRotation, drawInfo.headOrigin, 1f, drawInfo.spriteEffects, 0);
			Main.playerDrawData.Add(data);
		});
	}

	internal class Saethar_Body : PatreonItem
	{
		public override string SetName => "Saethar";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(30, 18);
		}
	}

	internal class Saethar_Legs : PatreonItem
	{
		public override string SetName => "Saethar";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}
	}

	internal class Saethar_Wings : PatreonItem
	{
		public override string SetName => "Saethar";
		public override EquipType ItemEquipType => EquipType.Wings;

		public override void SetDefaults() {
			base.SetDefaults();
			item.vanity = false;
			item.width = 24;
			item.height = 8;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.wingTimeMax = 150;
		}

	}
}