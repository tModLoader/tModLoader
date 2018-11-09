using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Developer
{
	class DrawDataInfo
	{
		public Vector2 position;
		public Texture2D texture;
		public Rectangle? frame;
		public float rotation;
		public Vector2 origin;
	}

	abstract class AndromedonItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";
		public sealed override string SetName => "PowerRanger";
		public const int ShaderNumSegments = 8;
		public const int ShaderDrawOffset = 2;

		public sealed override void SetStaticDefaults()
		{
			DisplayName.SetDefault($"Andromedon {EquipTypeSuffix}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}

		protected static Vector2 GetDrawOffset(int i)
		{
			return new Vector2(0, ShaderDrawOffset).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);

			//var halfDist = ShaderDrawOffset / 2;
			//var offY = halfDist + halfDist * i % halfDist;
			//return new Vector2(0, offY).RotatedBy((float)i / ShaderNumSegments * MathHelper.TwoPi);
		}

		public static float LayerStrength = 0f;

		private static void BeginShaderBatch(SpriteBatch batch)
		{
			batch.End();
			RasterizerState rasterizerState = Main.LocalPlayer.gravDir == 1f ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
			batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, rasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
		}

		//private static void ResetBatch(SpriteBatch batch)
		//{
		//	batch.End();
		//	RasterizerState rasterizerState = Main.LocalPlayer.gravDir == 1f ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
		//	batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rasterizerState, null, Main.GameViewMatrix.TransformationMatrix);
		//	//Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		//}

		public static DrawDataInfo GetHeadDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X + drawPlayer.width / 2f - drawPlayer.bodyFrame.Width / 2f - Main.screenPosition.X),
							  (int)(drawInfo.position.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f - Main.screenPosition.Y))
						  + drawPlayer.headPosition
						  + drawInfo.headOrigin;

			return new DrawDataInfo
			{
				position = pos,
				frame = drawPlayer.bodyFrame,
				texture = texture,
				origin = drawInfo.headOrigin,
				rotation = drawPlayer.headRotation
			};
		}

		public static DrawDataInfo GetBodyDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f))
						  + drawPlayer.bodyPosition
						  + drawInfo.bodyOrigin;

			return new DrawDataInfo
			{
				position = pos,
				frame = drawPlayer.bodyFrame,
				texture = texture,
				origin = drawInfo.bodyOrigin,
				rotation = drawPlayer.bodyRotation
			};
		}

		public static DrawDataInfo GetLegDrawDataInfo(PlayerDrawInfo drawInfo, Texture2D texture)
		{
			Player drawPlayer = drawInfo.drawPlayer;
			Vector2 pos = new Vector2(
							  (int)(drawInfo.position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f),
							  (int)(drawInfo.position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f))
						  + drawPlayer.legPosition
						  + drawInfo.legOrigin;

			return new DrawDataInfo
			{
				position = pos,
				frame = drawPlayer.legFrame,
				texture = texture,
				origin = drawInfo.legOrigin,
				rotation = drawPlayer.legRotation
			};
		}

		public static PlayerLayer CreateShaderLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc)
		{
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) =>
			{
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis)
				{
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);
				Player drawPlayer = drawInfo.drawPlayer;
				SpriteEffects effects = SpriteEffects.None;
				if (drawPlayer.direction == -1)
				{
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1)
				{
					effects |= SpriteEffects.FlipVertically;
				}

				DrawData data = new DrawData(
					drawDataInfo.texture,
					drawDataInfo.position,
					drawDataInfo.frame,
					Color.White * Main.essScale * LayerStrength,
					drawDataInfo.rotation,
					drawDataInfo.origin,
					1f,
					effects,
					0);

				SpriteBatch backup = Main.spriteBatch;

				BeginShaderBatch(Main.spriteBatch);
				GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye), drawPlayer, data);
				var centerPos = data.position;

				for (int i = 0; i < ShaderNumSegments; i++)
				{
					data.position = centerPos + GetDrawOffset(i);
					data.Draw(Main.spriteBatch);
				}

				data.position = centerPos;
				Main.spriteBatch = backup;
			});
		}

		public static PlayerLayer CreateGlowLayer(string name, PlayerLayer parent, Func<PlayerDrawInfo, DrawDataInfo> getDataFunc)
		{
			return new PlayerLayer("ModLoaderMod", name, parent, (drawInfo) =>
			{
				if (drawInfo.shadow != 0f || drawInfo.drawPlayer.invis)
				{
					return;
				}

				DrawDataInfo drawDataInfo = getDataFunc.Invoke(drawInfo);

				Player drawPlayer = drawInfo.drawPlayer;
				SpriteEffects effects = SpriteEffects.None;
				if (drawPlayer.direction == -1)
				{
					effects |= SpriteEffects.FlipHorizontally;
				}

				if (drawPlayer.gravDir == -1)
				{
					effects |= SpriteEffects.FlipVertically;
				}

				DrawData data = new DrawData(
					drawDataInfo.texture,
					drawDataInfo.position,
					drawDataInfo.frame,
					Color.White * Main.essScale * LayerStrength,
					drawDataInfo.rotation,
					drawDataInfo.origin,
					1f,
					effects,
					0)
				{
					shader = GameShaders.Armor.GetShaderIdFromItemId(ItemID.LivingRainbowDye)
				};
				Main.playerDrawData.Add(data);
			});
		}
	}

	internal class PowerRanger_Head : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(18, 20);
		}

		public override bool IsVanitySet(int head, int body, int legs)
		{
			return head == mod.GetEquipSlot($"{SetName}_{EquipType.Head}", EquipType.Head)
				   && body == mod.GetEquipSlot($"{SetName}_{EquipType.Body}", EquipType.Body)
				   && legs == mod.GetEquipSlot($"{SetName}_{EquipType.Legs}", EquipType.Legs);
		}

		public override void UpdateVanitySet(Player player)
		{
			DeveloperPlayer.GetPlayer(player).AndromedonSet = true;
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonHeadGlow", PlayerLayer.Head, drawInfo =>
		{
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Head_Head_Glow");
			return GetHeadDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonHeadShader", PlayerLayer.Body, drawInfo =>
		{
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Head_Head_Shader");
			return GetHeadDrawDataInfo(drawInfo, _shaderTexture);
		});
	}

	internal class PowerRanger_Body : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(26, 22);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonBodyGlow", PlayerLayer.Body, drawInfo =>
		{
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Glow");
			return GetBodyDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonBodyShader", PlayerLayer.Body, drawInfo =>
		{
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Body_Body_Shader");
			return GetBodyDrawDataInfo(drawInfo, _shaderTexture);
		});
	}

	internal class PowerRanger_Legs : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(20, 12);
		}

		private static Texture2D _glowTexture;
		private static Texture2D _shaderTexture;

		public static PlayerLayer GlowLayer = CreateGlowLayer("AndromedonLegsGlow", PlayerLayer.Head, drawInfo =>
		{
			_glowTexture = _glowTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Glow");
			return GetLegDrawDataInfo(drawInfo, _glowTexture);
		});

		public static PlayerLayer ShaderLayer = CreateShaderLayer("AndromedonLegsShader", PlayerLayer.Body, drawInfo =>
		{
			_shaderTexture = _shaderTexture ?? ModLoaderMod.ReadTexture($"Developer.PowerRanger_Legs_Legs_Shader");
			return GetLegDrawDataInfo(drawInfo, _shaderTexture);
		});
	}
}
