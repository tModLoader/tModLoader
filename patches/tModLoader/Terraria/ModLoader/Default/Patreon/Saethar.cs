using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace Terraria.ModLoader.Default.Patreon;

internal class SaetharSetEffectPlayer : ModPlayer
{
	public bool IsActive;

	public override void ResetEffects()
	{
		IsActive = false;
	}
}

internal class SaetharEyeGlowDrawLayer : PlayerDrawLayer
{
	private Asset<Texture2D>? textureAsset;

	public override Position GetDefaultPosition()
		=> new AfterParent(PlayerDrawLayers.Head);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> drawInfo.drawPlayer.TryGetModPlayer(out SaetharSetEffectPlayer modPlayer) && modPlayer.IsActive;

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		// Don't render as after-image
		if (drawInfo.shadow != 0f) {
			return;
		}

		textureAsset ??= ModContent.Request<Texture2D>("ModLoader/Patreon.Saethar_Head_Glow");

		if (textureAsset is not { IsLoaded: true, Value: Texture2D texture }) {
			return;
		}

		var headOrigin = drawInfo.headVect;
		var player = drawInfo.drawPlayer;

		//TODO: Mitigate the need for this boilerplate.
		Vector2 position = player.headPosition + drawInfo.headVect + new Vector2(
			(int)(drawInfo.Position.X + player.width / 2f - player.bodyFrame.Width / 2f - Main.screenPosition.X),
			(int)(drawInfo.Position.Y + player.height - player.bodyFrame.Height + 4f - Main.screenPosition.Y)
		);

		drawInfo.DrawDataCache.Add(new DrawData(texture, position, player.bodyFrame, Color.White, player.headRotation, headOrigin, 1f, drawInfo.playerEffect, 0));
	}
}

[AutoloadEquip(EquipType.Head)]
internal class Saethar_Head : PatreonItem
{
	private static int[]? equipSlots;

	public override bool IsVanitySet(int head, int body, int legs)
	{
		equipSlots ??= new int[] {
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Head}", EquipType.Head),
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Body}", EquipType.Body),
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Legs}", EquipType.Legs),
		};

		return head == equipSlots[0]; // && body == equipSlots[1] && legs == equipSlots[2];
	}

	public override void UpdateVanitySet(Player player)
	{
		if (player.TryGetModPlayer(out SaetharSetEffectPlayer modPlayer)) {
			modPlayer.IsActive = true;
		}
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(34);
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Saethar_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(30, 18);
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Saethar_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(22, 18);
	}
}

[AutoloadEquip(EquipType.Wings)]
internal class Saethar_Wings : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.vanity = false;
		Item.width = 24;
		Item.height = 8;
		Item.accessory = true;
	}
}
