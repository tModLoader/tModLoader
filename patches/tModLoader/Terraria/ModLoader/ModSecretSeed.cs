using System;
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
			_seedCode = Regex.Replace(value, "[^a-zA-Z0-9 ]+", "");
			SecretSeed.Code = Secrets.ToSecret(Regex.Replace(value.ToLower(), "[^a-z0-9]+", ""));
			SecretSeed.TextThatWasUsedToUnlock = SeedCode;
		}
	}
	private string _seedCode;

	/// <summary>
	/// Whether this seed should be automatically added to the secret seeds menu without needing to be unlocked first.
	/// Can only be set at load time.
	/// </summary>
	public bool AutoUnlock {
		get => _autoUnlock;
		protected set {
			if (!SeedLoader.CanChangeAutoUnlock)
				throw new Exception($"The AutoUnlock property of {FullName} can not be changed after load time.");
			_autoUnlock = value;
		}
	}
	private bool _autoUnlock;

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
		if (string.IsNullOrWhiteSpace(SeedCode)) {
			throw new NullReferenceException($"Secret seed {FullName}'s seed code cannot be null or empty.");
		}
	}
}
