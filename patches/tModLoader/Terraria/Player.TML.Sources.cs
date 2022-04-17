using Terraria.DataStructures;

namespace Terraria
{
	public partial class Player
	{
		public IEntitySource GetSource_Buff(int buffIndex) {
			int buffId = buffType[buffIndex];

			return new EntitySource_Buff(this, buffId, buffIndex);
		}
	}
}
