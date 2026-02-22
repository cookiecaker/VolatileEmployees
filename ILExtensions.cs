using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace volatileEmployees
{
    // class for methods to make messing with CodeInstructions easier 
    internal static class ILExtensions
    {
        internal static void Add(this List<CodeInstruction> instructions, OpCode op, object operand = null)
        {
            instructions.Add(new CodeInstruction(op, operand));
        }

        internal static void PrintCodeList(this List<CodeInstruction> codes)
        {
            Plugin.mls.LogDebug("\n\nNew codes:\n");
            foreach (CodeInstruction code in codes) {
                Plugin.mls.LogDebug(code);
            }
        }
    }
}
