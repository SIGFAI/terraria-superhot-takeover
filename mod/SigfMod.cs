using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class SigfMod : ModSystem
{
    static int Count(int type) => Main.npc.Count(n => n.active && n.type == type);

    static void SpawnMan(double minTiles, double maxTiles)
    {
        Player p = Mix.Host;
        for (int i = 0; i < 6; i++) {
            Vector2 g = Mix.Ground(maxTiles);
            if (Vector2.Distance(g, p.Bottom) > minTiles * 16) { Mix.Spawn<RedMan>(g); return; }
        }
    }

    static void FireAll()
    {
        Player p = Mix.Host;
        foreach (NPC n in Main.npc.Where(n => n.active && n.ModNPC is RedMan)) {
            Vector2 muzzle = n.Center + new Vector2(n.direction * 20, -6);
            Mix.Shoot<HotBullet>(muzzle, Mix.Aim(muzzle, p.Center, 6.5f), 15, 2f, true);
            Mix.Sound("gunshot", muzzle, 0.5f, 0.1f);
        }
    }

    public override void OnWorldLoad()
    {
        // always on: a few red crystal gunmen keep coming
        Mix.Every(3.5, () => { if (Count(ModContent.NPCType<RedMan>()) < 6) SpawnMan(8, 20); });
        Mix.After(0.5, () => { Mix.Arm<HotKatana>(); SpawnMan(10, 16); });
        Mix.Every(14, () => { if (Mix.IsDemo) return;
            if (Mix.Host.HeldItem.type == ModContent.ItemType<HotKatana>()) Mix.Arm<HotPistol>(); else Mix.Arm<HotKatana>();
        });

        Mix.Demo(0.5, () => { Hot.Title("SUPERHOT", "TIME MOVES ONLY WHEN YOU MOVE", 4); SpawnMan(9, 12); SpawnMan(12, 16); });
        Mix.Demo(4, () => { Mix.Autopilot = false; Mix.AutoAttack = false; Hot.Title("STAND STILL", "TIME STOPS. BULLETS HANG IN THE AIR", 5); });
        Mix.Demo(6, () => FireAll());
        Mix.Demo(8.5, () => FireAll());
        Mix.Demo(11, () => { Mix.Autopilot = true; Mix.AutoAttack = true; Hot.Title("MOVE", "TIME FLOWS AGAIN", 3); });
        Mix.Demo(24, () => {
            Player p = Mix.Host;
            Mix.Autopilot = false; Mix.AutoAttack = false;
            Vector2 w = Mix.Ground(p.Bottom + new Vector2(p.direction * 5 * 16, 0), 0);
            wall = w;
            for (int i = 1; i <= 3; i++) Mix.PlaceTile(w - new Vector2(0, i * 16 - 8), TileID.GrayBrick);
            Mix.Spawn<RedMan>(Mix.Ground(w + new Vector2(p.direction * 7 * 16, 0), 0));
            Mix.Spawn<RedMan>(Mix.Ground(w + new Vector2(p.direction * 10 * 16, 0), 0));
            Hot.Title("BLOCKS STOP BULLETS", "", 3);
        });
        Mix.Demo(25.5, () => FireAll());
        Mix.Demo(28, () => FireAll());
        Mix.Demo(31, () => { Mix.Autopilot = true; Mix.AutoAttack = true; });
        Mix.Demo(37, () => { for (int i = 1; i <= 3; i++) Mix.KillTile(wall - new Vector2(0, i * 16 - 8)); });
        Mix.Demo(39, () => {
            Mix.Arm<HotPistol>();
            for (int i = 0; i < 3; i++) Mix.Spawn(NPCID.Zombie, Mix.Ground(16));
            Hot.Title("EVERYTHING HOSTILE TURNS RED", "", 3);
        });
        Mix.Demo(52, () => { Mix.Arm<HotKatana>(); SpawnMan(6, 9); SpawnMan(8, 12); Hot.Title("SLASH THE BULLETS BACK", "", 3); });
        Mix.Demo(55, () => FireAll());
        Mix.Demo(60, () => FireAll());
    }

    static Vector2 wall;
}
