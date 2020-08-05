using System;
using System.Collections.Generic;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal sealed class AndromedonEffect : ICloneable
	{
		public bool HasSetBonus;
		public bool HasAura => _auraTime > 0;
		public float LayerStrength;
		public float ShaderStrength;

		private int _lastLife = -1;
		internal int _auraTime;

		private int? _headSlot;
		private int? _bodySlot;
		private int? _legSlot;

		public void ResetEffects() {
			HasSetBonus = false;
		}

		public void UpdateDead() {
			HasSetBonus = false;
			_auraTime = 0;
		}

		public void UpdateEffects(Player player) {
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
					player.Center,
					Main.DiscoColor.ToVector3() * LayerStrength * ((float)Main.time % 2) * (float)Math.Abs(Math.Log10(Main.essScale * 0.75f)));
			}
		}

		public void UpdateAura(Player player) {
			if (_lastLife <= -1) {
				_lastLife = player.statLife;
			}
			int diff = _lastLife - player.statLife;
			if (diff >= 0.1f * player.statLifeMax2) {
				_auraTime = 300 + diff;
			}
			_lastLife = player.statLife;
		}

		public void ModifyDrawLayers(Mod mod, Player player, List<PlayerLayer> layers) {
			_headSlot ??= mod.GetEquipSlot($"PowerRanger_{EquipType.Head}", EquipType.Head);
			_bodySlot ??= mod.GetEquipSlot($"PowerRanger_{EquipType.Body}", EquipType.Body);
			_legSlot ??= mod.GetEquipSlot($"PowerRanger_{EquipType.Legs}", EquipType.Legs);

			if (LayerStrength >= 0f) {
				int i;

				if (player.head == _headSlot) {
					PowerRanger_Head.GlowLayer.visible = true;

					i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Head"));
					if (i != -1) {
						if (ShaderStrength > 0f) {
							PowerRanger_Head.ShaderLayer.visible = true;
							layers.Insert(i - 1, PowerRanger_Head.ShaderLayer);
						}
						layers.Insert(i + 2, PowerRanger_Head.GlowLayer);
					}
				}

				if (player.body == _bodySlot) {
					if (ShaderStrength > 0f) {
						PowerRanger_Body.ShaderLayer.visible = true;
						i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Body"));
						if (i != -1) {
							layers.Insert(i - 1, PowerRanger_Body.ShaderLayer);
						}
					}

					PowerRanger_Body.GlowLayer.visible = true;
					i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Arms"));
					if (i != -1) {
						layers.Insert(i + 1, PowerRanger_Body.GlowLayer);
					}
				}

				if (player.legs == _legSlot) {
					PowerRanger_Legs.GlowLayer.visible = true;

					i = layers.FindIndex(x => x.mod.Equals("Terraria") && x.Name.Equals("Legs"));
					if (i != -1) {
						if (ShaderStrength > 0f) {
							PowerRanger_Legs.ShaderLayer.visible = true;
							layers.Insert(i - 1, PowerRanger_Legs.ShaderLayer);
						}
						layers.Insert(i + 2, PowerRanger_Legs.GlowLayer);
					}
				}
			}
		}

		public object Clone() {
			return MemberwiseClone();
		}
	}
}
