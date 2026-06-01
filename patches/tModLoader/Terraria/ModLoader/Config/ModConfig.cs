using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
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
	/// <br/><br/> In advanced situations <paramref name="pendingConfig"/> can be modified here and the changes will be applied and be synced.
	/// </summary>
	/// <param name="pendingConfig">An instance of the ModConfig with the attempted changes</param>
	/// <param name="whoAmI">The client whoAmI</param>
	/// <param name="message">A message that will be returned to the client, set this to the reason the server rejects the changes.</param>
	/// <returns>Return false to reject client changes</returns>
	public virtual bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		=> true;

	/// <summary>
	/// Called on multiplayer clients after the server accepts or rejects ServerSide config changes made by a client. Can be used to update UI attempting to manually save changes to a ServerSide config (using <see cref="SaveChanges(ModConfig, Action{string, Color}, bool, bool)"/>. For rejections this is only called on the client who requested the changes.
	/// <br/><br/> <paramref name="player"/> indicates which player requested the changes (see <see cref="Main.myPlayer"/>).
	/// <br/><br/> <paramref name="success"/> indicates if the changes were accepted and <paramref name="message"/> is the corresponding message from AcceptClientChanges.
	/// </summary>
	public virtual void HandleAcceptClientChangesReply(bool success, int player, NetworkText message) { }

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
	/// Attempts to save changes made to this ModConfig. This must be called on the active ModConfig instance.
	/// <br/><br/> If <paramref name="pendingConfig"/> is provided, it will be used as the source for the changes to apply to the active config instance. If <paramref name="status"/> is provided, it will be called with text and a color to indicate the status of the operation. If <paramref name="silent"/> is false, sounds will play indicating success or failure. If <paramref name="broadcast"/> is false, the chat message informing all players when a ServerSide config is changed saying "Shared config changed: Message: {0}, Mod: {1}, Config: {2}" will not appear on clients.
	/// <br/><br/> <b>Mod code can run this method in-game, but there are some considerations to keep in mind: </b>
	/// <br/><br/> Calling this method on a <see cref="ConfigScope.ServerSide"/> config from a multiplayer client will result in <see cref="ConfigSaveResult.RequestSentToServer"/> being returned and the actual save logic being performed on the server. <see cref="HandleAcceptClientChangesReply(bool, int, NetworkText)"/> will be called on all clients after the server accepts or denies the changes. Calling this method on the server for a ServerSide config is also supported.
	/// <br/><br/> Attempting to save changes that would violate <see cref="NeedsReload"/> will fail and <see cref="ConfigSaveResult.NeedsReload"/> will be returned.
	/// <br/><br/> If there is a chance that the changes won't be accepted, or if you want to provide a UI for the user to make changes without them taking effect immediately, you should use a clone of the ModConfig and pass it in as <paramref name="pendingConfig"/> instead of modifying the active ModConfig directly. To make a clone, call the <see cref="ConfigManager.GeneratePopulatedClone(ModConfig)"/> method.
	/// <br/><br/> See <see href="https://github.com/tModLoader/tModLoader/blob/stable/ExampleMod/Common/UI/ExampleFullscreenUI/ExampleFullscreenUI.cs">ExampleFullscreenUI.cs</see> for a complete example of using this method.
	/// </summary>
	public ConfigSaveResult SaveChanges(ModConfig pendingConfig = null, Action<string, Color> status = null, bool silent = true, bool broadcast = true)
	{
		if (this != ConfigManager.GetConfig(Mod, Name))
			throw new Exception("SaveChanges must be called on the active config.");
		var modConfig = this;
		pendingConfig = pendingConfig ?? this; // The changes are present in a clone passed in or the active config.
		bool pendingIsActive = pendingConfig == this; // If they were made on the active config, we'll need to restore them if save not accepted.

		// Main Menu: Save, leave reload for later
		// MP with ServerSide: Send request to server
		// SP or (MP with ClientSide): Apply immediately if !NeedsReload
		if (Main.gameMenu) {
			if (!silent)
				SoundEngine.PlaySound(SoundID.MenuOpen);
			ConfigManager.Save(pendingConfig);
			ConfigManager.Load(modConfig);
			// modConfig.OnChanged(); delayed until ReloadRequired checked
			// Reload will be forced by Back Button in UIMods if needed
		}
		else {
			// If we are in game...
			if (pendingConfig.Mode == ConfigScope.ServerSide && Main.netMode == NetmodeID.MultiplayerClient) {
				//if (pendingIsActive)
				//	throw new Exception("SaveChanges for ServerSide configs must be called on a clone of the active config for multiplayer compatibility.");

				status?.Invoke(Language.GetTextValue("tModLoader.ModConfigAskingServerToAcceptChanges"), Color.Yellow); // "Asking server to accept changes..."

				var requestChanges = new ModPacket(MessageID.InGameChangeConfig);
				requestChanges.Write(pendingConfig.Mod.Name);
				requestChanges.Write(pendingConfig.Name);
				string json = JsonConvert.SerializeObject(pendingConfig, ConfigManager.serializerSettingsCompact);
				requestChanges.Write(broadcast);
				requestChanges.Write(json);
				requestChanges.Send();

				if (pendingIsActive)
					ConfigManager.Load(modConfig);

				return ConfigSaveResult.RequestSentToServer;
			}

			// SP with either, MP with ClientSide, or Server with ServerSide
			ModConfig loadTimeConfig = ConfigManager.GetLoadTimeConfig(modConfig.Mod, modConfig.Name);

			if (loadTimeConfig.NeedsReload(pendingConfig)) {
				if (!silent)
					SoundEngine.PlaySound(SoundID.MenuClose);
				status?.Invoke(Language.GetTextValue("tModLoader.ModConfigCantSaveBecauseChangesWouldRequireAReload"), Color.Red); // "Can't save because changes would require a reload."
				if (pendingIsActive)
					ConfigManager.Load(modConfig);
				return ConfigSaveResult.NeedsReload;
			}
			else {
				if (!silent)
					SoundEngine.PlaySound(SoundID.MenuOpen);
				ConfigManager.Save(pendingConfig);
				ConfigManager.Load(modConfig);
				modConfig.OnChanged();

				if (pendingConfig.Mode == ConfigScope.ServerSide && Main.netMode == NetmodeID.Server) {
					// Send new config to all clients
					var p = new ModPacket(MessageID.InGameChangeConfig);
					p.Write(true);
					NetworkText message = NetworkText.FromKey("tModLoader.ModConfigAccepted");
					message.Serialize(p);
					p.Write(modConfig.Mod.Name);
					p.Write(modConfig.Name);
					p.Write(broadcast);
					p.Write((byte)255);
					string json = JsonConvert.SerializeObject(modConfig, ConfigManager.serializerSettingsCompact);
					p.Write(json);
					p.Send();
				}
			}
		}

		status?.Invoke(Language.GetTextValue("tModLoader.ModConfigConfigSaved"), Color.Green);

		return ConfigSaveResult.Success;
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