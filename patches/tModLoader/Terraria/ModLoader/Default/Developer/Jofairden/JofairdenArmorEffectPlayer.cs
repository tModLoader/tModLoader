using System;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal sealed class JofairdenArmorEffectPlayer : ModPlayer
	{
		public bool HasSetBonus;
		public float LayerStrength;
		public float ShaderStrength;

		private int _lastLife = -1;
		private int _auraTime;

		public bool HasAura => _auraTime > 0;

		public override bool CloneNewInstances => true;

		public override void ResetEffects() {
			HasSetBonus = false;
		}

		public override void UpdateDead() {
			HasSetBonus = false;
			_auraTime = 0;
		}

		public override void PostUpdate() {
			if (!HasAura) {
				if (ShaderStrength > 0f) {
					ShaderStrength -= 0.02f;
				}
			}
			else {
				if (ShaderStrength <= 1f) {
					ShaderStrength += 0.02f;
				}
				else {
					_auraTime--;
				}
			}

			if (!HasSetBonus) {
				if (LayerStrength > 0) {
					LayerStrength -= 0.02f;
				}
			}
			else {
				if (LayerStrength <= 1) {
					LayerStrength += 0.02f;
				}
			}

			if (ShaderStrength > 0f) {
				Lighting.AddLight(
					Player.Center,
					Main.DiscoColor.ToVector3() * LayerStrength * ((float)Main.time % 2) * (float)Math.Abs(Math.Log10(Main.essScale * 0.75f))
				);
			}
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (_lastLife <= -1) {
				_lastLife = Player.statLife;
			}

			int diff = _lastLife - Player.statLife;

			if (diff >= 0.1f * Player.statLifeMax2) {
				_auraTime = 300 + diff;
			}

			_lastLife = Player.statLife;
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
