using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Sigf.Content;

/// <summary>Draws the SUPERHOT look: a white-washed world, enemies and bullets in glowing red on top, and the SUPER / HOT words.</summary>
public class HotSystem : ModSystem
{
    static readonly Dictionary<(Texture2D, int), Texture2D> cache = new();

    public override void PostUpdatePlayers() => Hot.Update();

    public override void OnWorldUnload() { Hot.Reset(); }

    public override void Unload() { cache.Clear(); }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Insert(0, new LegacyGameInterfaceLayer("Sigf: Wash", () => { DrawWorldOverlay(); return true; }, InterfaceScaleType.None));
        layers.Add(new LegacyGameInterfaceLayer("Sigf: Words", () => { DrawWords(); return true; }, InterfaceScaleType.UI));
    }

    /// <summary>mode 0 = red crystal tint of the sprite, 1 = flat white silhouette.</summary>
    static Texture2D Recolor(Texture2D src, int mode)
    {
        if (cache.TryGetValue((src, mode), out var t)) return t;
        var data = new Color[src.Width * src.Height];
        src.GetData(data);
        for (int i = 0; i < data.Length; i++) {
            Color c = data[i];
            if (c.A == 0) continue;
            if (mode == 1) { data[i] = new Color(255, 245, 245, c.A); continue; }
            float l = (c.R * 0.3f + c.G * 0.59f + c.B * 0.11f) / 255f;
            l = Math.Clamp(l * 1.15f, 0f, 1f);
            Color dark = new Color(95, 0, 12), mid = new Color(235, 22, 30), hi = new Color(255, 150, 140);
            Color o = l < 0.6f ? Color.Lerp(dark, mid, l / 0.6f) : Color.Lerp(mid, hi, (l - 0.6f) / 0.4f);
            float a = c.A / 255f;
            data[i] = new Color((int)(o.R * a), (int)(o.G * a), (int)(o.B * a), c.A);
        }
        var res = new Texture2D(Main.graphics.GraphicsDevice, src.Width, src.Height);
        res.SetData(data);
        cache[(src, mode)] = res;
        return res;
    }

    static void DrawWorldOverlay()
    {
        if (Mix.ReadyTick < 0) return;
        SpriteBatch sb = Main.spriteBatch;
        Texture2D px = TextureAssets.MagicPixel.Value;
        // the white wash
        sb.Draw(px, new Rectangle(0, 0, Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height), new Color(244, 245, 250) * 0.68f);
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        sb.Draw(px, new Rectangle(0, 0, Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height), new Color(38, 38, 42));
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        if (Hot.Flash > 0) sb.Draw(px, new Rectangle(0, 0, Main.graphics.GraphicsDevice.Viewport.Width, Main.graphics.GraphicsDevice.Viewport.Height), Color.White * (Hot.Flash / 7f));
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        try {
            // a pale blueprint grid fills the empty ground below, so the white world reads as a stage
            float gy = Mix.Center.Y + 24f;
            float x0 = (float)Math.Floor(Main.screenPosition.X / 96f) * 96f;
            float yTop = Math.Max(gy, Main.screenPosition.Y), yBot = Main.screenPosition.Y + 1400f;
            Color gc = new Color(120, 135, 175) * 0.32f;
            for (int k = 0; k < 28; k++) {
                float wx = x0 + k * 96f;
                sb.Draw(px, new Rectangle((int)(wx - Main.screenPosition.X), (int)(yTop - Main.screenPosition.Y), 2, (int)(yBot - yTop)), gc);
            }
            for (float wy = (float)Math.Ceiling(yTop / 96f) * 96f; wy < yBot; wy += 96f)
                sb.Draw(px, new Rectangle((int)(x0 - Main.screenPosition.X), (int)(wy - Main.screenPosition.Y), 28 * 96, 2), gc);

            // the player keeps his colors
            foreach (Player p in Main.player)
                if (p.active && !p.dead)
                    Main.PlayerRenderer.DrawPlayer(Main.Camera, p, p.position, p.fullRotation, p.fullRotationOrigin, 0f, 1f);

            foreach (NPC n in Main.npc) {
                if (!n.active || !(n.CanBeChasedBy() || n.boss) || n.friendly) continue;
                Main.instance.LoadNPC(n.type);
                Texture2D tex = TextureAssets.Npc[n.type].Value;
                bool mine = n.ModNPC is RedMan;
                var gn = n.GetGlobalNPC<HotGlobalNPC>();
                if (gn.Flash > 0) tex = Recolor(tex, 1);
                else if (!mine) tex = Recolor(tex, 0);
                int fh = n.frame.Height > 0 ? n.frame.Height : tex.Height / Math.Max(1, Main.npcFrameCount[n.type]);
                Rectangle fr = n.frame.Width > 0 ? n.frame : new Rectangle(0, 0, tex.Width, fh);
                Vector2 origin = new Vector2(fr.Width / 2f, fr.Height / 2f);
                float off = mine ? 0f : 4f;
                Vector2 pos = new Vector2(n.Center.X, n.Bottom.Y + off - fr.Height * n.scale / 2f + n.gfxOffY) - Main.screenPosition;
                var fx = n.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                sb.Draw(tex, pos, fr, Color.White, n.rotation, origin, n.scale, fx, 0f);
            }

            Texture2D bullet = ModContent.Request<Texture2D>("Sigf/Content/HotBullet").Value;
            foreach (var d in Hot.Shards) {
                float al = Math.Min(1f, d.Life / 30f);
                sb.Draw(bullet, d.Pos - Main.screenPosition, null, Color.White * al, d.Rot, new Vector2(bullet.Width / 2f, bullet.Height / 2f), d.Size * 1.1f, SpriteEffects.None, 0f);
            }
            int bt = ModContent.ProjectileType<HotBullet>();
            foreach (Projectile pr in Main.projectile) {
                if (!pr.active || pr.type != bt) continue;
                Vector2 o = new Vector2(bullet.Width / 2f, bullet.Height / 2f);
                for (int k = pr.oldPos.Length - 1; k >= 0; k--) {
                    if (pr.oldPos[k] == Vector2.Zero) continue;
                    float f = 1f - k / (float)pr.oldPos.Length;
                    sb.Draw(bullet, pr.oldPos[k] + pr.Size / 2f - Main.screenPosition, null, Color.White * (0.35f * f), pr.rotation, o, pr.scale * (0.6f + 0.4f * f), SpriteEffects.None, 0f);
                }
                sb.Draw(bullet, pr.Center - Main.screenPosition, null, Color.White, pr.rotation, o, pr.scale * 1.35f, SpriteEffects.None, 0f);
            }
        } finally {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        }
    }

    static void DrawWords()
    {
        if (Mix.ReadyTick < 0) return;
        SpriteBatch sb = Main.spriteBatch;
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        Texture2D px = TextureAssets.MagicPixel.Value;
        float w = Main.graphics.GraphicsDevice.Viewport.Width, h = Main.graphics.GraphicsDevice.Viewport.Height;

        // time gauge
        int bw = 260, bh = 14, bx = (int)(w / 2 - bw / 2), by = (int)(h - 84);
        sb.Draw(px, new Rectangle(bx - 3, by - 3, bw + 6, bh + 6), Color.Black * 0.85f);
        sb.Draw(px, new Rectangle(bx, by, (int)(bw * Hot.Scale), bh), Color.Lerp(new Color(255, 30, 30), Color.White, Hot.Scale * 0.5f));
        string label = Hot.Scale < 0.2f ? "TIME: FROZEN" : "TIME: " + (int)(Hot.Scale * 100) + "%";
        Vector2 ls = FontAssets.MouseText.Value.MeasureString(label);
        Utils.DrawBorderString(sb, label, new Vector2(w / 2 - ls.X / 2, by + 18), Hot.Scale < 0.2f ? new Color(255, 90, 90) : Color.White, 1f);

        string kl = "SHATTERED: " + Hot.Kills;
        Vector2 ks = FontAssets.MouseText.Value.MeasureString(kl) * 1.3f;
        Utils.DrawBorderString(sb, kl, new Vector2(w / 2 - ks.X / 2, by + 40), new Color(255, 70, 70), 1.3f);

        if (Hot.TitleBig != null && Mix.Tick < Hot.TitleUntil) {
            float age = (Hot.TitleUntil - Mix.Tick) / 60f;
            float a = Math.Min(1f, age * 2f);
            var font = FontAssets.DeathText.Value;
            Vector2 s1 = font.MeasureString(Hot.TitleBig) * 1.25f;
            Vector2 p1 = new Vector2(w / 2 - s1.X / 2, h * 0.1f);
            float tw = Math.Max(s1.X, FontAssets.MouseText.Value.MeasureString(Hot.TitleSmall).X * 1.4f) + 80;
            sb.Draw(px, new Rectangle((int)(w / 2 - tw / 2), (int)(p1.Y + 4), (int)tw, (int)(s1.Y * 0.78f + (Hot.TitleSmall.Length > 0 ? 34 : 0))), Color.White * (0.9f * a));
            Utils.DrawBorderStringBig(sb, Hot.TitleBig, p1, new Color(20, 20, 20) * a, 1.25f);
            if (Hot.TitleSmall.Length > 0) {
                Vector2 s2 = FontAssets.MouseText.Value.MeasureString(Hot.TitleSmall) * 1.4f;
                Utils.DrawBorderString(sb, Hot.TitleSmall, new Vector2(w / 2 - s2.X / 2, p1.Y + s1.Y * 0.8f), new Color(235, 20, 25) * a, 1.4f);
            }
        }

        if (Hot.Word != null) {
            float age = (Mix.Tick - Hot.WordStart) / 60f;
            if (age < 1.1f) {
                float a = age < 0.8f ? 1f : 1f - (age - 0.8f) / 0.3f;
                float sc = 1.9f + 0.5f * (float)Math.Exp(-age * 9);
                var font = FontAssets.DeathText.Value;
                Vector2 s = font.MeasureString(Hot.Word) * sc;
                Vector2 pos = new Vector2(w / 2 - s.X / 2, h * 0.33f - s.Y / 2);
                sb.Draw(px, new Rectangle((int)(pos.X - 20), (int)(pos.Y + s.Y * 0.18f), (int)(s.X + 40), (int)(s.Y * 0.7f)), Color.White * (0.85f * a));
                Color c = (Hot.Word == "SUPER" ? new Color(20, 20, 20) : new Color(235, 20, 25)) * a;
                Utils.DrawBorderStringBig(sb, Hot.Word, pos, c, sc);
            }
        }
    }
}
