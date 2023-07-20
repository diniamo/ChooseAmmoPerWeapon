using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;

namespace ChooseAmmoPerWeapon
{
    public class Utils
    {
        private Utils() { }

        public static int AddVariable(ILContext context, Type type)
        {
            context.Body.Variables.Add(new VariableDefinition(context.Import(type)));
            return context.Body.Variables.Count - 1;
        }
    }
}
