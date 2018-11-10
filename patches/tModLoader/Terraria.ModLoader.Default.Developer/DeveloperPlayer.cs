using System.Collections.Generic;

namespace Terraria.ModLoader.Default.Developer
{
	internal class DeveloperPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public static DeveloperPlayer GetPlayer(Player player)
			=> player.GetModPlayer<DeveloperPlayer>();

		public AndromedonEffect AndromedonEffect;

		public override void Initialize()
		{
			AndromedonEffect = new AndromedonEffect();
		}

		public override void ResetEffects()
		{
			AndromedonEffect?.ResetEffects();
		}

		public override void UpdateDead()
		{
			AndromedonEffect?.UpdateDead();
		}

		public override void PostUpdate()
		{
			AndromedonEffect?.UpdateEffects(player);
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit)
		{
			AndromedonEffect?.UpdateAura(player);
		}

		public override void ModifyDrawLayers(List<PlayerLayer> layers)
		{
			AndromedonEffect?.ModifyDrawLayers(mod, player, layers);
		}

		public override void clientClone(ModPlayer clientClone)
		{
			((DeveloperPlayer) clientClone).AndromedonEffect = (AndromedonEffect) AndromedonEffect.Clone();
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			// This happens when a player joins the server
			// We need to inform them of our state
			ModNetHandler.Andromedon.SendState(toWho, player.whoAmI, AndromedonEffect);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			DeveloperPlayer devPlayer = (DeveloperPlayer)clientPlayer;

			// If we determine that the aura updated, we need to sync the changes
			if (devPlayer.AndromedonEffect.HasAura != AndromedonEffect.HasAura)
			{
				ModNetHandler.Andromedon.SendAuraTime(-1, player.whoAmI, AndromedonEffect._auraTime);
			}
			else if (devPlayer.AndromedonEffect != AndromedonEffect)
			{
				ModNetHandler.Andromedon.SendState(-1, player.whoAmI, AndromedonEffect);
			}
		}
	}
}
