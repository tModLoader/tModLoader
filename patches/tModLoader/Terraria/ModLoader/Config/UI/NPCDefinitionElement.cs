using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class NPCDefinitionElement : DefinitionElement<NPCDefinition>
{
	protected override DefinitionOptionElement<NPCDefinition> CreateDefinitionOptionElement() => new NPCDefinitionOptionElement(0, Value, 0.5f);

	protected override List<DefinitionOptionElement<NPCDefinition>> CreateDefinitionOptionElementList()
	{
		OptionScale = 0.8f;
		var options = new List<DefinitionOptionElement<NPCDefinition>>();

		var npcIDsInSensibleOrder = Enumerable.Range(0, NPCID.Count).Concat(Enumerable.Range(NPCID.NegativeIDCount + 1, 65).Reverse()).Concat(Enumerable.Range(NPCID.Count, NPCLoader.NPCCount - NPCID.Count));

		int order = 0;
		foreach (int i in npcIDsInSensibleOrder) {
			var optionElement = new NPCDefinitionOptionElement(order, new NPCDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
			order++;
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
	public int Order { get; set; }

	public NPCDefinitionOptionElement(int order, NPCDefinition definition, float scale = .75f) : base(definition, scale)
	{
		this.Order = order;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = base.GetInnerDimensions();

		spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

		if (Definition != null) {
			int type = Unloaded ? 0 : NPCID.FromNetId(Type);
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

			NPC npc = ContentSamples.NpcsByNetId[Unloaded ? 0 : Type];
			spriteBatch.Draw(npcTexture, position2, rectangle2, Color.White, 0f, origin, drawScale, SpriteEffects.None, 0f);
			if (npc.color != default) {
				spriteBatch.Draw(npcTexture, position2, rectangle2, npc.GetColor(Color.White), 0f, origin, drawScale, SpriteEffects.None, 0f);
			}
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}

	public override int CompareTo(object obj)
	{
		if (obj is NPCDefinitionOptionElement other) {
			if (Order == 0)
				return -1;
			if (other.Order == 0)
				return 1;

			bool hasSort = ContentSamples.NpcBestiarySortingId.TryGetValue(Type, out int sortValue);
			bool otherHasSort = ContentSamples.NpcBestiarySortingId.TryGetValue(other.Type, out int otherSortValue);

			if (hasSort && otherHasSort)
				return sortValue.CompareTo(otherSortValue);

			if (hasSort)
				return -1;

			if (otherHasSort)
				return 1;

			return Order.CompareTo(other.Order);
		}

		return base.CompareTo(obj);
	}
}
