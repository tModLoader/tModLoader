using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.DataStructures;

#nullable enable

namespace Terraria.ModLoader.Default.Patreon;

internal class GuildpackSetEffectPlayer : ModPlayer
{
	public bool IsActive;

	public override void ResetEffects()
	{
		IsActive = false;
	}
}

internal class GuildpackMiscEffectsDrawLayer : PlayerDrawLayer
{
	private Asset<Texture2D>? textureAsset;

	public override Position GetDefaultPosition()
		=> new BeforeParent(PlayerDrawLayers.JimsCloak); // Preferably before everything

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> drawInfo.drawPlayer.TryGetModPlayer(out GuildpackSetEffectPlayer modPlayer) && modPlayer.IsActive;

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		if (drawInfo.shadow != 0f) {
			return;
		}

		textureAsset ??= ModContent.Request<Texture2D>("ModLoader/Patreon.Guildpack_Aura");

		if (!textureAsset.IsLoaded || textureAsset.Value is not Texture2D texture) {
			return;
		}

		var player = drawInfo.drawPlayer;
		int frameSize = texture.Height / 3;
		int frame = (int)(DateTime.Now.Millisecond / 167 % 3);
		var position = (drawInfo.Position + (player.Size * 0.5f) - Main.screenPosition).ToPoint().ToVector2();
		var srcRect = new Rectangle(0, frameSize * frame, texture.Width, frameSize);

		drawInfo.DrawDataCache.Add(new DrawData(texture, position, srcRect, Color.White, 0f, new Vector2(texture.Width, frameSize) * 0.5f, 1f, drawInfo.playerEffect, 0));
	}
}

internal class GuildpackEyeGlowDrawLayer : PlayerDrawLayer
{
	private Asset<Texture2D>? textureAsset;

	public override Position GetDefaultPosition()
		=> new AfterParent(PlayerDrawLayers.Head);

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		=> drawInfo.drawPlayer.TryGetModPlayer(out GuildpackSetEffectPlayer modPlayer) && modPlayer.IsActive;

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		textureAsset ??= ModContent.Request<Texture2D>("ModLoader/Patreon.Guildpack_Head_Glow");

		if (!textureAsset.IsLoaded || textureAsset.Value is not Texture2D texture) {
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
internal class Guildpack_Head : PatreonItem
{
	private static int[]? equipSlots;

	public override bool IsVanitySet(int head, int body, int legs)
	{
		equipSlots ??= new int[] {
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Head}", EquipType.Head),
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Body}", EquipType.Body),
			EquipLoader.GetEquipSlot(Mod, $"{InternalSetName}_{EquipType.Legs}", EquipType.Legs),
		};

		return head == equipSlots[0]
			&& body == equipSlots[1]
			&& legs == equipSlots[2];
	}

	public override void UpdateVanitySet(Player player)
	{
		if (player.TryGetModPlayer(out GuildpackSetEffectPlayer modPlayer)) {
			modPlayer.IsActive = true;
		}
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 34;
		Item.height = 22;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Guildpack_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 42;
		Item.height = 24;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Guildpack_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}
