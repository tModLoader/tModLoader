using Terraria;
using Terraria.DataStructures;

public class EntitySourceTest
{
	void TestSources(IEntitySource source)
	{
		if (source is EntitySource_BossSpawn sb)
			_ = sb.Entity;

		if (source is EntitySource_FishedOut sf)
			_ = sf.Entity;

		if (source is EntitySource_OnHit hit && hit.EntityStriking is Player)
			_ = hit.EntityStruck;
	}
}