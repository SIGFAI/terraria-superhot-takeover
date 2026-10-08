using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Enemy bullets crawl while the player stands still.</summary>
public class HotGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    float acc;
    bool frozenTick;
    Vector2 stash;
    bool hasStash;

    public override bool PreAI(Projectile p)
    {
        if (hasStash) { p.velocity += stash; hasStash = false; }
        frozenTick = false;
        if (Mix.ReadyTick < 0 || !p.hostile) return true;
        acc += Hot.Scale;
        if (acc >= 1f) { acc -= 1f; return true; }
        frozenTick = true;
        stash = p.velocity;
        hasStash = true;
        p.velocity = Vector2.Zero;
        if (p.timeLeft < 3000) p.timeLeft++;   // frozen time does not use up its life
        return false;
    }

    public override void PostAI(Projectile p)
    {
        if (frozenTick) p.velocity = Vector2.Zero;
    }
}
