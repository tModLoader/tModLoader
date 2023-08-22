using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class xAqult_Head : PatreonItem
    {
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 22;
			Item.height = 10;
		}
    }

	[AutoloadEquip(EquipType.Body)]
	internal class xAqult_Body : PatreonItem
	{
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 34;
			Item.height = 26;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class xAqult_Legs : PatreonItem
	{
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 22;
			Item.height = 18;
		}
	}

	[AutoloadEquip(EquipType.Head)]
	internal class xAqult_Mask : PatreonItem
	{
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		/*public override void Load() {
			if (Main.netMode == NetmodeID.Server) {
				return;
			}

			EquipLoader.AddEquipTexture(Mod, $"{Texture}_Scar_{EquipType.Head}", EquipType.Head, name: $"{Name}_Scar");
		}*/

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 30;
			Item.height = 34;
		}
	}

	[AutoloadEquip(EquipType.Face)]
	internal class xAqult_Lens : PatreonItem
	{
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void Load() {
			if (Main.netMode == NetmodeID.Server) {
				return;
			}

			EquipLoader.AddEquipTexture(Mod, $"{Texture}_Blue_{EquipType.Face}", EquipType.Face, name: $"{Name}_Blue");
		}

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			if (Main.netMode == NetmodeID.Server) {
				return;
			}

			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[Item.faceSlot] = true;
			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[EquipLoader.GetEquipSlot(Mod, "xAqult_Lens_Blue", EquipType.Face)] = true;
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.width = 18;
			Item.height = 20;
			Item.accessory = true;
		}
	}

	[AutoloadEquip(EquipType.Wings)]
	internal class xAqult_Wings : PatreonItem
	{
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
		}

		public override void SetDefaults() {
			base.SetDefaults();

			Item.vanity = false;
			Item.width = 28;
			Item.height = 52;
			Item.accessory = true;
		}
	}

	internal class xAqultPlayer : ModPlayer
	{
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
			if (Player.head == EquipLoader.GetEquipSlot(Mod, "xAqult_Mask", EquipType.Head)) {
				if (Player.face < 0) {
					drawInfo.drawPlayer.face = EquipLoader.GetEquipSlot(Mod, "xAqult_Lens", EquipType.Face);
				}
				/*if (Player.direction == 1) {
					drawInfo.drawPlayer.head = EquipLoader.GetEquipSlot(Mod, "xAqult_Mask_Scar", EquipType.Head);
				}*/
			}
			if (Player.face == EquipLoader.GetEquipSlot(Mod, "xAqult_Lens", EquipType.Face) && Player.direction == -1) {
				drawInfo.drawPlayer.face = EquipLoader.GetEquipSlot(Mod, "xAqult_Lens_Blue", EquipType.Face);
			}
		}
	}

	internal class xAqultFaceLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() {
			return new AfterParent(PlayerDrawLayers.Head);
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.face == EquipLoader.GetEquipSlot(Mod, "xAqult_Lens", EquipType.Face)
				|| drawInfo.drawPlayer.face == EquipLoader.GetEquipSlot(Mod, "xAqult_Lens_Blue", EquipType.Face);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			int insertIndex = -1;
			for (int k = 0; k < drawInfo.DrawDataCache.Count; k++) {
				if (drawInfo.DrawDataCache[k].texture == TextureAssets.Players[drawInfo.skinVar, 2].Value) {
					insertIndex = k + 1;
					break;
				}
			}
			if (insertIndex < 0) {
				return;
			}
			for (int k = insertIndex; k < drawInfo.DrawDataCache.Count; k++) {
				DrawData data = drawInfo.DrawDataCache[k];
				if (data.texture == ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Lens_Face", AssetRequestMode.ImmediateLoad).Value) {
					drawInfo.DrawDataCache.RemoveAt(k);
					drawInfo.DrawDataCache.Insert(insertIndex, data);
					data.texture = ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Lens_Face_Glow", AssetRequestMode.ImmediateLoad).Value;
					data.color = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
					drawInfo.DrawDataCache.Insert(insertIndex + 1, data);
					break;
				}
				if (data.texture == ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Lens_Blue_Face", AssetRequestMode.ImmediateLoad).Value) {
					drawInfo.DrawDataCache.RemoveAt(k);
					drawInfo.DrawDataCache.Insert(insertIndex, data);
					data.texture = ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Lens_Blue_Face_Glow", AssetRequestMode.ImmediateLoad).Value;
					data.color = drawInfo.drawPlayer.GetImmuneAlphaPure(Color.White, drawInfo.shadow);
					drawInfo.DrawDataCache.Insert(insertIndex + 1, data);
					break;
				}
			}
		}
	}

	internal class xAqultWingLayer : PlayerDrawLayer
    {
		public override Position GetDefaultPosition() {
			return new AfterParent(PlayerDrawLayers.Wings);
        }

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "xAqult_Wings", EquipType.Wings);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			if (drawInfo.drawPlayer.dead) {
				return;
            }
			DrawData? wingData = null;
			foreach (DrawData data in drawInfo.DrawDataCache) {
				if (data.texture == ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Wings_Wings", AssetRequestMode.ImmediateLoad).Value) {
					wingData = data;
                }
            }
			if (wingData.HasValue) {
				DrawData glow = new DrawData(
					texture: ModContent.Request<Texture2D>("ModLoader/Patreon.xAqult_Wings_Wings_Glow", AssetRequestMode.ImmediateLoad).Value,
					color: Color.White * drawInfo.stealth * (1f - drawInfo.shadow),
					position: wingData.Value.position,
					sourceRect: wingData.Value.sourceRect,
					rotation: wingData.Value.rotation,
					origin: wingData.Value.origin,
					scale: wingData.Value.scale,
					effect: wingData.Value.effect,
					inactiveLayerDepth: 0
				);
				glow.shader = wingData.Value.shader;
				drawInfo.DrawDataCache.Add(glow);
			}
        }
    }
}
