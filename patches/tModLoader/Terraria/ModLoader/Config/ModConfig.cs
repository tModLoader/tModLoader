using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config;

// TODO: Enforce no statics allowed.

/// <summary>
/// ModConfig provides a way for mods to be configurable. ModConfigs can either be Client specific or Server specific.
/// When joining a MP server, Client configs are kept but Server configs are synced from the server.
/// Using serialization attributes such as [DefaultValue(5)] or [JsonIgnore] are critical for proper usage of ModConfig.
/// tModLoader also provides its own attributes such as ReloadRequiredAttribute and LabelAttribute.
/// </summary>
public abstract class ModConfig : ILocalizedModType
{
	[JsonIgnore]
	public Mod Mod { get; internal set; }

	[JsonIgnore]
	public string Name { get; internal set; }

	[JsonIgnore]
	public string FullName => $"{Mod.Name}/{Name}";

	[JsonIgnore]
	public virtual string LocalizationCategory => "Configs";

	[JsonIgnore]
	public virtual LocalizedText DisplayName => Language.GetOrRegister(this.GetLocalizationKey(nameof(DisplayName)), () => ConfigManager.GetLegacyLabelAttribute(GetType())?.LocalizationEntry ?? Regex.Replace(Name, "([A-Z])", " $1").Trim());

	[JsonIgnore]
	public abstract ConfigScope Mode { get; }

	// TODO: Does non-autoloaded ModConfigs have a use-case?
	public virtual bool Autoload(ref string name) => Mod.ContentAutoloadingEnabled;

	/// <summary>
	/// This method is called when the ModConfig has been loaded for the first time. This happens before regular Autoloading and Mod.Load. You can use this hook to assign a static reference to this instance for easy access.
	/// tModLoader will automatically assign (and later unload) this instance to a static field named Instance in the class prior to calling this method, if it exists.
	/// </summary>
	public virtual void OnLoaded() { }

	/// <summary>
	/// This hook is called anytime new config values have been set and are ready to take effect. This will always be called right after OnLoaded and anytime new configuration values are ready to be used. The hook won't be called with values that violate NeedsReload. Use this hook to integrate with other code in your Mod to apply the effects of the configuration values. If your NeedsReload is correctly implemented, you should be able to apply the settings without error in this hook. Be aware that this hook can be called in-game and in the main menu, as well as in single player and multiplayer situations.
	/// </summary>
	public virtual void OnChanged() { }

	/// <inheritdoc cref="AcceptClientChanges(ModConfig, int, ref NetworkText)"/>
	[Obsolete("Use the updated hook signature")]
	public virtual bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		=> true;

	/// <summary>
	/// Called on the Server for ServerSide configs to determine if the changes asked for by the Client will be accepted. Useful for enforcing permissions. Called after a check for NeedsReload.
	/// </summary>
	/// <param name="pendingConfig">An instance of the ModConfig with the attempted changes</param>
	/// <param name="whoAmI">The client whoAmI</param>
	/// <param name="message">A message that will be returned to the client, set this to the reason the server rejects the changes.</param>
	/// <returns>Return false to reject client changes</returns>
	public virtual bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		=> true;

	// TODO: Can we get rid of Clone and just load from disk? Don't think so yet.
	/// <summary>
	/// tModLoader will call Clone on ModConfig to facilitate proper implementation of the ModConfig user interface and detecting when a reload is required. Modders need to override this method if their config contains reference types. Failure to do so will lead to bugs. See ModConfigShowcaseDataTypes.Clone for examples and explanations.
	/// </summary>
	/// <returns></returns>
	public virtual ModConfig Clone() => (ModConfig)MemberwiseClone();

	/// <summary>
	/// Whether or not a reload is required. The default implementation compares properties and fields annotated with the ReloadRequiredAttribute. Unlike the other ModConfig hooks, this method is called on a clone of the ModConfig that was saved during mod loading. The pendingConfig has values that are about to take effect. Neither of these instances necessarily match the instance used in OnLoaded.
	/// </summary>
	/// <param name="pendingConfig">The other instance of ModConfig to compare against, it contains the values that are pending to take effect</param>
	/// <returns></returns>
	public virtual bool NeedsReload(ModConfig pendingConfig)
	{
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(this)) {
			var reloadRequired = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ReloadRequiredAttribute>(variable, this, null);

			if (reloadRequired == null) {
				continue;
			}

			// Do we need to implement nested ReloadRequired? Right now only top level fields will trigger it.
			if (!ConfigManager.ObjectEquals(variable.GetValue(this), variable.GetValue(pendingConfig))) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Opens this config in the config UI.
	/// <para/> Can be used to allow your own UI to access the config.
	/// <para/> <paramref name="onClose"/> can be used to run code after the config is closed, such as opening a modded UI or showing a message to the user.
	/// <para/> <paramref name="scrollToOption"/> can be used to scroll to a specific member of the config and highlight it. It can also be used to scroll to the header above a member using the format <c>"Header:{MemberNameHere}"</c>. If the member has <c>[SeparatePage]</c> then the subpage will open automatically as well. Set <paramref name="centerScrolledOption"/> to false if you'd like the config option to be at the top of the list when focused instead of at the center.
	/// </summary>
	/// <param name="onClose">A delegate that is called when the back button is pressed to allow for custom back button behavior.</param>
	/// <param name="scrollToOption">The name of a field of the ModConfig to scroll to.</param>
	/// <param name="centerScrolledOption"></param>
	/// <param name="playSound">Whether <see cref="SoundID.MenuOpen"/> will be played when the UI is opened.</param>
	public void Open(Action onClose = null, string scrollToOption = null, bool centerScrolledOption = true, bool playSound = true)
	{ 
		if (playSound)
			SoundEngine.PlaySound(SoundID.MenuOpen);

		Interface.modConfig.SetMod(Mod, this, openedFromModder: true, onClose, scrollToOption, centerScrolledOption);

		if (Main.gameMenu) {
			Main.menuMode = Interface.modConfigID;
		}
		else {
			IngameFancyUI.CoverNextFrame();
			Main.playerInventory = false;
			Main.editChest = false;
			Main.npcChatText = "";
			Main.inFancyUI = true;
			Main.InGameUI.SetState(Interface.modConfig);
			// Same as IngameFancyUI.OpenUIState(Interface.modConfig); except no ClearChat()
		}
	}
}