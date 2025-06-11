using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Terraria.ModLoader;

[Autoload(false)]
public abstract class VanillaExtraJump : ExtraJump
{
	public sealed override Position GetDefaultPosition() => null;

	public sealed override IEnumerable<Position> GetModdedConstraints() => null;
}

public sealed class FlipperJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 1f;

	public override bool CanStart(Player player)
	{
		return (!player.mount.Active || !player.mount.Cart) && player.wet;
	}

	public override void OnStarted(Player player, ref bool playSound)
	{
		if (player.swimTime == 0)
			player.swimTime = 30;

		if (player.sliding)
			player.velocity.X = 3 * -player.slideDir;

		playSound = false;

		player.GetJumpState(this).Available = true;
	}
}

public sealed class GoatMountJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 2f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		Vector2 center2 = player.Center;
		Vector2 vector4 = new Vector2(50f, 20f);
		float num12 = (float)Math.PI * 2f * Main.rand.NextFloat();
		for (int n = 0; n < 5; n++) {
			for (float num13 = 0f; num13 < 14f; num13 += 1f) {
				Dust obj2 = Main.dust[Dust.NewDust(center2, 0, 0, 6)];
				Vector2 vector5 = Vector2.UnitY.RotatedBy(num13 * ((float)Math.PI * 2f) / 14f + num12);
				vector5 *= 0.2f * (float)n;
				obj2.position = center2 + vector5 * vector4;
				obj2.velocity = vector5 + new Vector2(0f, player.gravDir * 4f);
				obj2.noGravity = true;
				obj2.scale = 1f + Main.rand.NextFloat() * 0.8f;
				obj2.fadeIn = Main.rand.NextFloat() * 2f;
				obj2.shader = GameShaders.Armor.GetSecondaryShader(player.cMount, player);
			}
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.5f;
	}
}

public sealed class BasiliskMountJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 0.75f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		Vector2 center2 = player.Center;
		Vector2 vector4 = new Vector2(50f, 20f);
		float num12 = (float)Math.PI * 2f * Main.rand.NextFloat();
		for (int n = 0; n < 5; n++) {
			for (float num13 = 0f; num13 < 14f; num13 += 1f) {
				Dust obj2 = Main.dust[Dust.NewDust(center2, 0, 0, 31)];
				Vector2 vector5 = Vector2.UnitY.RotatedBy(num13 * ((float)Math.PI * 2f) / 14f + num12);
				vector5 *= 0.2f * (float)n;
				obj2.position = center2 + vector5 * vector4;
				obj2.velocity = vector5 + new Vector2(0f, player.gravDir * 4f);
				obj2.noGravity = true;
				obj2.scale = 1f + Main.rand.NextFloat() * 0.8f;
				obj2.fadeIn = Main.rand.NextFloat() * 2f;
				obj2.shader = GameShaders.Armor.GetSecondaryShader(player.cMount, player);
			}
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.5f;
	}
}

public sealed class SantankMountJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 2f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		int num17 = player.height;
		if (player.gravDir == -1f)
			num17 = 0;

		for (int num18 = 0; num18 < 15; num18++) {
			int num19 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num17 - 16f), 102, 32, 4, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, new Color(250, 230, 230, 150), 1.5f);
			Main.dust[num19].velocity.X = Main.dust[num19].velocity.X * 0.5f - player.velocity.X * 0.1f;
			Main.dust[num19].velocity.Y = Main.dust[num19].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
			Main.dust[num19].noGravity = true;
			num19 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num17 - 16f), 102, 32, 6, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 20, default(Color), 1.5f);
			Main.dust[num19].velocity.Y -= 1f;
			if (num18 % 2 == 0)
				Main.dust[num19].fadeIn = Main.rand.NextFloat() * 2f;
		}

		float y = player.Bottom.Y - 22f;
		for (int num20 = 0; num20 < 3; num20++) {
			Vector2 vector8 = player.Center;
			switch (num20) {
				case 0:
					vector8 = new Vector2(player.Center.X - 16f, y);
					break;
				case 1:
					vector8 = new Vector2(player.position.X - 36f, y);
					break;
				case 2:
					vector8 = new Vector2(player.Right.X + 4f, y);
					break;
			}

			int num21 = Gore.NewGore(vector8, new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(61, 63));
			Main.gore[num21].velocity *= 0.1f;
			Main.gore[num21].velocity.X -= player.velocity.X * 0.1f;
			Main.gore[num21].velocity.Y -= player.velocity.Y * 0.05f;
			Main.gore[num21].velocity += Main.rand.NextVector2Circular(1f, 1f) * 0.5f;
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.5f;
	}
}

public sealed class UnicornMountJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 2f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		Vector2 center = player.Center;
		Vector2 vector2 = new Vector2(50f, 20f);
		float num10 = (float)Math.PI * 2f * Main.rand.NextFloat();
		for (int m = 0; m < 5; m++) {
			for (float num11 = 0f; num11 < 14f; num11 += 1f) {
				Dust obj = Main.dust[Dust.NewDust(center, 0, 0, Utils.SelectRandom<int>(Main.rand, 176, 177, 179))];
				Vector2 vector3 = Vector2.UnitY.RotatedBy(num11 * ((float)Math.PI * 2f) / 14f + num10);
				vector3 *= 0.2f * (float)m;
				obj.position = center + vector3 * vector2;
				obj.velocity = vector3 + new Vector2(0f, player.gravDir * 4f);
				obj.noGravity = true;
				obj.scale = 1f + Main.rand.NextFloat() * 0.8f;
				obj.fadeIn = Main.rand.NextFloat() * 2f;
				obj.shader = GameShaders.Armor.GetSecondaryShader(player.cMount, player);
			}
		}
	}

	public override void ShowVisuals(Player player)
	{
		Dust obj = Main.dust[Dust.NewDust(player.position, player.width, player.height, Utils.SelectRandom(Main.rand, 176, 177, 179))];
		obj.velocity = Vector2.Zero;
		obj.noGravity = true;
		obj.scale = 0.5f + Main.rand.NextFloat() * 0.8f;
		obj.fadeIn = 1f + Main.rand.NextFloat() * 2f;
		obj.shader = GameShaders.Armor.GetSecondaryShader(player.cMount, player);
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.5f;
	}
}

public sealed class SandstormInABottleJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 3f;

	public override void ShowVisuals(Player player)
	{
		int num3 = player.height;
		if (player.gravDir == -1f)
			num3 = -6;

		float num4 = ((float)player.jump / 75f + 1f) / 2f;
		for (int i = 0; i < 3; i++) {
			int num5 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)(num3 / 2)), player.width, 32, 124, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 150, default(Color), 1f * num4);
			Main.dust[num5].velocity *= 0.5f * num4;
			Main.dust[num5].fadeIn = 1.5f * num4;
		}

		player.sandStorm = true;
		if (player.miscCounter % 3 == 0) {
			int num6 = Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 18f, player.position.Y + (float)(num3 / 2)), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(220, 223), num4);
			Main.gore[num6].velocity = player.velocity * 0.3f * num4;
			Main.gore[num6].alpha = 100;
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 1.5f;
		player.maxRunSpeed *= 2f;
	}
}

public sealed class BlizzardInABottleJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 1.5f;

	public override void ShowVisuals(Player player)
	{
		int num12 = player.height - 6;
		if (player.gravDir == -1f)
			num12 = 6;

		for (int k = 0; k < 2; k++) {
			int num13 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)num12), player.width, 12, 76, player.velocity.X * 0.3f, player.velocity.Y * 0.3f);
			Main.dust[num13].velocity *= 0.1f;
			if (k == 0)
				Main.dust[num13].velocity += player.velocity * 0.03f;
			else
				Main.dust[num13].velocity -= player.velocity * 0.03f;

			Main.dust[num13].velocity -= player.velocity * 0.1f;
			Main.dust[num13].noGravity = true;
			Main.dust[num13].noLight = true;
		}

		for (int l = 0; l < 3; l++) {
			int num14 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)num12), player.width, 12, 76, player.velocity.X * 0.3f, player.velocity.Y * 0.3f);
			Main.dust[num14].fadeIn = 1.5f;
			Main.dust[num14].velocity *= 0.6f;
			Main.dust[num14].velocity += player.velocity * 0.8f;
			Main.dust[num14].noGravity = true;
			Main.dust[num14].noLight = true;
		}

		for (int m = 0; m < 3; m++) {
			int num15 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)num12), player.width, 12, 76, player.velocity.X * 0.3f, player.velocity.Y * 0.3f);
			Main.dust[num15].fadeIn = 1.5f;
			Main.dust[num15].velocity *= 0.6f;
			Main.dust[num15].velocity -= player.velocity * 0.8f;
			Main.dust[num15].noGravity = true;
			Main.dust[num15].noLight = true;
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.5f;
	}
}

public sealed class FartInAJarJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 2f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		int num7 = player.height;
		if (player.gravDir == -1f)
			num7 = 0;

		playSound = false;
		SoundEngine.PlaySound(SoundID.Item16, player.position);

		for (int l = 0; l < 10; l++) {
			int num8 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num7 - 16f), 102, 32, 188, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
			Main.dust[num8].velocity.X = Main.dust[num8].velocity.X * 0.5f - player.velocity.X * 0.1f;
			Main.dust[num8].velocity.Y = Main.dust[num8].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
		}

		int num9 = Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 16f, player.position.Y + (float)num7 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(435, 438));
		Main.gore[num9].velocity.X = Main.gore[num9].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num9].velocity.Y = Main.gore[num9].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
		num9 = Gore.NewGore(new Vector2(player.position.X - 36f, player.position.Y + (float)num7 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(435, 438));
		Main.gore[num9].velocity.X = Main.gore[num9].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num9].velocity.Y = Main.gore[num9].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
		num9 = Gore.NewGore(new Vector2(player.position.X + (float)player.width + 4f, player.position.Y + (float)num7 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(435, 438));
		Main.gore[num9].velocity.X = Main.gore[num9].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num9].velocity.Y = Main.gore[num9].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
	}

	public override void ShowVisuals(Player player)
	{
		int num7 = player.height;
		if (player.gravDir == -1f)
			num7 = -6;

		int num8 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)num7), player.width + 8, 4, 188, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
		Main.dust[num8].velocity.X = Main.dust[num8].velocity.X * 0.5f - player.velocity.X * 0.1f;
		Main.dust[num8].velocity.Y = Main.dust[num8].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
		Main.dust[num8].velocity *= 0.5f;
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 3f;
		player.maxRunSpeed *= 1.75f;
	}
}

public sealed class TsunamiInABottleJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 1.25f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		int num5 = player.height;
		if (player.gravDir == -1f)
			num5 = 0;
		for (int k = 0; k < 30; k++) {
			int num6 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)num5), player.width, 12, 253, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, default(Color), 1.5f);
			if (k % 2 == 0)
				Main.dust[num6].velocity.X += (float)Main.rand.Next(30, 71) * 0.1f;
			else
				Main.dust[num6].velocity.X -= (float)Main.rand.Next(30, 71) * 0.1f;

			Main.dust[num6].velocity.Y += (float)Main.rand.Next(-10, 31) * 0.1f;
			Main.dust[num6].noGravity = true;
			Main.dust[num6].scale += (float)Main.rand.Next(-10, 41) * 0.01f;
			Main.dust[num6].velocity *= Main.dust[num6].scale * 0.7f;
			Vector2 vector = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
			vector.Normalize();
			vector *= (float)Main.rand.Next(81) * 0.1f;
		}
	}

	public override void ShowVisuals(Player player)
	{
		int num9 = 1;
		if (player.jump > 0)
			num9 = 2;

		int num10 = player.height - 6;
		if (player.gravDir == -1f)
			num10 = 6;

		for (int j = 0; j < num9; j++) {
			int num11 = Dust.NewDust(new Vector2(player.position.X, player.position.Y + (float)num10), player.width, 12, 253, player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 100, default(Color), 1.5f);
			Main.dust[num11].scale += (float)Main.rand.Next(-5, 3) * 0.1f;
			if (player.jump <= 0)
				Main.dust[num11].scale *= 0.8f;
			else
				Main.dust[num11].velocity -= player.velocity / 5f;

			Main.dust[num11].noGravity = true;
			Vector2 vector = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
			vector.Normalize();
			vector *= (float)Main.rand.Next(81) * 0.1f;
		}
	}

	public override void UpdateHorizontalSpeeds(Player player)
	{
		player.runAcceleration *= 1.5f;
		player.maxRunSpeed *= 1.25f;
	}
}

public sealed class CloudInABottleJump : VanillaExtraJump
{
	public override float GetDurationMultiplier(Player player) => 0.75f;

	public override void OnStarted(Player player, ref bool playSound)
	{
		int num22 = player.height;
		if (player.gravDir == -1f)
			num22 = 0;

		for (int num23 = 0; num23 < 10; num23++) {
			int num24 = Dust.NewDust(new Vector2(player.position.X - 34f, player.position.Y + (float)num22 - 16f), 102, 32, 16, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
			Main.dust[num24].velocity.X = Main.dust[num24].velocity.X * 0.5f - player.velocity.X * 0.1f;
			Main.dust[num24].velocity.Y = Main.dust[num24].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
		}

		int num25 = Gore.NewGore(new Vector2(player.position.X + (float)(player.width / 2) - 16f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
		Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
		num25 = Gore.NewGore(new Vector2(player.position.X - 36f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
		Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
		num25 = Gore.NewGore(new Vector2(player.position.X + (float)player.width + 4f, player.position.Y + (float)num22 - 16f), new Vector2(0f - player.velocity.X, 0f - player.velocity.Y), Main.rand.Next(11, 14));
		Main.gore[num25].velocity.X = Main.gore[num25].velocity.X * 0.1f - player.velocity.X * 0.1f;
		Main.gore[num25].velocity.Y = Main.gore[num25].velocity.Y * 0.1f - player.velocity.Y * 0.05f;
	}

	public override void ShowVisuals(Player player)
	{
		int num = player.height;
		if (player.gravDir == -1f)
			num = -6;

		int num2 = Dust.NewDust(new Vector2(player.position.X - 4f, player.position.Y + (float)num), player.width + 8, 4, 16, (0f - player.velocity.X) * 0.5f, player.velocity.Y * 0.5f, 100, default(Color), 1.5f);
		Main.dust[num2].velocity.X = Main.dust[num2].velocity.X * 0.5f - player.velocity.X * 0.1f;
		Main.dust[num2].velocity.Y = Main.dust[num2].velocity.Y * 0.5f - player.velocity.Y * 0.3f;
	}
}
