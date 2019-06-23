using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Developer
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
			_headSlot = _headSlot ?? mod.GetEquipSlot($"PowerRanger_{EquipType.Head}", EquipType.Head);
			_bodySlot = _bodySlot ?? mod.GetEquipSlot($"PowerRanger_{EquipType.Body}", EquipType.Body);
			_legSlot = _legSlot ?? mod.GetEquipSlot($"PowerRanger_{EquipType.Legs}", EquipType.Legs);

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

	internal class DrawDataInfo
	{
		public Vector2 Position;
		public Rectangle? Frame;
		public float Rotation;
		public Texture2D Texture;
		public Vector2 Origin;
	}

	abstract class AndromedonItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";
		public sealed override string SetName => "PowerRanger";
		public const int ShaderNumSegments = 8;
		public const int ShaderDrawOffset = 2;

		private static int? ShaderId;

		public sealed override void SetStaticDefaults() {
			DisplayName.SetDefault($"Andromedon {EquipTypeSuffix}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}

		protected static Vector2 GetDrawOffset(int i) {
			return new Vector2(0, ShaderDrawOffset).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);

			//var halfDist = ShaderDrawOffset / 2;
			//var offY = halfDist + halfDist * i % halfDist;
			//return new Vector2(0, offY).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);
		}

		private static void BeginShaderBatch(SpriteBatch batch) {
			batch.End();
			RasterizerState rasterizerState = Main.LocalPlayer.gravDir == 1f ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
			batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public static DrawDataInfo GetHeadDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X + drawPlayer.width / 2f - drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X),
							  (int)(drawInfo.position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y))
						  + drawPlayer.headPosition
						  + drawInfo.headOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.headOrigin,
				Rotation = drawPlayer.headRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetBodyDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f))
						  + drawPlayer.bodyPosition
						  + drawInfo.bodyOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.bodyFrame,
				Origin = drawInfo.bodyOrigin,
				Rotation = drawPlayer.bodyRotation,
				Texture = texture
			};
		}

		public static DrawDataInfo GetLegDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture) {
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f))
						  + drawPlayer.legPosition
						  + drawInfo.legOrigin;

			return new DrawDataInfo {
				Position = pos,
				Frame = drawPlayer.legFrame,
				Origin = drawInfo.legOrigin,
				Rotation = drawPlayer.legRotation,
				Texture = texture
			};
		}

		public static PlayerLayer CreateShaderLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc) {
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) => {
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis) {
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);
				Player drawPlayer = drawInfo.drawPlayer;
				DeveloperPlayer devPlayer = DeveloperPlayer.GetPlayer(drawPlayer);
				SpriteEffects effects = SpriteEffects.None;
				if (drawPlayer.direction == -1) {
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1) {
					effects |= SpriteEffects.FlipVertically;
				}

				DrawData data = new DrawData(
					drawDataInfo.Texture,
					drawDataInfo.Position,
					drawDataInfo.Frame,
					Color.White * Main.essScale * devPlayer.AndromedonEffect.LayerStrength * devPlayer.AndromedonEffect.ShaderStrength,
					drawDataInfo.Rotation,
					drawDataInfo.Origin,
					1f,
					effects,
					0);

				BeginShaderBatch(Main.spriteBatch);
				ShaderId = ShaderId ?? GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye);
				GameShaders.Armor.Apply(ShaderId.Value, drawPlayer, data);
				var centerPos = data.position;

				for (int i = 0; i < ShaderNumSegments; i++) {
					data.position = centerPos + GetDrawOffset(i);
					data.Draw(Main.spriteBatch);
				}

				data.position = centerPos;
			});
		}

		public static PlayerLayer CreateGlowLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc) {
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) => {
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis) {
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);

				Player drawPlayer = drawInfo.drawPlayer;
				DeveloperPlayer devPlayer = DeveloperPlayer.GetPlayer(drawPlayer);
				SpriteEffects effects = SpriteEffects.None;
				if (drawPlayer.direction == -1) {
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1) {
					effects |= SpriteEffects.FlipVertically;
				}

				DrawData data = new DrawData(
					drawDataInfo.Texture,
					drawDataInfo.Position,
					drawDataInfo.Frame,
					Color.White * Main.essScale * devPlayer.AndromedonEffect.LayerStrength,
					drawDataInfo.Rotation,
					drawDataInfo.Origin,
					1f,
					effects,
					0);

				if (devPlayer.AndromedonEffect.HasAura) {
					ShaderId = ShaderId ?? GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye);
					data.shader = ShaderId.Value;
				}
				Main.playerDrawData.Add(data);
			});
		}
	}

	internal class PowerRanger_Head : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs) {
			return head == mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player) {
			DeveloperPlayer.GetPlayer(player).AndromedonEffect.HasSetBonus = true;
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonHeadGlow", PlayerLayer.Head, drawInfo => {
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Head_Head_Glow");
			return GetHeadDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonHeadShader", PlayerLayer.Body, drawInfo => {
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Head_Head_Shader");
			return GetHeadDrawDataInfo(drawInfo, _shaderTexture);
		});
	}

	internal class PowerRanger_Body : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(34, 22);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonBodyGlow", PlayerLayer.Body, drawInfo => {
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Glow");
			return GetBodyDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonBodyShader", PlayerLayer.Body, drawInfo => {
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Shader");
			return GetBodyDrawDataInfo(drawInfo, _shaderTexture);
		});
	}

	internal class PowerRanger_Legs : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonLegsGlow", PlayerLayer.Head, drawInfo => {
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Glow");
			return GetLegDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonLegsShader", PlayerLayer.Body, drawInfo => {
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Shader");
			return GetLegDrawDataInfo(drawInfo, _shaderTexture);
		});
	}
}
