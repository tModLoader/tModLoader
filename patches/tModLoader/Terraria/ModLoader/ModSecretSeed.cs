using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace Terraria.ModLoader;

/// <summary>
/// This type of class represents a secret seed added by a mod. Secret seeds can be used to change gameplay and/or worldgen only for worlds that have them enabled.
/// You can check if a ModSecretSeed is enabled for a particular world by using <see cref="SeedLoader.SeedEnabled"/>.
/// <br/><br/>Unlike special seeds, secret seeds are simpler and more out-of-the-way. They also need to be unlocked and can be more hidden.
/// </summary>
public abstract class ModSecretSeed : ModSeedType
{
	/// <summary>
	/// The code used to match the secret seed input with your secret seed. It is formatted to remove spaces and special characters.
	/// <br/><br/>Note that this code also acts as the seed's name in the secret seeds menu.
	/// </summary>
	public string SeedCode {
		get => _seedCode;
		set {
			_seedCode = value;
			if (CodeEncrypted) {
				SecretSeed.TextThatWasUsedToUnlock = SeedCode;
				SecretSeed.Code = value;
				return;
			}
			_seedCode = Regex.Replace(value, "[^a-zA-Z0-9 ]+", "");
			SecretSeed.Code = Secrets.ToSecret(Regex.Replace(value.ToLower(), "[^a-z0-9]+", ""));
			SecretSeed.TextThatWasUsedToUnlock = SeedCode;
		}
	}
	private string _seedCode;

	/// <summary>
	/// Setting this to true will make the game presume that <see cref="SeedCode"/> has already been encrypted and doesn't need to be encrypted again.
	/// <br/><br/>Useful if you want to prevent players from finding out the seed's code by looking at the source.
	/// <br/><br/>Only use if you believe both: <br/>1) that your players will look at the source code to find the secret seed <br/>2) that your
	/// players will still look for the seed after finding it encrypted in the source code.
	/// </summary>
	public bool CodeEncrypted {
		get => _codeEncrypted;
		set {
			if (value && !_codeEncrypted) {
				SecretSeed.Code = _seedCode;
			}
			if (!value && _codeEncrypted) {
				SecretSeed.Code = Secrets.ToSecret(_seedCode);
			}
			_codeEncrypted = value;
		}
	}
	private bool _codeEncrypted;

	public bool Known {
		get => (AutoUnlock && !SecretSeedsTracker.ProcessedConfig) || SecretSeedsTracker.SeedsForInterface.Contains(SecretSeed);
		set {
			if (!SecretSeedsTracker.ProcessedConfig) {
				AutoUnlock = value;
				return;
			}
			if (value && !Known) {
				SecretSeedsTracker.SeedsForInterface.Add(SecretSeed);
			}
			if (!value && Known) {
				SecretSeedsTracker.SeedsForInterface.Remove(SecretSeed);
			}
		}
	}
	internal bool AutoUnlock { get; private set; }

	/// <summary>
	/// The <see cref="WorldGen.SecretSeed"/> object that this ModSecretSeed controls.
	/// </summary>
	public WorldGen.SecretSeed SecretSeed { get; private set; }

	protected sealed override void Register()
	{
		ModTypeLookup<ModSecretSeed>.Register(this);
		SeedLoader.Add(this);
	}

	public sealed override void SetupContent()
	{
		SecretSeed = new WorldGen.SecretSeed(Description, SoundID.MenuAccept, "");
		SecretSeed.ModSecretSeed = this;
		SetStaticDefaults();
	}
}