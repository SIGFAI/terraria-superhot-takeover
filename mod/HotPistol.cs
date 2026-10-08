using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sigf.Content;

public class HotPistol : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 28; Item.height = 13;
        Item.damage = 30; Item.DamageType = DamageClass.Ranged; Item.knockBack = 3f;
        Item.useTime = 20; Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot; Item.autoReuse = true; Item.noMelee = true;
        Item.UseSound = Mix.Style("gunshot", 0.7f);
        Item.rare = ItemRarityID.Red; Item.value = Item.sellPrice(gold: 1);
        Item.shoot = ModContent.ProjectileType<HotBullet>(); Item.shootSpeed = 13f;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-2, 0);
}
