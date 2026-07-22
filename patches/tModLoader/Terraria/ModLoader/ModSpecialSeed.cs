using Terraria.Localization;
using Terraria.WorldBuilding;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.IO;
using Terraria.UI;

namespace Terraria.ModLoader;

/// <summary>
/// This type of class represents a special seed added by a mod. Special seeds can be used to change gameplay and/or worldgen only for worlds that have them enabled.<br/>
/// You can check if a ModSpecialSeed is enabled for a particular world by using <see cref="SeedLoader.SeedEnabled"/>.
/// <br/><br/>Unlike secret seeds, special seeds have custom icons and appear prominently in the seeds menu.
/// </summary>
public abstract class ModSpecialSeed : ModSeedType
{
	private ModSpecialSeedUIOption _uIOption;

	/// <summary>
	/// The underlying data used to create and manage the option button for this seed in the menu.
	/// <br/><br/>Note that UIOption.Enabled only states if the button is enabled in the menu.
	/// Do NOT use it to check if a seed is enabled in a world.
	/// </summary>
	public AWorldGenerationOption UIOption => _uIOption;

	/// <summary>
	/// The translation for the display name of this special seed
	/// </summary>
	public virtual LocalizedText DisplayName => Language.GetOrRegister($"Mods.{Mod.Name}.SpecialSeeds.{Name}.{nameof(DisplayName)}", PrettyPrintName);

	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

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

	/// <summary>
	/// Whether this seed should be marked as a dependency of the Zenith (also known as Everything) seed.
	/// </summary>
	public bool IncludeInZenith {
		get => WorldGenerationOptions.Get<WorldSeedOption_Everything>().Dependencies.Contains(_uIOption);
		set {
			if (value && !WorldGenerationOptions.Get<WorldSeedOption_Everything>().Dependencies.Contains(UIOption)) {
				WorldGenerationOptions.Get<WorldSeedOption_Everything>().Dependencies.Add(_uIOption);
			}
			if (!value && WorldGenerationOptions.Get<WorldSeedOption_Everything>().Dependencies.Contains(UIOption)) {
				WorldGenerationOptions.Get<WorldSeedOption_Everything>().Dependencies.Remove(_uIOption);
			}
		}
	}

	/// <summary>
	/// Whether this seed should be automatically enabled when all of its dependencies are also enabled.
	/// True for the Zenith seed.
	/// </summary>
	public bool AutoEnableWithDependencies { get; set; }

	/// <summary>
	/// Whether this secret seed's UI option can be enabled or disabled by clicking it.<br/>
	/// Can be used with <see cref="OnSeedButtonPress"/> to have whether this seed's UI option is enabled be dependent on custom logic.
	/// <br/><br/>Defaults to true.
	/// </summary>
	public ref bool ToggleOnClick => ref _uIOption.ToggleOnClick;

	#region Sorting

	// Use the variable in ModSpecialSeedUIOption in order to catch ordering loops.
	public (AWorldGenerationOption target, bool after) Ordering { get => _uIOption.Ordering; internal set => _uIOption.Ordering = value; }

	/// <inheritdoc cref="SortBefore(AWorldGenerationOption)"/>
	public void SortBeforeVanillaSeed<T>() where T : AWorldGenerationOption
	{
		SortBefore(WorldGenerationOptions.Get<T>());
	}

	/// <inheritdoc cref="SortBefore(AWorldGenerationOption)"/>
	public void SortBeforeModdedSeed<T>() where T : ModSpecialSeed
	{
		SortBefore(ModContent.GetInstance<T>().UIOption);
	}

	/// <inheritdoc cref="SortBefore(AWorldGenerationOption)"/>
	/// <param name="option">The ModSpecialSeed instance to sort this seed before.</param>
	public void SortBefore(ModSpecialSeed option)
	{
		SortBefore(option.UIOption);
	}

	/// <summary>
	/// Allows you to specify the seed that this seed's option should be placed before.
	/// <br/><br/>Note that this only affects the ordering of the seed buttons in the seed menu, not the seed's actual loading priority.
	/// </summary>
	public void SortBefore(AWorldGenerationOption option)
	{
		SetOrdering(option,false);
	}

	/// <inheritdoc cref="SortAfter(AWorldGenerationOption)"/>
	public void SortAfterVanillaSeed<T>() where T : AWorldGenerationOption
	{
		SortAfter(WorldGenerationOptions.Get<T>());
	}

	/// <inheritdoc cref="SortAfter(AWorldGenerationOption)"/>
	public void SortAfterModdedSeed<T>() where T : ModSpecialSeed
	{
		SortAfter(ModContent.GetInstance<T>().UIOption);
	}

	/// <inheritdoc cref="SortAfter(AWorldGenerationOption)"/>
	/// <param name="option">The ModSpecialSeed instance to sort this seed after.</param>
	public void SortAfter(ModSpecialSeed option)
	{
		SortAfter(option.UIOption);
	}

	/// <summary>
	/// Allows you to specify the seed that this seed's option should be placed after.
	/// <br/><br/>Note that this only affects the ordering of the seed buttons in the seed menu, not the seed's actual loading priority.
	/// </summary>
	public void SortAfter(AWorldGenerationOption option)
	{
		SetOrdering(option, true);
	}

	private void SetOrdering(AWorldGenerationOption target, bool after)
	{
		Ordering = (target, after);
		ModSpecialSeedUIOption moddedTarget = null;
		if (target is ModSpecialSeedUIOption castTarget) {
			moddedTarget = castTarget;
		}
		if (moddedTarget == null) {
			return;
		}
		if (moddedTarget.Ordering.target == UIOption) {
			throw new Exception("Special seed ordering loop!");
		}
	}

	#endregion

	/// <summary>
	/// Gets the icon used to represent worlds with this seed enabled.
	/// Unlike the autoloaded texture, the texture used here needs to have the icon backdrop behind it.
	/// </summary>
	public abstract Asset<Texture2D> GetWorldIconTexture();

	protected sealed override void Register()
	{
		ModTypeLookup<ModSpecialSeed>.Register(this);
		SeedLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		textureAsset = ModContent.Request<Texture2D>(Texture);
		SetupWorldGenerationOption();
		SetStaticDefaults();
	}

	internal void Unsubscribe()
	{
		_uIOption.OnUIButtonPress -= OnUIButtonPress;
		_uIOption.OnAnyOptionStateChange -= OnAnyOptionStateChange;
		_uIOption.Unsubscribe();
	}

	internal void FinalizeContent()
	{
		List<AWorldGenerationOption> dependencies = GetDependencies().ToList();
		List<AWorldGenerationOption> incompatibilities = GetIncompatibilities().ToList();
		int sharedIndex = dependencies.FindIndex((dependent) => incompatibilities.Contains(dependent));
		if (sharedIndex != -1) {
			throw new Exception($"Seed {FullName} had a seed ({(dependencies[sharedIndex] is ModSpecialSeedUIOption specialOption ? specialOption.ParentName : "")}) that is both a dependency and an incompatibility.");
		}
		Dependencies = dependencies;
		Incompatibilities = incompatibilities;
		PostSetupContent();
	}

	public List<AWorldGenerationOption> Dependencies { get; private set; }
	public List<AWorldGenerationOption> Incompatibilities { get; private set; }

	private Asset<Texture2D> textureAsset;

	private void SetupWorldGenerationOption()
	{
		_uIOption = new ModSpecialSeedUIOption(SpecialSeedNames(), SpecialSeedNumbers(), FullName, Description, DisplayName, textureAsset);
		_uIOption.OnUIButtonPress += OnUIButtonPress;
		_uIOption.OnAnyOptionStateChange += OnAnyOptionStateChange;
	}

	private void OnUIButtonPress(object sender, EventArgs e)
	{
		OnSeedButtonPress();
	}

	private void ChangeDependencyState()
	{
		if (!UIOption.Enabled && Dependencies.Any((option) => !option.Enabled)) {
			return;
		}

		foreach (AWorldGenerationOption option in Dependencies) {
			option.Enabled = UIOption.Enabled;
		}
	}

	private void ChangeIncompatibilityState()
	{
		if (!UIOption.Enabled) {
			return;
		}

		foreach (AWorldGenerationOption option in Incompatibilities) {
			option.Enabled = false;
		}
	}

	private void OnAnyOptionStateChange(AWorldGenerationOption changed)
	{
		if (changed == UIOption) {
			OnChangeOptionEnabled();
			ChangeDependencyState();
			ChangeIncompatibilityState();
		}
		UpdateDependencies(changed);
		if (AutoEnableWithDependencies && Dependencies.All(dependent => dependent.Enabled)) {
			UIOption.Enabled = true;
		}
		UpdateIncompatibilities(changed);
	}

	private void UpdateDependencies(AWorldGenerationOption changed)
	{
		AWorldGenerationOption dependency = Dependencies.Find((option) => option == changed);
		if (dependency == null) {
			return;
		}

		if (!dependency.Enabled && UIOption.Enabled) {
			UIOption.Enabled = false;
		}
	}

	private void UpdateIncompatibilities(AWorldGenerationOption changed)
	{
		AWorldGenerationOption incompatibility = Incompatibilities.Find((option) => option == changed);
		if (incompatibility == null) {
			return;
		}
		if(incompatibility.Enabled && UIOption.Enabled) {
			UIOption.Enabled = false;
		}
	}

	public UIElement ProvideSeedIconElement()
	{
		var element = UIOption.ProvideUIElement();
		ModifySeedMenuElement(element);
		return element;
	}

	protected static AWorldGenerationOption GetModdedSeedOption<T>() where T : ModSpecialSeed
	{
		return ModContent.GetInstance<T>().UIOption;
	}

	#region Hooks
	/// <summary>
	/// This allows changing the icon for this seed's toggle in the world creation menu.
	/// </summary>
	/// <param name="element">The UI element that is used for the toggle</param>
	public virtual void ModifySeedMenuElement(UIElement element) { }

	/// <summary>
	/// Allows you to add custom seed names that will trigger your special seed when entered into the seed menu.
	/// <br/><br/>Any seed name you add will automatically be formatted to be all lowercase and to have spaces and special characters removed.
	/// <br/><br/>Called at load time.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<string> SpecialSeedNames() { return Enumerable.Empty<string>(); }
	/// <summary>
	/// Allows you to add custom seed numbers that will trigger your special seed when entered into the seed menu.
	/// <br/><br/>Called at load time.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerable<int> SpecialSeedNumbers() { return Enumerable.Empty<int>(); }


	/// <summary>
	/// Allows you to make things happen when the button for this option is pressed.
	/// </summary>
	public virtual void OnSeedButtonPress() { }

	/// <summary>
	/// Called whenever the Enabled property of this ModSpecialSeed's UIOption changes value.<br/>
	/// This differs from <see cref="OnSeedButtonPress"/> in that it can also be triggered when something else causes the Enabled property to change,<br/>
	/// e.g. when another seed enables this one as one of its dependencies.
	/// </summary>
	public virtual void OnChangeOptionEnabled() { }

	/// <summary>
	/// Used in conjunction with <see cref="GetModdedSeedOption"/> and <see cref="WorldGenerationOptions.Get"/> to mark seeds that will be enabled when this seed is enabled.
	/// <br/><br/>Called during load time after content has been set up.
	/// </summary>
	public virtual IEnumerable<AWorldGenerationOption> GetDependencies() { return Enumerable.Empty<AWorldGenerationOption>(); }

	/// <summary>
	/// Used in conjunction with <see cref="GetModdedSeedOption"/> and <see cref="WorldGenerationOptions.Get"/> to mark seeds that are disabled when this seed is enabled.
	/// <br/><br/>Called during load time after content has been set up.
	/// </summary>
	public virtual IEnumerable<AWorldGenerationOption> GetIncompatibilities() { return Enumerable.Empty<AWorldGenerationOption>(); }

	/// <summary>
	/// Used to modify the way the icon is displayed for worlds with this seed enabled.
	/// </summary>
	/// <param name="isCrimson">True if this is a Crimson world, false if it is a Corruption world.</param>
	/// <param name="isHardmode">True if this is in Hardmode, false if it is in Pre-Hardmode</param>
	/// <param name="frame">The frame of the texture being displayed. Will use the entire texture if not specified.</param>
	public virtual void ModifyWorldIconDrawParams(bool isCrimson, bool isHardmode, ref Rectangle frame) { }

	/// <summary>
	/// Allows you to run code after the mod's content has been setup.<br/>
	/// Use <see cref="SortBefore(AWorldGenerationOption)"/> and <see cref="SortAfter(AWorldGenerationOption)"/> in this hook to implement proper seed button sorting after all the mod's seeds have been loaded.
	/// </summary>
	public virtual void PostSetupContent() { }
	#endregion
}