using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace Config.cs
{
    public static class ILHelper
    {

        // こういうヘルパーメソッド用意しておくと便利
        public static void EmitPop(this ILGenerator il, int count)
        {
            for (int i = 0; i < count; i++)
            {
                il.Emit(OpCodes.Pop);
            }
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ret);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LoadLocalA_0(this ILGenerator il)
        {
            il.Emit(OpCodes.Ldloca_S, 0);
        }
    }
}
