using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
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

	/// <summary>
	/// Called on the Server for ServerSide configs to determine if the changes asked for by the Client will be accepted. Useful for enforcing permissions. Called after a check for NeedsReload.
	/// </summary>
	/// <param name="pendingConfig">An instance of the ModConfig with the attempted changes</param>
	/// <param name="whoAmI">The client whoAmI</param>
	/// <param name="message">A message that will be returned to the client, set this to the reason the server rejects the changes.<br/>
	/// Make sure you set this to the localization key instead of the actual value, since the server and client could have different languages.</param>
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
	/// <returns>Whether a reload is required.</returns>
	public virtual bool NeedsReload(ModConfig pendingConfig)
	{
		foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(this))
		{
			var reloadRequired = ConfigManager.GetCustomAttributeFromMemberThenMemberType<ReloadRequiredAttribute>(variable, this, null);

			if (reloadRequired == null)
				continue;

			// Do we need to implement nested ReloadRequired? Right now only top level fields will trigger it.
			if (!ConfigManager.ObjectEquals(variable.GetValue(this), variable.GetValue(pendingConfig)))
				return true;
		}

		return false;
	}

	/// <summary>
	/// Opens this config in the config UI.<br/>
	/// Can be used to allow your own UI to add buttons to access the config.
	/// </summary>
	public void Open()
	{
		SoundEngine.PlaySound(SoundID.MenuOpen);
		Interface.modConfig.SetMod(Mod, this, openedFromModder: true);

		if (Main.gameMenu)
		{
			Main.menuMode = Interface.modConfigID;
		}
		else
		{
			IngameFancyUI.CoverNextFrame();

			Main.playerInventory = false;
			Main.editChest = false;
			Main.npcChatText = "";
			Main.inFancyUI = true;

			Main.InGameUI.SetState(Interface.modConfig);
		}
	}

	/// <summary>
	/// Saves any changes to this config.
	/// </summary>
	/// <param name="showErrors">Whether messages in the config UI and in chat should be shown.</param>
	/// <returns>Whether the config was successfully saved.</returns>
	public bool Save(bool showErrors = true)
	{
		// TODO: finish
		// Since this can be called on a clone, we need to get the real config to load the data into
		var realConfig = ConfigManager.GetConfig(Mod, Name);// Used to load changes back into
		var loadTimeConfig = ConfigManager.GetLoadTimeConfig(Mod, Name);// Used to check if a reload is required

		// Main Menu - Save, leave reload for later
		// MP with ServerSide - Send request to server
		// SP or MP with ClientSide - Apply immediately if !NeedsReload

		// Game, client, server side config
		if (!Main.gameMenu && Mode == ConfigScope.ServerSide && Main.netMode == NetmodeID.MultiplayerClient)
		{
			if (showErrors)
				Interface.modConfig.SetMessage(Language.GetTextValue("tModLoader.ModConfigAskingServerToAcceptChanges"), Language.GetTextValue("tModLoader.ModConfigChangesPending"), Color.Yellow);

			var requestChanges = new ModPacket(MessageID.InGameChangeConfig);
			requestChanges.Write(Mod.Name);
			requestChanges.Write(Name);
			string json = JsonConvert.SerializeObject(this, ConfigManager.serializerSettingsCompact);
			requestChanges.Write(json);
			requestChanges.Send();

			return true;
		}

		// Game, singleplayer, 
		if (!Main.gameMenu && loadTimeConfig.NeedsReload(this))
		{
			if (showErrors)
				Interface.modConfig.SetMessage(Language.GetTextValue("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload"), Language.GetTextValue("tModLoader.ModConfigChangesRejected"), Color.Red);

			return false;
		}

		// Menu or singleplayer
		ConfigManager.Save(this);
		ConfigManager.Load(realConfig);

		// ModConfig.OnChanged() delayed until ReloadRequired checked
		// Reload will be forced by back button in UIMods if needed
		if (!Main.gameMenu)
			OnChanged();

		return true;
	}
}
