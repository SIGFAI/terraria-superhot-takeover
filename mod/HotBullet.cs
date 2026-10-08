using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>A glowing red crystal bullet. Hostile when an enemy fires it; the katana can slash it back.</summary>
public class HotBullet : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10; Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.extraUpdates = 0;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, 0.9f, 0.08f, 0.08f);
        if (Main.rand.NextBool(3))
            Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, Vector2.Zero, 0, default, 1.1f).noGravity = true;

        if (Projectile.hostile) {
            // the katana slashes bullets back at the shooter
            foreach (Player p in Main.player) {
                if (!p.active || p.dead || p.itemAnimation <= 0 || p.HeldItem.type != ModContent.ItemType<HotKatana>()) continue;
                if (Vector2.Distance(p.Center, Projectile.Center) < 62f) {
                    Projectile.hostile = false; Projectile.friendly = true;
                    Projectile.velocity = -Projectile.velocity * 1.4f;
                    Projectile.damage = 70;
                    Projectile.penetrate = 2;
                    Mix.Popup(Projectile.Center, "DEFLECT!", new Color(255, 60, 60));
                    Mix.Sound("slash", Projectile.Center, 0.8f, 0.3f);
                    Hot.Shatter(Projectile.Center, 10, 10, false);
                    break;
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) => false;   // drawn above the white wash by HotSystem

    public override void OnKill(int timeLeft)
    {
        Hot.Shatter(Projectile.Center, 10, 10, false);
        Mix.Sound(SoundID.Item27, Projectile.Center);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
}
