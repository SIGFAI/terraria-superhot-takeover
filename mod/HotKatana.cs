using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class HotKatana : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 44; Item.height = 38;
        Item.damage = 30; Item.DamageType = DamageClass.Melee; Item.knockBack = 5f;
        Item.useTime = 15; Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing; Item.autoReuse = true;
        Item.UseSound = Mix.Style("slash", 0.8f);
        Item.rare = ItemRarityID.Red; Item.value = Item.sellPrice(gold: 1);
        Item.scale = 1.3f;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        Dust d = Dust.NewDustDirect(hitbox.TopLeft(), hitbox.Width, hitbox.Height, DustID.RedTorch, 0, 0, 0, default, 1.5f);
        d.noGravity = true; d.velocity *= 0.3f;
    }
}
