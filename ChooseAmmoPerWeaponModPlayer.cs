using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ChooseAmmoPerWeapon
{
    public class ChooseAmmoPerWeaponModPlayer : ModPlayer
    {
        public static Dictionary<Item, Item> LocalAssignedAmmo => Main.LocalPlayer.GetModPlayer<ChooseAmmoPerWeaponModPlayer>().assignedAmmo;

        public Dictionary<Item, Item> assignedAmmo;

        public override void Initialize()
        {
            assignedAmmo = new Dictionary<Item, Item>();

            base.Initialize();
        }

        public override void LoadData(TagCompound tag)
        {
            Item[] inventory = this.Player.inventory;

            // Initialized is always called before this function, so assignedAmmo is cleared there
            var keys = tag.GetList<int>("assignedAmmoKeys");
            var values = tag.GetList<int>("assignedAmmoValues");
            // It may be worth adding sanity checks for
            // - keys - useAmmo
            // - values - ammo != AmmoID.None
            // but as far as my logic goes, the current saving logic should not be able to miss.
            foreach (var (key, value) in keys.Zip(values))
                assignedAmmo[inventory[key]] = inventory[value];

            base.LoadData(tag);
        }

        // This is slow, but I couldn't find a better way
        // Tracking the slots at runtime doesn't sound reliable
        private List<int> ItemsToIndicies(IEnumerable<Item> iterator)
        {
            Item[] inventory = this.Player.inventory;
            var indicies = new List<int>();

            foreach (var item in iterator)
            {
                for (int i = 0; i < inventory.Length; i++)
                {
                    if (inventory[i] == item)
                    {
                        indicies.Add(i);
                        break;
                    }
                }
            }

            return indicies;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["assignedAmmoKeys"] = ItemsToIndicies(assignedAmmo.Keys);
            tag["assignedAmmoValues"] = ItemsToIndicies(assignedAmmo.Values);

            base.SaveData(tag);
        }

        public override void Unload()
        {
            assignedAmmo = null;

            base.Unload();
        }
    }
}
