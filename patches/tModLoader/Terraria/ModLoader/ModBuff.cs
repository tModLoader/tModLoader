using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader
{
	/// <summary>
	/// This class serves as a place for you to define a new buff and how that buff behaves.
	/// </summary>
	public abstract class ModBuff : ModTexturedType
	{
		/// <summary> The buff id of this buff. </summary>
		public int Type { get; internal set; }

		/// <summary> The translations of this buff's display name. </summary>
		public ModTranslation DisplayName { get; internal set; }

		/// <summary> The translations of this buff's description. </summary>
		public ModTranslation Description { get; internal set; }

		/// <summary> If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode. Defaults to false. </summary>
		public bool LongerExpertDebuff { get; set; }

		/// <summary> Whether or not it is always safe to call Player.DelBuff on this buff. Setting this to false will prevent the nurse from being able to remove this debuff. Defaults to true. </summary>
		public bool CanBeCleared { get; set; } = true;

		protected override sealed void Register() {
			ModTypeLookup<ModBuff>.Register(this);

			Type = BuffLoader.ReserveBuffID();
			DisplayName = LocalizationLoader.GetOrCreateTranslation(Mod, $"BuffName.{Name}");
			Description = LocalizationLoader.GetOrCreateTranslation(Mod, $"BuffDescription.{Name}");

			BuffLoader.buffs.Add(this);
		}

		public sealed override void SetupContent() {
			TextureAssets.Buff[Type] = ModContent.Request<Texture2D>(Texture);
			
			SetStaticDefaults();
			
			BuffID.Search.Add(FullName, Type);
		}

		/// <summary>
		/// Allows you to modify the properties after initial loading has completed.
		/// <br/> This is where all buff related assignments go.
		/// <br/> For example:
		/// <list type="bullet">
		/// <item> Main.debuff[Type] = true; </item>
		/// <item> Main.buffNoTimeDisplay[Type] = true; </item>
		/// <item> Main.pvpBuff[Type] = true; </item>
		/// <item> Main.vanityPet[Type] = true; </item>
		/// <item> Main.lightPet[Type] = true; </item>
		/// </list>
		/// </summary>
		public override void SetStaticDefaults() { }

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
