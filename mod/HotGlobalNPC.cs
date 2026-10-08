using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

/// <summary>Every enemy lives in SUPERHOT time: it only advances when the player moves. Hits flash white, deaths shatter.</summary>
public class HotGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int Flash;
    float acc;
    bool frozenTick;
    Vector2 stash;
    bool hasStash;
    int savedFrameY;
    double savedCounter;

    static bool Slowable(NPC n) => Mix.ReadyTick >= 0 && (n.CanBeChasedBy() || n.boss) && !n.friendly;

    public override bool PreAI(NPC npc)
    {
        if (hasStash) { npc.velocity += stash; hasStash = false; }
        frozenTick = false;
        if (!Slowable(npc)) return true;
        acc += Hot.Scale;
        if (acc >= 1f) { acc -= 1f; return true; }
        frozenTick = true;
        stash = npc.velocity;
        hasStash = true;
        npc.velocity = Vector2.Zero;
        return false;
    }

    public override void PostAI(NPC npc)
    {
        if (frozenTick) {
            // keep it where it is: no velocity, no falling
            npc.velocity = Vector2.Zero;
        }
        if (Flash > 0) Flash--;
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (frozenTick) { npc.frame.Y = savedFrameY; npc.frameCounter = savedCounter; }
        else { savedFrameY = npc.frame.Y; savedCounter = npc.frameCounter; }
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (!Slowable(npc)) return;
        Flash = 6;
        if (npc.life <= 0) { Hot.Kills++; Hot.Shatter(npc.Center, npc.width, npc.height); }
        else Hot.Shatter(npc.Center, npc.width, npc.height, false);
    }
}
