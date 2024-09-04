namespace Terraria.ModLoader;

[Autoload(false)]
public class SimpleModCloud : ModCloud
{
    public override float SpawnChance(int cloudIndex)
    {
        return 1f;
    }
}