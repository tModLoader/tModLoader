using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

internal class ModSpecialSeedUIOption : AWorldGenerationOption
{
	protected override string KeyName { get; }
	public override string ServerConfigName { get; }

	public event EventHandler OnEnableStateChange;
	public event Action<AWorldGenerationOption> OnAnyOptionStateChange;

	public string ParentName { get; }

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

	protected override void OnEnabledStateChanged()
	{
		OnEnableStateChange?.Invoke(this, EventArgs.Empty);
	}

	private void UpdateOptionState(AWorldGenerationOption changed)
	{
		if(OnAnyOptionStateChange != null)
			OnAnyOptionStateChange(changed);
	}
}