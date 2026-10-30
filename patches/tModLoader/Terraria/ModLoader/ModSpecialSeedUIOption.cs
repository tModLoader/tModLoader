using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

	public ModSpecialSeed Parent { get; }

	public ref bool ToggleOnClick => ref _toggleOnClick;
	private bool _toggleOnClick = true;

	public ModSpecialSeedUIOption(IEnumerable<string> specialSeedNames, IEnumerable<int> specialSeedNumbers, ModSpecialSeed parent, LocalizedText description, LocalizedText title, Asset<Texture2D> texture)
	{
		SpecialSeedNames = specialSeedNames.ToArray();
		for (int i = 0; i < SpecialSeedNames.Length; i++) {
			SpecialSeedNames[i] = Regex.Replace(SpecialSeedNames[i].ToLower(), "[^a-z0-9]+", "");
		}
		SpecialSeedValues = specialSeedNumbers.ToArray();
		Description = description;
		Title = title;
		Texture = texture;
		Parent = parent;
		OnOptionStateChanged += UpdateOptionState;
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