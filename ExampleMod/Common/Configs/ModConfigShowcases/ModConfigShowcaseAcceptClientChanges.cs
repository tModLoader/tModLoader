using ExampleMod.Common.UI.ExampleFullscreenUI;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExampleMod.Common.Configs.ModConfigShowcases
{
	// ModConfigShowcaseAcceptClientChanges showcases the AcceptClientChanges method.
	// In multiplayer all connected clients can attempt to change ServerSide configs.
	// By default changes that don't require a reload will be accepted, but modders can use AcceptClientChanges to further limit this behavior.
	// Use the AcceptClientChanges method to determine if the clients changes to this config will be accepted and relayed to all other clients.
	public class ModConfigShowcaseAcceptClientChanges : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public bool OnlyChangeableDuringNight;

		public int SomeNumber;

		// This LocalizedText will be used for a rejection message. Since it is static it won't be part of the config shown in-game.
		public static LocalizedText RejectChangesDaytime { get; private set; }

		public override void OnLoaded() {
			RejectChangesDaytime = this.GetLocalization(nameof(RejectChangesDaytime));
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message) {
			// If OnlyChangeableDuringNight has changed and it is day time, we reject the changes.
			// This is a toy example. A real mod might have some logic that ReloadRequired wouldn't be suitable for.
			if (Main.dayTime && ((ModConfigShowcaseAcceptClientChanges)pendingConfig).OnlyChangeableDuringNight != OnlyChangeableDuringNight) {
				// The NetworkText class ensures that messages are shown to clients in the client's selected language. The NetworkText.FromKey and LocalizedText.ToNetworkText methods can be used to create a NetworkText object from a translation key.
				// The NetworkText.FromLiteral method allows sending a string directly but is not recommended to use.
				message = RejectChangesDaytime.ToNetworkText();
				return false; // return false to reject the changes
			}

			// This code limits changes to only the local host (the host and play client).
			if (!NetMessage.DoesPlayerSlotCountAsAHost(whoAmI)) {
				message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost"); // "Only the host can change this config"
				return false;
			}
			// Note: The local host approach is a simple, but won't work for users hosting tModLoader on a dedicated server.
			// There is currently no tModLoader provided authentication mechanism, but some mods implement their own systems to determine
			// if a remote client should be treated as the host.
			// The Shorter Respawn mod, for example, uses the user permissions system of HEROs Mod (a mod that is intended for
			// multiplayer administration) to determine if a particular user has permission to change the config:
			// https://github.com/JavidPack/ShorterRespawn/blob/1.4/ShorterRespawnConfig.cs#L85

			// Accept the changes. This is the default behavior of the AcceptClientChanges method.
			return true;
		}

		public override void HandleAcceptClientChangesReply(bool success, int player, NetworkText message) {
			ExampleFullscreenUI.instance.UpdateConfigSaveStatusMessage(message.ToString(), success ? Color.Green : Color.Red);

			if (success) {
				ExampleFullscreenUI.instance.RefreshContents();
			}

			// Only play the sound if this player requested the changes
			if (player == Main.myPlayer) {
				if (success) {
					SoundEngine.PlaySound(SoundID.CoinPickup);
				}
				else {
					SoundEngine.PlaySound(SoundID.Duck);
				}
			}
		}
	}
}
