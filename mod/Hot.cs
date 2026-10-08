using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Sigf.Content;

/// <summary>Shared SUPERHOT state: time only moves when the player moves.</summary>
public static class Hot
{
    /// <summary>1 = normal time, close to 0 = frozen.</summary>
    public static float Scale = 1f;
    public static int Flash;            // white screen flash (ticks left)
    public static string Word;          // big SUPER / HOT word on screen
    public static long WordStart;
    public static long LastVoice = -99999;
    static bool wasFrozen;
    static long lastWord;
    public static int Kills;
    public static string TitleBig, TitleSmall = "";
    public static long TitleUntil;
    public static void Title(string big, string small, double sec) { TitleBig = big; TitleSmall = small; TitleUntil = Mix.Tick + (long)(sec * 60); }

    public class Shard { public Vector2 Pos, Vel; public float Rot, Spin, Size; public int Life, Max; }
    public static readonly List<Shard> Shards = new();

    static void AddShards(Vector2 center, int w, int h, int n, float speed)
    {
        for (int i = 0; i < n && Shards.Count < 400; i++) {
            Shards.Add(new Shard {
                Pos = center + new Vector2(Main.rand.NextFloat(-w, w), Main.rand.NextFloat(-h, h)) * 0.5f,
                Vel = Main.rand.NextVector2Circular(speed, speed) + new Vector2(0, -speed * 0.4f),
                Rot = Main.rand.NextFloat(6.28f), Spin = Main.rand.NextFloat(-0.25f, 0.25f),
                Size = Main.rand.NextFloat(0.9f, 2.0f), Life = Main.rand.Next(70, 130), Max = 130 });
        }
    }

    static void UpdateShards()
    {
        float s = Math.Max(Scale, 0.04f);
        for (int i = Shards.Count - 1; i >= 0; i--) {
            var d = Shards[i];
            d.Vel.Y += 0.22f * s; d.Vel.X *= 1f - 0.01f * s;
            d.Pos += d.Vel * s; d.Rot += d.Spin * s;
            d.Life -= Scale < 0.2f ? 0 : 1;      // frozen shards hang in the air
            if (d.Life <= 0) Shards.RemoveAt(i);
        }
    }

    public static void Reset()
    {
        Scale = 1f; Flash = 0; Kills = 0; Word = null; wasFrozen = false; Shards.Clear();
    }

    public static void Update()
    {
        if (Mix.ReadyTick < 0) { Scale = 1f; return; }
        Main.GameZoomTarget = 1.6f;
        Player p = Mix.Host;
        float target = 1f;
        if (p != null && p.active && !p.dead) {
            float speed = Math.Max(Math.Abs(p.velocity.X) / 3.2f, Math.Abs(p.velocity.Y) / 5f);
            bool acting = p.itemAnimation > 0 || p.controlUseItem;
            target = Math.Clamp(0.05f + speed * 0.95f, 0.05f, 1f);
            if (acting) target = Math.Max(target, 0.7f);
        }
        // time stops fast, restarts a little slower
        Scale = MathHelper.Lerp(Scale, target, target < Scale ? 0.25f : 0.12f);

        bool frozen = Scale < 0.2f;
        if (frozen != wasFrozen && Mix.Tick - lastWord > 50) {
            wasFrozen = frozen;
            lastWord = Mix.Tick;
            Word = frozen ? "SUPER" : "HOT";
            WordStart = Mix.Tick;
            if (frozen && Mix.Tick - LastVoice > 60 * 14) { LastVoice = Mix.Tick; Mix.Sound("superhot", null, 0.8f); }
            if (frozen) Mix.Sound("slowmo", null, 0.6f);
        }
        if (Flash > 0) Flash--;
        UpdateShards();
    }

    /// <summary>Crystal shatter: shards fly, white flash, glassy sound.</summary>
    public static void Shatter(Vector2 center, int w, int h, bool big = true)
    {
        for (int i = 0; i < (big ? 36 : 12); i++) {
            int t = i % 3 == 0 ? DustID.GemRuby : (i % 3 == 1 ? DustID.RedTorch : DustID.Glass);
            Dust d = Dust.NewDustDirect(center - new Vector2(w / 2f, h / 2f), w, h, t, 0, 0, 0, i % 3 == 2 ? default : Color.Red, big ? 1.6f : 1.1f);
            d.velocity = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -2f);
            d.noGravity = i % 4 == 0;
        }
        AddShards(center, w, h, big ? 26 : 6, big ? 7f : 4f);
        if (big) {
            Flash = 7;
            Mix.Sound("shatter", center, 0.9f, Main.rand.NextFloat(-0.2f, 0.2f));
            Mix.Shake(5, 0.25);
        }
    }
}
