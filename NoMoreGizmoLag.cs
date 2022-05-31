using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BaseX;
using FrooxEngine;
using HarmonyLib;
using NeosModLoader;

namespace NoMoreGizmoLag
{
    public class NoMoreGizmoLag : NeosMod
    {
        public override string Name => "NoMoreGizmoLag";
        public override string Author => "Fro Zen";
        public override string Version => "1.0.0";

        public override void OnEngineInit()
        {
            var harmony = new Harmony(Name);
            var gizmo = typeof(SlotGizmo).GetMethod("OnCommonUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            var trans = typeof(NoMoreGizmoLag).GetMethod(nameof(SlotGizmoUpdateTranspiler));
            harmony.Patch(gizmo, transpiler: new HarmonyMethod(trans));
            Msg("Removed bounding box check from gizmos");
        }
        public static IEnumerable<CodeInstruction> SlotGizmoUpdateTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var replaceWith = codes.First(i => 
                i.opcode == OpCodes.Call && i.operand.ToString().Contains("Empty"));
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Call ||
                    !codes[i].operand.ToString().Contains("ComputeBoundingBox")) continue;
                //replace ComputeBoundingBox with an empty bounding box
                codes.RemoveAt(i);
                codes.Insert(i, new CodeInstruction(replaceWith));
                //prevent invalid il error
                codes.RemoveRange(i-5, 5);
            }
            return codes;
        }
    }
}
