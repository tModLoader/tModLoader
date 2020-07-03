using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	class PrefixDefinitionElement : DefinitionElement<PrefixDefinition>
	{
		protected override DefinitionOptionElement<PrefixDefinition> CreateDefinitionOptionElement() => new PrefixDefinitionOptionElement(Value, .8f);

		protected override void TweakDefinitionOptionElement(DefinitionOptionElement<PrefixDefinition> optionElement) {
			optionElement.Top.Set(0f, 0f);
			optionElement.Left.Set(-124, 1f);
		}

		protected override List<DefinitionOptionElement<PrefixDefinition>> CreateDefinitionOptionElementList() {
			optionScale = 0.8f;
			var options = new List<DefinitionOptionElement<PrefixDefinition>>();
			for (int i = 0; i < ModPrefix.PrefixCount; i++) {
				PrefixDefinitionOptionElement optionElement;
				if(i == 0)
					optionElement = new PrefixDefinitionOptionElement(new PrefixDefinition("Terraria", "None"), optionScale);
				else
					optionElement = new PrefixDefinitionOptionElement(new PrefixDefinition(i), optionScale);
				optionElement.OnClick += (a, b) => {
					Value = optionElement.definition;
					updateNeeded = true;
					selectionExpanded = false;
				};
				options.Add(optionElement);
			}
			return options;
		}

		protected override List<DefinitionOptionElement<PrefixDefinition>> GetPassedOptionElements() {
			var passed = new List<DefinitionOptionElement<PrefixDefinition>>();
			foreach (var option in options) {
				// Should this be the localized Prefix name?
				if (Lang.prefix[option.type].Value.IndexOf(chooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				string modname = option.definition.mod;
				if (option.type > PrefixID.Count) {
					modname = ModPrefix.GetPrefix((byte)option.type).mod.DisplayName; // or internal name?
				}
				if (modname.IndexOf(chooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				passed.Add(option);
			}
			return passed;
		}
	}

	internal class PrefixDefinitionOptionElement : DefinitionOptionElement<PrefixDefinition>
	{
		UIAutoScaleTextTextPanel<string> text;
		public PrefixDefinitionOptionElement(PrefixDefinition definition, float scale = .75f) : base(definition, scale) {
			Width.Set(150 * scale, 0f);
			Height.Set(40 * scale, 0f);

			text = new UIAutoScaleTextTextPanel<string>(type == 0 ? "None" : Lang.prefix[type].Value) {
				Width = { Percent = 1f },
				Height = { Percent = 1f },
			};
			Append(text);
		}

		public override void SetItem(PrefixDefinition item) {
			base.SetItem(item);

			text?.SetText(type == 0 ? "None" : Lang.prefix[type].Value);
		}

		public override void SetScale(float scale) {
			base.SetScale(scale);

			Width.Set(150 * scale, 0f);
			Height.Set(40 * scale, 0f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (IsMouseHovering)
				UIModConfig.tooltip = tooltip;
		}
	}
}
