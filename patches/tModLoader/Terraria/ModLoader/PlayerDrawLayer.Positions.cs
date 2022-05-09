using System.Collections;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	public abstract partial class PlayerDrawLayer
	{
		public abstract class Position { }

		public sealed class Between : Position
		{
			public PlayerDrawLayer Layer1 { get; }
			public PlayerDrawLayer Layer2 { get; }

			public Between(PlayerDrawLayer layer1, PlayerDrawLayer layer2) {
				Layer1 = layer1;
				Layer2 = layer2;
			}

			public Between() {
			}
		}

		public class Multiple : Position, IEnumerable
		{
			public delegate bool Condition(PlayerDrawSet drawInfo);
			public IList<(Between, Condition)> Positions { get; } = new List<(Between, Condition)>();

			public void Add(Between position, Condition condition) => Positions.Add((position, condition));

			public IEnumerator GetEnumerator() => Positions.GetEnumerator();
		}

		public class BeforeParent : Position
		{
			public PlayerDrawLayer Parent { get; }

			public BeforeParent(PlayerDrawLayer parent) {
				Parent = parent;
			}
		}

		public class AfterParent : Position
		{
			public PlayerDrawLayer Parent { get; }

			public AfterParent(PlayerDrawLayer parent) {
				Parent = parent;
			}
		}
	}
}
