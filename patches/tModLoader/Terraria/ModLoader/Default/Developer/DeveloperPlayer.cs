using System.Collections.Generic;
using Terraria.ModLoader.Default.Developer.Jofairden;

namespace Terraria.ModLoader.Default.Developer
{
	internal class DeveloperPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static DeveloperPlayer GetPlayer(Player player)
			=> player.GetModPlayer<DeveloperPlayer>();

		public AndromedonEffect AndromedonEffect;

		public override void Initialize() {
			AndromedonEffect = new AndromedonEffect();
		}

		public override void ResetEffects() {
			AndromedonEffect?.ResetEffects();
		}

		public override void UpdateDead() {
			AndromedonEffect?.UpdateDead();
		}

		public override void PostUpdate() {
			AndromedonEffect?.UpdateEffects(player);
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			AndromedonEffect?.UpdateAura(player);
		}

		public override void ModifyDrawLayers(List<PlayerLayer> layers) {
			AndromedonEffect?.ModifyDrawLayers(Mod, player, layers);
		}
	}
}
