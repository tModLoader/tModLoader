using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Terraria.Unified.Startup;

/// <summary>
///		A game host lifetime.
/// </summary>
public sealed record GameLifetime(IHost Host, ILogger GameLogger);
