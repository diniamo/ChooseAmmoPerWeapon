using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChooseAmmoPerWeapon
{
	public class ChooseAmmoPerWeapon : Mod
	{
        public static ChooseAmmoPerWeapon Instance => ModContent.GetInstance<ChooseAmmoPerWeapon>();

        public static Dictionary<Item, Item> AssignedAmmo;

        public override void Load()
        {
            AssignedAmmo = new Dictionary<Item, Item>();

            // Keeps the same item objects used by the inventory for Main.HoverItem (instead of Cloning)
            // Might break stuff

            IL_ItemSlot.MouseHover_ItemArray_int_int += il =>
            {
                var c = new ILCursor(il);

                c.GotoNext(i => i.MatchCallvirt(typeof(Item).GetMethod(nameof(Item.Clone))));
                c.Remove();
            };

            // This achieves the save as the ChooseAmmo parts below, but I spent quite a bit of time on this, so I have no heart to delete it
            /*IL.Terraria.Player.ChooseAmmo += il =>
            {
                var c = new ILCursor(il);
                var ammoVar = Utils.AddVariable(il, typeof(Item));
                var endLabel = c.DefineLabel();

                c.Emit(Ldsfld, typeof(ChooseAmmoPerWeapon).GetField(nameof(ChooseAmmoPerWeapon.AssignedAmmo), BindingFlags.Public | BindingFlags.Static));
                c.Emit(Ldarg_1);
                c.Emit(Ldloca, ammoVar);
                c.Emit(Callvirt, typeof(Dictionary<Item, Item>).GetMethod(nameof(Dictionary<Item, Item>.TryGetValue)));
                c.Emit(Brfalse, endLabel);

                c.Emit(Ldloc, ammoVar);
                c.Emit(Ret);

                c.MarkLabel(endLabel);
            };*/

            // Assign functionality

            On_ItemSlot.OverrideLeftClick += OverrideLeftClick;
            // Unassign functionality
            On_ItemSlot.RightClick_ItemArray_int_int += RightClick;
            // Draw ammo icon next to weapons
            On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += ItemSlot_Draw;
            // Main mod functionality
            On_Player.ChooseAmmo += ChooseAmmo;

            base.Load();
        }

        private void RightClick(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            Item item = inv[slot];
            if (Main.mouseRight && !AssignedAmmo.Remove(item))
                orig.Invoke(inv, context, slot);
        }

        private Item ChooseAmmo(On_Player.orig_ChooseAmmo orig, Player self, Item weapon)
        {
            Item ammo;
            if (AssignedAmmo.TryGetValue(weapon, out ammo))
                return ammo;
            else
                return orig.Invoke(self, weapon);
        }

        public override void Unload()
        {
            AssignedAmmo = null;

            base.Unload();
        }

        private void ItemSlot_Draw(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, Color lightColor)
        {
            orig.Invoke(spriteBatch, inv, context, slot, position, lightColor);

            Item ammo;
            if (ChooseAmmoPerWeapon.AssignedAmmo.TryGetValue(inv[slot], out ammo))
            {
                Texture2D texture = TextureAssets.Item[ammo.type].Value;
                position.X += TextureAssets.InventoryBack.Value.Width * Main.inventoryScale;

                // Margin
                position.X -= 5;
                position.Y += 5;

                spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width * Main.inventoryScale, 0f), 0.9f * Main.inventoryScale, SpriteEffects.None, 0f);
            }
        }

        private static bool OverrideLeftClick(On_ItemSlot.orig_OverrideLeftClick orig, Item[] inv, int context, int slot)
        {
            Item clicked = inv[slot];
            Item holding = Main.mouseItem;

            if (holding.type != ItemID.None && clicked.type != ItemID.None)
            {
                if (clicked.useAmmo > 0 && holding.ammo > 0 && ItemLoader.CanChooseAmmo(clicked, holding, Main.LocalPlayer))
                {
                    AssignedAmmo[clicked] = holding;
                    return true;
                }
            }

            orig.Invoke(inv, context, slot);
            return false;
        }
    }
}