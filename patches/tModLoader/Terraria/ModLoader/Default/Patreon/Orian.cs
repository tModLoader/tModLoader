using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

internal class OrianSetEffectPlayer : ModPlayer
{
	public bool IsActive;

	public override void ResetEffects()
	{
		IsActive = false;
	}

	public override void PostUpdate()
	{
		if (!IsActive) {
			return;
		}

		var player = Player;
		var playerCenter = player.Center;
		bool closeToAnEnemy = Main.npc.Any(x => x != Main.npc[Main.maxNPCs] && x.active && !x.friendly && !NPCID.Sets.TownCritter[x.type] && x.type != NPCID.TargetDummy && x.WithinRange(player.position, 300));
		float maxIntensity = 0f;

		const float MaxDistance = 512f;
		const float MaxDistanceSqr = MaxDistance * MaxDistance;

		for (int i = 0; i < Main.maxNPCs; i++) {
			var npc = Main.npc[i];

			if (!npc.active || npc.damage <= 0 || npc.friendly || NPCID.Sets.TownCritter[npc.type] || npc.type == NPCID.TargetDummy) {
				continue;
			}

			float distanceSquared = npc.DistanceSQ(playerCenter);
			float intensity = 1f - MathF.Min(1f, distanceSquared / MaxDistanceSqr);

			intensity *= intensity;

			maxIntensity = MathF.Max(maxIntensity, intensity);
		}

		if (maxIntensity > 0f) {
			float pulse = (MathF.Sin((float)Main.GameUpdateCount / 17f) * 0.25f) + 0.75f;

			Lighting.AddLight(playerCenter, Color.DeepSkyBlue.ToVector3() * maxIntensity * pulse * 1.5f);
		}
	}
}

[AutoloadEquip(EquipType.Head)]
internal class Orian_Head : PatreonItem
{
	public override bool IsVanitySet(int head, int body, int legs)
	{
		return head == EquipLoader.GetEquipSlot(Mod, nameof(Orian_Head), EquipType.Head)
			&& body == EquipLoader.GetEquipSlot(Mod, nameof(Orian_Body), EquipType.Body)
			&& legs == EquipLoader.GetEquipSlot(Mod, nameof(Orian_Legs), EquipType.Legs);
	}

	public override void UpdateVanitySet(Player player)
	{
		player.GetModPlayer<OrianSetEffectPlayer>().IsActive = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 24;
		Item.height = 24;
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Orian_Body : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 30;
		Item.height = 20;
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Orian_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 22;
		Item.height = 18;
	}
}
