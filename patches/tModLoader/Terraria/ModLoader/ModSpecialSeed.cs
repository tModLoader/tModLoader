using Terraria.Localization;
using Terraria.GameContent.UI;
using Terraria.WorldBuilding;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.UI;

namespace Terraria.ModLoader;

public abstract class ModSpecialSeed : ModTexturedType
{
	public bool Enabled = false;

	internal WorldGenerationOption WorldGenerationOption { get; private set; }

	public virtual LocalizedText DisplayName => Language.GetOrRegister($"Mods.{Mod.Name}.SpecialSeeds.{Name}.{nameof(DisplayName)}", PrettyPrintName);
	public virtual LocalizedText Description => Language.GetOrRegister($"Mods.{Mod.Name}.SpecialSeeds.{Name}.{nameof(Description)}", () => "");

	/// <summary>
	/// How the menu will look while a world with this seed is being generated
	/// </summary>
	public virtual ModMenu GenerationMenu => null;

	protected sealed override void Register()
	{
		SpecialSeedLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		textureAsset = ModContent.Request<Texture2D>(Texture);
		SetupWorldGenerationOption();
		SetStaticDefaults();
	}

	private Asset<Texture2D> textureAsset;

	public virtual IEnumerable<string> SpecialSeedNames() { return Enumerable.Empty<string>(); }
	public virtual IEnumerable<int> SpecialSeedNumbers() { return Enumerable.Empty<int>(); }

	private void SetupWorldGenerationOption()
	{
		WorldGenerationOption = new WorldGenerationOption(SpecialSeedNames(), SpecialSeedNumbers(), Description, DisplayName, textureAsset);
	}

	public virtual UIElement ProvideSeedIconElement()
	{
		return WorldGenerationOption.ProvideUIElement();
	}
}