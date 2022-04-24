using Terraria.DataStructures;

namespace Terraria
{
	public partial class Player
	{
		public IEntitySource GetSource_Buff(int buffIndex) {
			int buffTypeId = buffType[buffIndex];

			return new EntitySource_Buff(this, buffTypeId, buffIndex);
		}
	}
}
