namespace Terraria.ModLoader;

/// <summary>
/// Orders scene effects from lowest to highest priority.
/// </summary>
/// <remarks>
/// The music examples below refer to scenes in both the regular and Otherworld soundtracks unless specified.
/// The credits and music boxes can take precedence over scene music.
/// </remarks>
public enum SceneEffectPriority
{
	/// <summary>Represents no priority.</summary>
	None,
	/// <summary>Overrides Hallow, Ocean, and standard overworld music. Also overrides Desert music in the Otherworld soundtrack.</summary>
	BiomeLow,
	/// <summary>Overrides Meteor, Jungle, Graveyard, and Snow music. Also overrides Desert and Underground Desert music in the regular soundtrack.</summary>
	BiomeMedium,
	/// <summary>Overrides Temple, Dungeon, Glowing Mushroom, Corruption, and Crimson music.</summary>
	BiomeHigh,
	/// <summary>Overrides Sandstorm, Underworld, Eclipse, Space, and Aether music. Also overrides Town and Slime Rain music in the regular soundtrack.</summary>
	Environment,
	/// <summary>Overrides Pirate Invasion, Goblin Army, Old One's Army, Pumpkin Moon, and Frost Moon music, as well as the Rainbow Boulder music in the regular soundtrack.</summary>
	Event,
	/// <summary>
	/// Overrides most boss music, including the dedicated King Slime, Queen Bee, Eater of Worlds, Skeletron, mechanical boss, and Lunatic Cultist tracks in the regular soundtrack.
	/// This is the default for modded NPC music.
	/// </summary>
	BossLow,
	/// <summary>Overrides Martian Madness, Lunar Pillars, Plantera, and Mechdusa music.</summary>
	BossMedium,
	/// <summary>Overrides Moon Lord and Torch God music.</summary>
	BossHigh
}
