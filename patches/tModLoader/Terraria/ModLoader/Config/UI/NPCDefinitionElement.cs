using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class NPCDefinitionElement : DefinitionElement<NPCDefinition>
{
	protected override DefinitionOptionElement<NPCDefinition> CreateDefinitionOptionElement() => new NPCDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<NPCDefinition>> CreateDefinitionOptionElementList()
	{
		OptionScale = 0.8f;
		var options = new List<DefinitionOptionElement<NPCDefinition>>();

		for (int i = 0; i < NPCLoader.NPCCount; i++) {
			var optionElement = new NPCDefinitionOptionElement(new NPCDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<NPCDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<NPCDefinition>>();

		foreach (var option in Options) {
			// Should this be the localized NPC name?
			if (Lang.GetNPCName(option.Type).Value.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			string modname = option.Definition.Mod;

			if (option.Type >= NPCID.Count) {
				modname = NPCLoader.GetNPC(option.Type).Mod.DisplayNameClean; // or internal name?
			}

			if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}

		return passed;
	}
}

internal class NPCDefinitionOptionElement : DefinitionOptionElement<NPCDefinition>
{
	public NPCDefinitionOptionElement(NPCDefinition definition, float scale = .75f) : base(definition, scale)
	{
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = base.GetInnerDimensions();

		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null) {
			int type = Unloaded ? 0 : Type;
			if (TextureAssets.Npc[type].State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(TextureAssets.Npc[type].Name, AssetRequestMode.AsyncLoad);
			Texture2D npcTexture = TextureAssets.Npc[type].Value;

			int frameCounter = Interface.modConfig.UpdateCount / 8;
			int frames = Main.npcFrameCount[type];

			if (Unloaded) {
				npcTexture = TextureAssets.Item[ModContent.ItemType<UnloadedItem>()].Value;
				frames = 1;
			}

			int height = npcTexture.Height / frames;
			int width = npcTexture.Width;
			int frame = frameCounter % frames;
			int y = height * frame;
			Rectangle rectangle2 = new Rectangle(0, y, width, height);

			float drawScale = 1f;
			float availableWidth = DefaultBackgroundTexture.Width() * Scale;

			if (width > availableWidth || height > availableWidth) {
				if (width > height) {
					drawScale = availableWidth / width;
				}
				else {
					drawScale = availableWidth / height;
				}
			}

			drawScale *= Scale;

			Vector2 vector = BackgroundTexture.Size() * Scale;
			Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
			Vector2 origin = rectangle2.Size() * 0;

			//Color color = (npc.color != new Color(byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue)) ? new Color(npc.color.R, npc.color.G, npc.color.B, 255f) : new Color(1f, 1f, 1f);

			spriteBatch.Draw(npcTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}
