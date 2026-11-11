using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
    [AutoloadEquip(EquipType.Head)]
    internal class Mayne_Head : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 28;
			Item.height = 18;
        }
    }

    [AutoloadEquip(EquipType.Body)]
	internal class Mayne_Body : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 26;
			Item.height = 26;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
	internal class Mayne_Legs : PatreonItem
    {
		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 20;
			Item.height = 16;
        }
    }

    [AutoloadEquip(EquipType.Wings)]
	internal class Mayne_Wings : PatreonItem
    {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();

			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
		}

        public override void SetDefaults() {
            base.SetDefaults();

			Item.vanity = false;
			Item.width = 34;
			Item.height = 20;
			Item.accessory = true;
        }
	}

	internal class MayneWingLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() {
			return new AfterParent(PlayerDrawLayers.Wings);
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			return drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "Mayne_Wings", EquipType.Wings);
		}

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			if (drawInfo.drawPlayer.dead) {
				return;
			}
			DrawData? wingData = null;
			foreach (DrawData data in drawInfo.DrawDataCache) {
				if (data.texture == ModContent.Request<Texture2D>("ModLoader/Patreon.Mayne_Wings_Wings", AssetRequestMode.ImmediateLoad).Value) {
					wingData = data;
				}
			}
			if (wingData.HasValue) {
				DrawData glow = new DrawData(
					texture: ModContent.Request<Texture2D>("ModLoader/Patreon.Mayne_Wings_Wings_Glow", AssetRequestMode.ImmediateLoad).Value,
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
