using System;

namespace Terraria.ModLoader;

/// <summary>
/// This attribute annotates a <see cref="ModNPC"/> class to indicate that the game should autoload the head texture found at <see cref="ModNPC.HeadTexture"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoloadHead : Attribute
{
}

/// <summary>
/// This attribute annotates a <see cref="ModNPC"/> class to indicate that the game should autoload the boss head texture found at <see cref="ModNPC.BossHeadTexture"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoloadBossHead : Attribute
{
}
