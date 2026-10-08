using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>A red crystal gunman: walks up, stops at range, aims, then fires a slow glowing bullet.</summary>
public class RedMan : ModNPC
{
    int frameStep;
    float walkAcc;
    int fireTimer;
    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 4;

    public override void SetDefaults()
    {
        NPC.width = 20; NPC.height = 52;
        NPC.lifeMax = 54; NPC.damage = 18; NPC.defense = 0; NPC.knockBackResist = 0.8f; NPC.value = 60;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.Item27;
        NPC.DeathSound = null;
        NPC.noGravity = false;
        NPC.spriteDirection = -1;
    }

    public override void AI()
    {
        NPC.TargetClosest(true);
        Player t = Main.player[NPC.target];
        if (!t.active || t.dead) return;
        float dx = t.Center.X - NPC.Center.X;
        float dist = System.Math.Abs(dx);
        NPC.direction = dx < 0 ? -1 : 1;
        NPC.spriteDirection = NPC.direction;

        bool los = Collision.CanHitLine(NPC.Center, 1, 1, t.Center, 1, 1);
        float want = 0;
        if (dist > 230 || !los) want = NPC.direction * 1.5f;
        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, want, 0.15f);
        if (want != 0 && NPC.collideX && NPC.velocity.Y == 0) NPC.velocity.Y = -6.2f;
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        if (System.Math.Abs(NPC.velocity.X) > 0.2f) walkAcc += System.Math.Abs(NPC.velocity.X);

        fireTimer++;
        if (dist < 520 && los && fireTimer > 130) {
            // telegraph: red sparks at the gun
            Vector2 muzzle = NPC.Center + new Vector2(NPC.direction * 20, -6);
            if (fireTimer < 160) {
                if (fireTimer % 6 == 0) Dust.NewDustPerfect(muzzle, DustID.RedTorch, Main.rand.NextVector2Circular(1.5f, 1.5f), 0, default, 1.4f).noGravity = true;
                Lighting.AddLight(muzzle, 0.9f, 0.1f, 0.1f);
            } else {
                fireTimer = Main.rand.Next(-30, 20);
                Vector2 v = Mix.Aim(muzzle, t.Center, 6.5f);
                Mix.Shoot<HotBullet>(muzzle, v, 15, 2f, true);
                Mix.Sound("gunshot", muzzle, 0.5f, 0.1f);
                Mix.Burst(muzzle, DustID.RedTorch, 10, 6, 3f);
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        frameStep = (int)(walkAcc / 9f) % 4;
        NPC.frame.Y = (System.Math.Abs(NPC.velocity.X) > 0.2f || walkAcc > 0 && NPC.velocity.Y == 0 ? frameStep : 0) * frameHeight;
        if (NPC.velocity.Y != 0) NPC.frame.Y = frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;   // drawn on top of the white wash by HotSystem

    public override void OnKill()
    {
        Mix.Drop(ItemID.SilverCoin, NPC.Center, Main.rand.Next(2, 6));
        if (Main.rand.NextBool(6)) Mix.Drop(ItemID.Heart, NPC.Center);
        if (Main.rand.NextBool(7)) Mix.Drop<HotKatana>(NPC.Center);
        if (Main.rand.NextBool(7)) Mix.Drop<HotPistol>(NPC.Center);
    }
}
