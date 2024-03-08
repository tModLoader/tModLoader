using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal class PrefixDefinitionElement : DefinitionElement<PrefixDefinition>
{
	protected override DefinitionOptionElement<PrefixDefinition> CreateDefinitionOptionElement() => new PrefixDefinitionOptionElement(Value, .8f);

	protected override void TweakDefinitionOptionElement(DefinitionOptionElement<PrefixDefinition> optionElement)
	{
		optionElement.Top.Set(0f, 0f);
		optionElement.Left.Set(-124, 1f);
	}

	protected override List<DefinitionOptionElement<PrefixDefinition>> CreateDefinitionOptionElementList()
	{
		OptionScale = 0.8f;

		var options = new List<DefinitionOptionElement<PrefixDefinition>>();

		for (int i = 0; i < PrefixLoader.PrefixCount; i++) {
			PrefixDefinitionOptionElement optionElement;

			if (i == 0)
				optionElement = new PrefixDefinitionOptionElement(new PrefixDefinition("Terraria", "None"), OptionScale);
			else
				optionElement = new PrefixDefinitionOptionElement(new PrefixDefinition(i), OptionScale);

			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};

			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<PrefixDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<PrefixDefinition>>();

		foreach (var option in Options) {
			if (option.Definition.DisplayName.IndexOf(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			string modname = option.Definition.Mod;

			if (option.Type >= PrefixID.Count)
				modname = PrefixLoader.GetPrefix(option.Type).Mod.DisplayNameClean; // or internal name?

			if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}

		return passed;
	}
}

internal class PrefixDefinitionOptionElement : DefinitionOptionElement<PrefixDefinition>
{
	private readonly UIAutoScaleTextTextPanel<string> text;

	public PrefixDefinitionOptionElement(PrefixDefinition definition, float scale = .75f) : base(definition, scale)
	{
		Width.Set(150 * scale, 0f);
		Height.Set(40 * scale, 0f);

		text = new UIAutoScaleTextTextPanel<string>(Definition.DisplayName) {
			Width = { Percent = 1f },
			Height = { Percent = 1f },
		};
		Append(text);
	}

	public override void SetItem(PrefixDefinition item)
	{
		base.SetItem(item);

		text?.SetText(item.DisplayName);
	}

	public override void SetScale(float scale)
	{
		base.SetScale(scale);

		Width.Set(150 * scale, 0f);
		Height.Set(40 * scale, 0f);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}
