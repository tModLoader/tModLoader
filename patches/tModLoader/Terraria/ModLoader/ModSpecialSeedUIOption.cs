using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

internal class ModSpecialSeedUIOption : AWorldGenerationOption
{
	protected override string KeyName { get; }
	public override string ServerConfigName { get; }

	public event EventHandler OnUIButtonPress;
	public event Action<AWorldGenerationOption> OnAnyOptionStateChange;

	public string ParentName { get; }

	public ref bool ToggleOnClick => ref _toggleOnClick;
	private bool _toggleOnClick = true;

	public (AWorldGenerationOption target, bool after) Ordering { get; internal set; }

	public ModSpecialSeedUIOption(IEnumerable<string> specialSeedNames, IEnumerable<int> specialSeedNumbers, string parentName, LocalizedText description, LocalizedText title, Asset<Texture2D> texture)
	{
		SpecialSeedNames = specialSeedNames.ToArray();
		SpecialSeedValues = specialSeedNumbers.ToArray();
		Description = description;
		Title = title;
		Texture = texture;
		ParentName = parentName;
		AWorldGenerationOption.OnOptionStateChanged += UpdateOptionState;
	}

	public void OnClick()
	{
		OnUIButtonPress?.Invoke(this, EventArgs.Empty);
	}

	private void UpdateOptionState(AWorldGenerationOption changed)
	{
		if(OnAnyOptionStateChange != null)
			OnAnyOptionStateChange(changed);
	}

	public void Unsubscribe()
	{
		OnOptionStateChanged -= UpdateOptionState;
	}
}