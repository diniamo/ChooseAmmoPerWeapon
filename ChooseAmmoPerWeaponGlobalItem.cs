using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ChooseAmmoPerWeapon
{
    public class ChooseAmmoPerWeaponGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ChooseAmmoPerWeapon.hoverItem != null)
            {
                Item ammo;
                if (ChooseAmmoPerWeaponModPlayer.LocalAssignedAmmo.TryGetValue(ChooseAmmoPerWeapon.hoverItem, out ammo))
                {
                    if (tooltips.Last().Text.ToLower().Contains("research"))
                        tooltips.Insert(tooltips.Count - 1, new TooltipLine(ChooseAmmoPerWeapon.instance, "AssignedAmmo", $"[c/808080:Using {ammo.Name}]"));
                    else
                        tooltips.Add(new TooltipLine(ChooseAmmoPerWeapon.instance, "AssignedAmmo", $"[c/808080:Using {ammo.Name}]"));
                }
            }

            base.ModifyTooltips(item, tooltips);
        }
    }
}
