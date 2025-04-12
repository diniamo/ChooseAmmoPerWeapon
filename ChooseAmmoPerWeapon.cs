using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChooseAmmoPerWeapon
{
    public class ChooseAmmoPerWeapon : Mod
    {
        public static ChooseAmmoPerWeapon instance;

        // Static so the instance doesn't have to be retrieved in IL
        public static Item hoverItem;

        public override void Load()
        {
            instance = this;

            // We use our own HoverItem variable as the Item object is Cloned in Terraria code
            IL_ItemSlot.MouseHover_ItemArray_int_int += il =>
            {
                var c = new ILCursor(il);

                c.GotoNext(MoveType.Before, i => i.MatchCallvirt(typeof(Item).GetMethod(nameof(Item.Clone))));

                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Stsfld, typeof(ChooseAmmoPerWeapon).GetField(nameof(ChooseAmmoPerWeapon.hoverItem), BindingFlags.Public | BindingFlags.Static));
            };

            // Assign functionality
            On_ItemSlot.OverrideLeftClick += OverrideLeftClick;
            // Unassign functionality
            On_ItemSlot.RightClick_ItemArray_int_int += RightClick;
            // Draw ammo icon next to weapons
            On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlot_Draw;
            // Override used ammo with set
            On_Player.ChooseAmmo += ChooseAmmo;

            base.Load();
        }

        public override void Unload()
        {
            instance = null;
            hoverItem = null;

            base.Unload();
        }

        private bool OverrideLeftClick(On_ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot)
        {
            Item clicked = inv[slot];
            Item holding = Main.mouseItem;

            if (holding.type != ItemID.None && clicked.type != ItemID.None)
            {
                if (clicked.useAmmo > 0 && holding.ammo > 0 && ItemLoader.CanChooseAmmo(clicked, holding, Main.LocalPlayer))
                {
                    ChooseAmmoPerWeaponModPlayer.LocalAssignedAmmo[clicked] = holding;
                    return true;
                }
            }

            return orig.Invoke(inv, context, slot);
        }

        private void RightClick(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            Item item = inv[slot];
            if (Main.mouseRight && !ChooseAmmoPerWeaponModPlayer.LocalAssignedAmmo.Remove(item))
                orig.Invoke(inv, context, slot);
        }

        private void ItemSlot_Draw(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
        {
            orig.Invoke(spriteBatch, inv, context, slot, position, lightColor);

            // Some mods use this function to draw their icon in the mod configuration menu,
            // in which case the slot will be invalid.
            if (slot < 0 || slot > inv.Length - 1)
                return;

            Item ammo;
            if (ChooseAmmoPerWeaponModPlayer.LocalAssignedAmmo.TryGetValue(inv[slot], out ammo))
            {
                Texture2D texture = TextureAssets.Item[ammo.type].Value;
                position.X += TextureAssets.InventoryBack.Value.Width * Main.inventoryScale;

                // Margin
                position.X -= 5;
                position.Y += 5;

                spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width * Main.inventoryScale, 0f), 0.9f * Main.inventoryScale, SpriteEffects.None, 0f);
            }
        }

        private Item ChooseAmmo(On_Player.orig_ChooseAmmo orig, Player self, Item weapon)
        {
            Item ammo;
            if (ChooseAmmoPerWeaponModPlayer.LocalAssignedAmmo.TryGetValue(weapon, out ammo))
                return ammo;
            else
                return orig.Invoke(self, weapon);
        }
    }
}