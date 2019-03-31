namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to define a new buff and how that buff behaves.
	/// </summary>
	public class ModBuff
	{
		/// <summary>
		/// The mod that added this ModBuff.
		/// </summary>
		public Mod mod {
			get;
			internal set;
		}

		/// <summary>
		/// The internal name of this type of buff.
		/// </summary>
		public string Name {
			get;
			internal set;
		}

		/// <summary>
		/// The buff id of this buff.
		/// </summary>
		public int Type {
			get;
			internal set;
		}

		/// <summary>
		/// The translations of this buff's display name.
		/// </summary>
		public ModTranslation DisplayName {
			get;
			internal set;
		}

		/// <summary>
		/// The translations of this buff's description.
		/// </summary>
		public ModTranslation Description {
			get;
			internal set;
		}

		internal string texture;
		/// <summary>If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode. Defaults to false.</summary>
		public bool longerExpertDebuff = false;
		/// <summary>Whether or not it is always safe to call Player.DelBuff on this buff. Setting this to false will prevent the nurse from being able to remove this debuff. Defaults to true.</summary>
		public bool canBeCleared = true;

		/// <summary>
		/// Allows you to automatically load a buff instead of using Mod.AddBuff. Return true to allow autoloading; by default returns the mod's autoload property. Name is initialized to the overriding class name and texture is initialized to the namespace and overriding class name with periods replaced with slashes. Use this method to either force or stop an autoload, and to change the default display name and texture path.
		/// </summary>
		public virtual bool Autoload(ref string name, ref string texture) {
			return mod.Properties.Autoload;
		}

		/// <summary>
		/// This is where all buff related assignments go. For example:
		/// <list type="bullet">
		/// <item>Main.buffName[Type] = "Display Name";</item>
		/// <item>Main.buffTip[Type] = "Buff Tooltip";</item>
		/// <item>Main.debuff[Type] = true;</item>
		/// <item>Main.buffNoTimeDisplay[Type] = true;</item>
		/// <item>Main.vanityPet[Type] = true;</item>
		/// <item>Main.lightPet[Type] = true;</item>
		/// </list>
		/// </summary>
		public virtual void SetDefaults() {
		}

		/// <summary>
		/// Allows you to make this buff give certain effects to the given player. If you remove the buff from the player, make sure the decrement the buffIndex parameter by 1.
		/// </summary>
		public virtual void Update(Player player, ref int buffIndex) {
		}

		/// <summary>
		/// Allows you to make this buff give certain effects to the given NPC. If you remove the buff from the NPC, make sure to decrement the buffIndex parameter by 1.
		/// </summary>
		public virtual void Update(NPC npc, ref int buffIndex) {
		}

		/// <summary>
		/// Allows to you make special things happen when adding this buff to a player when the player already has this buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
		/// </summary>
		public virtual bool ReApply(Player player, int time, int buffIndex) {
			return false;
		}

		/// <summary>
		/// Allows to you make special things happen when adding this buff to an NPC when the NPC already has this buff. Return true to block the vanilla re-apply code from being called; returns false by default. The vanilla re-apply code sets the buff time to the "time" argument if that argument is larger than the current buff time.
		/// </summary>
		public virtual bool ReApply(NPC npc, int time, int buffIndex) {
			return false;
		}

		/// <summary>
		/// Allows you to modify the tooltip that displays when the mouse hovers over the buff icon, as well as the color the buff's name is drawn in.
		/// </summary>
		public virtual void ModifyBuffTip(ref string tip, ref int rare) {
		}
	}
}
