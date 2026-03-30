namespace Terraria.Map;

public partial interface IMapLayer
{
	public static IMapLayer BossBag { get; private set; } = new BossBagMapLayer();
	public static IMapLayer Spawn { get; private set; } = new SpawnMapLayer();
	public static IMapLayer TeamBasedSpawn { get; private set; } = new TeamBasedSpawnMapLayer();
	public static IMapLayer Pylons { get; private set; } = new TeleportPylonsMapLayer();
	public static IMapLayer Pings { get; private set; } = new PingMapLayer();

	bool Visible { get; internal set; }

	void Hide() => Visible = false;
}
