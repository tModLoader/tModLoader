using Terraria.Localization;
using Terraria.GameContent.UI;
using Terraria.WorldBuilding;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.IO;
using Terraria.UI;

namespace Terraria.ModLoader;

public abstract class ModSpecialSeed : ModTexturedType
{
	internal bool Enabled { get; set; } = false;

	internal WorldGenerationOption WorldGenerationOption { get; private set; }

	/// <summary>
	/// The translation for the display name of this special seed
	/// </summary>
	public virtual LocalizedText DisplayName => Language.GetOrRegister($"Mods.{Mod.Name}.SpecialSeeds.{Name}.{nameof(DisplayName)}", PrettyPrintName);

	/// <summary>
	/// The translation for the description used for this special seed
	/// </summary>
	public virtual LocalizedText Description => Language.GetOrRegister($"Mods.{Mod.Name}.SpecialSeeds.{Name}.{nameof(Description)}", () => "");

	/// <summary>
	/// The menu that will be used while a world with this seed is being generated
	/// </summary>
	public virtual ModMenu WorldGenMenu => null;

	/// <summary>
	/// Is invoked when multiple ModSpecialSeeds with their own menus are together in a generating world, and the game needs to pick a WorldGenMenu to use.
	/// Analogously, if WorldGenMenus were competing in a wrestling match, this would be how likely the WorldGenMenu should win within its weight class.
	/// Is intentionally bounded at a max of 100% (1) to reduce complexity. Defaults to 50% (0.5).
	/// </summary>
	public virtual float GetMenuWeight() => 0.5f;

	internal Asset<Texture2D> GetSeedIcon(WorldFileData data)
	{
		string text = "";
		text += (data.IsHardMode ? "Hallow" : "");
		text += (data.HasCorruption ? "Corruption" : "Crimson");
		return worldIcons[text];
	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModSpecialSeed>.Register(this);
		SpecialSeedLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		textureAsset = ModContent.Request<Texture2D>(Texture);
		worldIcons["Corruption"] = IconCorruption;
		worldIcons["Crimson"] = IconCrimson;
		worldIcons["HallowCorruption"] = IconHallowCorruption;
		worldIcons["HallowCrimson"] = IconHallowCrimson;
		SetupWorldGenerationOption();
		SetStaticDefaults();
	}

	private Asset<Texture2D> textureAsset;
	private Dictionary<string, Asset<Texture2D>> worldIcons = new();

	public abstract Asset<Texture2D> IconHallowCorruption { get; }
	public abstract Asset<Texture2D> IconHallowCrimson { get; }
	public abstract Asset<Texture2D> IconCorruption { get; }
	public abstract Asset<Texture2D> IconCrimson { get; }

	private void SetupWorldGenerationOption()
	{
		WorldGenerationOption = new WorldGenerationOption(SpecialSeedNames(), SpecialSeedNumbers(), Description, DisplayName, textureAsset);
	}

	public UIElement ProvideSeedIconElement()
	{
		var element = WorldGenerationOption.ProvideUIElement();
		ModifySeedIconElement(element);
		return element;
	}

	/// <summary>
	/// This allows changing the special seed toggle for this seed used in the world creation menu.
	/// </summary>
	/// <param name="element">The UI element that is used for the toggle</param>
	public virtual void ModifySeedIconElement(UIElement element) { }

	/// <summary>
	/// Allows you to add custom seed names that will trigger your special seed when entered into the seed menu.
	/// Any seed name you add will automatically be formatted to be all lowercase and to have spaces and special characters removed.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<string> SpecialSeedNames() { return Enumerable.Empty<string>(); }
	/// <summary>
	/// Allows you to add custom seed numbers that will trigger your special seed when entered into the seed menu.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<int> SpecialSeedNumbers() { return Enumerable.Empty<int>(); }

	/// <summary>
	/// <inheritdoc cref="ModSystem.ModifyWorldGenTasks"/>
	/// This only applies for worlds with this seed enabled. It is called before ModSystem.ModifyWorldGenTasks.
	/// </summary>
	public virtual void ModifyWorldGenTasks(List<GenPass> tasks) { }
}