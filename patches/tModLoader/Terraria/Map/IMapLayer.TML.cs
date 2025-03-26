using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace Terraria.Map;
public partial interface IMapLayer
{
	public static IMapLayer Spawn { get; private set; } = new SpawnMapLayer();
	public static IMapLayer Pylons { get; private set; } = new TeleportPylonsMapLayer();
	public static IMapLayer Pings { get; private set; } = new PingMapLayer();

	bool Visible { get; internal set; }

	void Hide() => Visible = false;

	Position GetDefaultPosition() => new Before(null);

	IEnumerable<Position> GetModdedConstraints() => null;

	#region Sort Positions

	public abstract class Position;

	public sealed class Append : Position;

	public sealed class Before : Position
	{
		public IMapLayer Layer { get; }

		public Before(IMapLayer layer)
		{
			Layer = layer;
		}
	}

	public sealed class After : Position
	{
		public IMapLayer Layer { get; }

		public After(IMapLayer layer)
		{
			Layer = layer;
		}
	}

	#endregion
}
