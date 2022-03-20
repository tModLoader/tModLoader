using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Drawing;


namespace ExampleMod.Items
{
    public class ExampleWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            //Items needed to Journey Mode Research
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public void WhipDefaults(int projectileId, int dmg, float kb, float shootspeed, bool reuse = true, int animationTotalTime = 30)
        {
            //If the Whip is going to be Reusable
            Item.autoReuse = reuse;
            //The Whip name color
            Item.rare = ItemRarityID.Red;
            Item.useStyle = 1;
            Item.useAnimation = animationTotalTime;
            Item.useTime = animationTotalTime;
            Item.width = 18;
            Item.height = 18;
            Item.shoot = projectileId;
            Item.UseSound = SoundID.Item152;
            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;
            Item.noUseGraphic = true;
            Item.damage = dmg;
            Item.knockBack = kb;
            Item.shootSpeed = shootspeed;
        }
        public override void SetDefaults()
        {
            //The first field is to set the Whip Projectile // The second is the damage // The third is the Knockback // The fourth is the lenght of the whip
            WhipDefaults(ModContent.ProjectileType<ExampleWhipProjectile>(), 190, 4f, 13f);
        }

        public class ExampleWhipProjectile : ModProjectile
        {
            public override void SetStaticDefaults()
            {
                //The Whip Projectile file
                DisplayName.SetDefault("ExampleWhipProjectile");
            }
            public override void SetDefaults()
            {
                Projectile.width = 18;
                Projectile.height = 18;
                Projectile.friendly = true;
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
                Projectile.scale = 1f;
                Projectile.ownerHitCheck = true;
                Projectile.extraUpdates = 1;
                Projectile.usesLocalNPCImmunity = true;
                Projectile.localNPCHitCooldown = -1;
                Projectile.DamageType = DamageClass.Summon;
            }


            //How many segments the whip will have
            protected int whipSegments = 20;
            protected float whipRangeMultiplier = 1f;
            //The color of the line in the middle of the whip
            protected Color originalColor = Color.White;
            protected int tag = -1;
            protected float tipScale = 1.5f;
            protected float fallOff = 0.3f;
            public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
            {
                ProjectileID.Sets.IsAWhip[Type] = true;
            }
            public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
            {
                Projectile.damage = (int)(Projectile.damage * (1f - fallOff));
                if (tag != -1)
                {
                    target.AddBuff(tag, 240);
                }
                Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
                ProjectileID.Sets.IsAWhip[Type] = false;

            }

            public override void AI()
            {

                Player player = Main.player[Projectile.owner];
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
                Projectile.ai[0] += 1f;
                GetWhipSettings(Projectile, out var timeToFlyOut, out var _, out var _);
                Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
                Projectile.spriteDirection = ((!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : (-1));
                if (Projectile.ai[0] >= timeToFlyOut || player.itemAnimation == 0)
                {
                    Projectile.Kill();
                    return;
                }
                player.heldProj = Projectile.whoAmI;
                player.itemAnimation = player.itemAnimationMax - (int)(Projectile.ai[0] / (float)Projectile.MaxUpdates);
                player.itemTime = player.itemAnimation;
                if (Projectile.ai[0] == (float)(int)(timeToFlyOut / 2f))
                {
                    Projectile.WhipPointsForCollision.Clear();
                    FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
                    Vector2 position = Projectile.WhipPointsForCollision[Projectile.WhipPointsForCollision.Count - 1];
                    SoundEngine.PlaySound(SoundID.Item153, position);
                }

            }

            public void GetWhipSettings(Projectile proj, out float timeToFlyOut, out int segments, out float rangeMultiplier)
            {
                timeToFlyOut = Main.player[proj.owner].itemAnimationMax * proj.MaxUpdates;
                segments = whipSegments;
                rangeMultiplier = whipRangeMultiplier;
            }
            public void FillWhipControlPoints(Projectile proj, List<Vector2> controlPoints)
            {
                GetWhipSettings(proj, out var timeToFlyOut, out var segments, out var rangeMultiplier);
                float num = proj.ai[0] / timeToFlyOut;
                float num2 = 0.5f;
                float num3 = 1f + num2;
                float num4 = (float)Math.PI * 10f * (1f - num * num3) * (float)(-proj.spriteDirection) / (float)segments;
                float num5 = num * num3;
                float num6 = 0f;
                if (num5 > 1f)
                {
                    num6 = (num5 - 1f) / num2;
                    num5 = MathHelper.Lerp(1f, 0f, num6);
                }
                float num7 = proj.ai[0] - 1f;
                Player player = Main.player[proj.owner];
                Item heldItem = Main.player[proj.owner].HeldItem;
                num7 = (float)(ContentSamples.ItemsByType[heldItem.type].useAnimation * 2) * num * player.whipRangeMultiplier;
                float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / (float)segments;
                float num9 = 1f;
                Vector2 playerArmPosition = Main.GetPlayerArmPosition(proj);
                Vector2 vector = playerArmPosition;
                float num10 = 0f - (float)Math.PI / 2f;
                Vector2 value = vector;
                float num11 = 0f + (float)Math.PI / 2f + (float)Math.PI / 2f * (float)proj.spriteDirection;
                Vector2 value2 = vector;
                float num12 = 0f + (float)Math.PI / 2f;
                controlPoints.Add(playerArmPosition);
                for (int i = 0; i < segments; i++)
                {
                    float num13 = (float)i / (float)segments;
                    float num14 = num4 * num13 * num9;
                    Vector2 vector2 = vector + num10.ToRotationVector2() * num8;
                    Vector2 vector3 = value2 + num12.ToRotationVector2() * (num8 * 2f);
                    Vector2 vector4 = value + num11.ToRotationVector2() * (num8 * 2f);
                    float num15 = 1f - num5;
                    float num16 = 1f - num15 * num15;
                    Vector2 value3 = Vector2.Lerp(vector3, vector2, num16 * 0.9f + 0.1f);
                    Vector2 value4 = Vector2.Lerp(vector4, value3, num16 * 0.7f + 0.3f);
                    Vector2 spinningpoint = playerArmPosition + (value4 - playerArmPosition) * new Vector2(1f, num3);
                    float num17 = num6;
                    num17 *= num17;
                    Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * (float)proj.spriteDirection, playerArmPosition);
                    controlPoints.Add(item);
                    num10 += num14;
                    num12 += num14;
                    num11 += num14;
                    vector = vector2;
                    value2 = vector3;
                    value = vector4;
                }
            }
            private void DrawWhip(Projectile proj)
            {
                List<Vector2> list = new List<Vector2>();
                FillWhipControlPoints(proj, list);
                Texture2D value = TextureAssets.FishingLine.Value;
                Microsoft.Xna.Framework.Rectangle value2 = value.Frame();
                Vector2 origin = new Vector2(value2.Width / 2, 2f);

                Vector2 value3 = list[0];
                for (int i = 0; i < list.Count - 1; i++)
                {
                    Vector2 vector = list[i];
                    Vector2 vector2 = list[i + 1] - vector;
                    float rotation = vector2.ToRotation() - (float)Math.PI / 2f;
                    Microsoft.Xna.Framework.Color color = Lighting.GetColor(vector.ToTileCoordinates(), originalColor);
                    Vector2 scale = new Vector2(1f, (vector2.Length() + 2f) / (float)value2.Height);
                    Main.EntitySpriteDraw(value, value3 - Main.screenPosition, value2, color, rotation, origin, scale, SpriteEffects.None, 0);
                    value3 += vector2;
                }
                DrawWhip_Local(proj, list);
            }
            public Vector2 DrawWhip_Local(Projectile proj, List<Vector2> controlPoints)
            {
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (proj.spriteDirection == 1)
                {
                    spriteEffects ^= SpriteEffects.FlipHorizontally;
                }
                Texture2D value = TextureAssets.Projectile[proj.type].Value;
                Rectangle rectangle = value.Frame(1, 5);
                int height = rectangle.Height;
                rectangle.Height -= 2;
                Vector2 vector = rectangle.Size() / 2f;
                Vector2 vector2 = controlPoints[0];
                for (int seg = 0; seg < controlPoints.Count - 1; seg++)
                {
                    bool flag = false;
                    Vector2 origin = vector;
                    float scale = 1f;
                    if (seg == 0)
                    {
                        origin.Y -= 4f;
                        flag = true;
                    }
                    else
                    {
                        flag = true;
                        int frame = 1;
                        ModifyMiddleSegments(seg, ref frame);
                        rectangle.Y = height * frame;
                    }
                    if (seg == controlPoints.Count - 2)
                    {
                        flag = true;
                        rectangle.Y = height * 4;
                        scale = tipScale;
                        Projectile.GetWhipSettings(proj, out var timeToFlyOut, out var _, out var _);
                        float t = proj.ai[0] / timeToFlyOut;
                        //float amount = Utils.GetLerpValue(0.1f, 0.7f, t, clamped: true) * Utils.GetLerpValue(0.9f, 0.7f, t, clamped: true);
                        //scale = MathHelper.Lerp(0.5f, 1.5f, amount);
                    }
                    Vector2 vector3 = controlPoints[seg];
                    Vector2 vector4 = controlPoints[seg + 1] - vector3;
                    if (flag)
                    {
                        float rotation = vector4.ToRotation() - (float)Math.PI / 2f;
                        Microsoft.Xna.Framework.Color color = Lighting.GetColor(vector3.ToTileCoordinates());
                        Main.EntitySpriteDraw(value, vector2 - Main.screenPosition, rectangle, color, rotation, origin, scale, spriteEffects, 0);
                    }
                    vector2 += vector4;
                }
                return vector2;
            }
            protected virtual void ModifyMiddleSegments(int seg, ref int frame)
            {
                if (seg > 10)
                {
                    frame = 2;
                }
                if (seg > 20)
                {
                    frame = 3;
                }

            }
            public override bool PreDraw(ref Color lightColor)
            {
                DrawWhip(Projectile);
                return false;
            }
            public override void CutTiles()
            {
                Projectile.WhipPointsForCollision.Clear();
                FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
                Vector2 zero = new Vector2((float)Projectile.width * Projectile.scale / 2f, 0f);
                for (int flag = 0; flag < Projectile.WhipPointsForCollision.Count; flag++)
                {
                    DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
                    Utils.PlotTileLine(Projectile.WhipPointsForCollision[flag] - zero, Projectile.WhipPointsForCollision[flag] + zero, (float)Projectile.height * Projectile.scale, DelegateMethods.CutTiles);
                }
            }
            public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
            {
                Projectile.WhipPointsForCollision.Clear();
                FillWhipControlPoints(Projectile, Projectile.WhipPointsForCollision);
                for (int n = 0; n < Projectile.WhipPointsForCollision.Count; n++)
                {
                    Point point = Projectile.WhipPointsForCollision[n].ToPoint();
                    projHitbox.Location = new Point(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2);
                    if (projHitbox.Intersects(targetHitbox))
                    {
                        return true;
                    }
                }
                return false;
            }
        }


        public override void AddRecipes()
        {
            //The recipe
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(4914);
            recipe.AddTile(134);
            recipe.Register();
        }
    }
}

