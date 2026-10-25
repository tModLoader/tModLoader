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
	public virtual LocalizedText DisplayName => Language.GetOrRegister($"Mods.{Mod.Name}.Seeds.{Name}.{nameof(DisplayName)}", PrettyPrintName);

	public virtual string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

	/// <summary>
	/// The menu that will be used while a world with this seed is being generated
	/// </summary>
	public virtual ModMenu WorldGenMenu => null;

	/// <summary>
	/// Is invoked when multiple ModSpecialSeeds with their own menus are together in a generating world, and the game needs to pick a WorldGenMenu to use.
	/// Analogously, if WorldGenMenus were competing in a wrestling match, this would be how likely the WorldGenMenu should win within its weight class.
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
	public bool ToggleOnClick { get; protected set; } = true;

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
		_uIOption.ToggleOnClick = ToggleOnClick;
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
			throw new Exception($"Seed {FullName} had a seed ({(dependencies[sharedIndex] is ModSpecialSeedUIOption specialOption ? specialOption.Parent.FullName : "")}) that is both a dependency and an incompatibility.");
		}
		Dependencies = dependencies;
		Incompatibilities = incompatibilities;
	}

	public List<AWorldGenerationOption> Dependencies { get; private set; }
	public List<AWorldGenerationOption> Incompatibilities { get; private set; }

	private Asset<Texture2D> textureAsset;

	private void SetupWorldGenerationOption()
	{
		_uIOption = new ModSpecialSeedUIOption(SpecialSeedNames(), SpecialSeedNumbers(), this, Description, DisplayName, textureAsset);
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
	/// Returns the seed's default position in regard to vanilla's seed ordering. Make use of e.g. <see cref="Before"/>/<see cref="After"/>, and provide a seed UI option (for example <c>new After(WorldGenerationOptions.Get&lt;WorldSeedOption_NotTheBees&gt;());</c>). You can also use <see cref="BeforeFirstVanillaSeed"/> or <see cref="AfterLastVanillaSeed"/> to put your seed at the start/end of the vanilla seed order.<br/><br/>
	/// <b>NOTE:</b> The position must specify a vanilla <see cref="AWorldGenerationOption"/> otherwise an exception will be thrown. Use <see cref="GetModdedConstraints"/> to order modded seeds.<br/><br/>
	/// By default, this hook positions this special seed after all vanilla seeds.
	/// </summary>
	public virtual Position GetDefaultPosition() => AfterLastVanillaSeed;

	/// <summary>
	/// Modded special seeds are placed between vanilla seeds via <see cref="GetDefaultPosition"/> and, by default, are sorted in load order.<br/>
	/// This hook allows you to sort this special seed before/after other modded seeds that were placed between the same two vanilla seeds.<br/>
	/// Example:
	/// <para>
	/// <c>yield return new After(ModContent.GetInstance&lt;ExampleSpecialSeed&gt;().UIOption);</c>
	/// </para>
	/// By default, this hook returns <see langword="null"/>, which indicates that this special seed has no modded ordering constraints.
	/// </summary>
	public virtual IEnumerable<Position> GetModdedConstraints() => null;
	#endregion

	#region Sort Positions

	public abstract class Position;

	public static Position BeforeFirstVanillaSeed => new Before(SeedLoader.FirstVanillaSeed);

	public static Position AfterLastVanillaSeed => new After(SeedLoader.LastVanillaSeed);

	public sealed class Before : Position
	{
		public AWorldGenerationOption Option { get; }

		public Before(AWorldGenerationOption option)
		{
			Option = option;
		}
	}

	public sealed class After : Position
	{
		public AWorldGenerationOption Option { get; }

		public After(AWorldGenerationOption option)
		{
			Option = option;
		}
	}
	#endregion
}