using Newtonsoft.Json;
using Terraria.ModLoader.Config.UI;

namespace Terraria.ModLoader.Config
{
	// TODO: PostSave hook
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
		public abstract MultiplayerSyncMode Mode { get; }

		// TODO: Does non-autoloaded ModConfigs have a usecase?
		public virtual bool Autoload(ref string name) => mod.Properties.Autoload;

		// PostAutoLoad is called right after
		public virtual void PostAutoLoad()
		{
			// TODO: is this name misleading?
		}

		// Called after changes have been made. Useful for informing the mod of changes to config.
		public virtual void PostSave()
		{
		}

		// Called on the Server for ServerDictates configs to determine if the changes asked for by the Client will be accepted. Useful for enforcing permissions.
		public virtual bool AcceptClientChanges(ModConfig currentConfig, int whoAmI, ref string message)
		{
			return true;
		}

		// TODO: Can we get rid of Clone and just load from disk? Don't think so yet.
		public virtual ModConfig Clone() => (ModConfig)MemberwiseClone();

		/// <summary>
		/// Whether or not a reload is required. The default implementation compares properties and fields annotated with the ReloadRequiredAttribute.
		/// </summary>
		/// <param name="old">The other instance of ModConfig to compare against</param>
		/// <returns></returns>
		public virtual bool NeedsReload(ModConfig old)
		{
			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(this))
			{
				ReloadRequiredAttribute reloadRequired = ConfigManager.GetCustomAttribute<ReloadRequiredAttribute>(variable, this, null);
				if (reloadRequired != null)
				{
					if (!variable.GetValue(this).Equals(variable.GetValue(old)))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public enum MultiplayerSyncMode
	{
		ServerDictates,
		UniquePerPlayer,
		// Player,
		// World
	}
}
