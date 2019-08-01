using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.Config.UI;

namespace Terraria.ModLoader.Config
{
	// TODO: Enforce no statics allowed.

	/// <summary>
	/// ModConfig provides a way for mods to be configurable. ModConfigs can either be Client specific or Server specific. 
	/// When joining a MP server, Client configs are kept but Server configs are synced from the server.
	/// Using serialization attributes such as [DefaultValue(5)] or [JsonIgnore] are critical for proper usage of ModConfig.
	/// tModLoader also provides its own attributes such as ReloadRequiredAttribute and LabelAttribute. 
	/// </summary>
	public abstract class ModConfig
	{
		[JsonIgnore]
		public Mod mod { get; internal set; }

		[JsonIgnore]
		public string Name { get; internal set; }

		[JsonIgnore]
		public abstract ConfigScope Mode { get; }

		// TODO: Does non-autoloaded ModConfigs have a use-case?
		public virtual bool Autoload(ref string name) => mod.Properties.Autoload;

		/// <summary>
		/// This method is called when the ModConfig has been loaded for the first time. This happens before regular Autoloading and Mod.Load. You can use this hook to assign a static reference to this instance for easy access.
		/// tModLoader will automatically assign (and later unload) this instance to a static field named Instance in the class prior to calling this method, if it exists.
		/// </summary>
		public virtual void OnLoaded()
		{
		}

		/// <summary>
		/// This hook is called anytime new config values have been set and are ready to take effect. This will always be called right after OnLoaded and anytime new configuration values are ready to be used. The hook won't be called with values that violate NeedsReload. Use this hook to integrate with other code in your Mod to apply the effects of the configuration values. If your NeedsReload is correctly implemented, you should be able to apply the settings without error in this hook. Be aware that this hook can be called in-game and in the main menu, as well as in single player and multiplayer situations. 
		/// </summary>
		public virtual void OnChanged()
		{
		}

		/// <summary>
		/// Called on the Server for ServerSide configs to determine if the changes asked for by the Client will be accepted. Useful for enforcing permissions. Called after a check for NeedsReload.
		/// </summary>
		/// <param name="pendingConfig">An instance of the ModConfig with the attempted changes</param>
		/// <param name="whoAmI">The client whoAmI</param>
		/// <param name="message">A message that will be returned to the client, set this to the reason the server rejects the changes.</param>
		/// <returns>Return false to reject client changes</returns>
		public virtual bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			return true;
		}

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
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(this))
			{
				ReloadRequiredAttribute reloadRequired = ConfigManager.GetCustomAttribute<ReloadRequiredAttribute>(variable, this, null);
				if (reloadRequired != null)
				{
					// Do we need to implement nested ReloadRequired? Right now only top level fields will trigger it.
					if (!ConfigManager.ObjectEquals(variable.GetValue(this), variable.GetValue(pendingConfig))) {
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Each ModConfig class has a different scope. Failure to use the correct mode will lead to bugs.
	/// </summary>
	public enum ConfigScope
	{
		/// <summary>
		/// This config is shared between all clients and maintained by the server. Use this for game-play changes that should affect all players the same. ServerSide also covers single player as well.
		/// </summary>
		ServerSide,
		/// <summary>
		/// This config is specific to the client. Use this for personalization options. 
		/// </summary>
		ClientSide,
		// PlayerSpecific,
		// WorldSpecific
	}
}
