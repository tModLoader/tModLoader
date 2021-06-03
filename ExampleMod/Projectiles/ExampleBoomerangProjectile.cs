using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
    public class ExampleBoomerangProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Example Boomerang");
        }

        public override void SetDefaults()
        {
            projectile.friendly = true; //Makes the projectile hurt enemies
            projectile.hostile = false; //Makes the projectile not hurt friendly creatures
            projectile.aiStyle = 3; //Gives the projectile boomerang behavior - return after being thrown, and die when touching player after returning

            projectile.width = 18; //Sets the width
            projectile.height = 32; //Sets the height
            projectile.melee = true; //Makes the projectile deal melee damage
            projectile.timeLeft = 300; //Sets the projectile's lifespan in ticks(60 ticks in a second), you can set this very high if you only want it to die after reaching the player
            projectile.tileCollide = true; //Makes the projectile die on contact with tiles - you can set this to false if you want it to pass through tiles
        }

        public override void AI()
        {
            projectile.rotation += 0.1f; //Dictates how fast the projectile rotates, set higher for faster rotation and lower for slower rotation, and make -= instead of += for rotation in the other direction
        }
    }
}