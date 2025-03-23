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
            ChooseAmmoPerWeapon instance = ChooseAmmoPerWeapon.instance;

            if (instance.hoverItem != null)
            {
                Item ammo;
                if (instance.assignedAmmo.TryGetValue(instance.hoverItem, out ammo))
                {
                    if (tooltips.Last().Text.ToLower().Contains("research"))
                        tooltips.Insert(tooltips.Count - 1, new TooltipLine(instance, "AssignedAmmo", $"[c/808080:Using {ammo.Name}]"));
                    else
                        tooltips.Add(new TooltipLine(instance, "AssignedAmmo", $"[c/808080:Using {ammo.Name}]"));
                }
            }

            base.ModifyTooltips(item, tooltips);
        }
    }
}
