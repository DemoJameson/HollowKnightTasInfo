using System;
using Assembly_CSharp.TasInfo.mm.Source;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;

// ReSharper disable All

public class patch_PlayMakerUnity2DProxy : PlayMakerUnity2DProxy {
    [MonoModIgnore]
    [PatchStart]
    public extern new void Start();

    private void OnColliderCreate() {
        if (PlayMakerUnity2d.isAvailable()) {
            TasInfo.OnColliderCreate(gameObject);
        }
    }
}

namespace MonoMod {
    [MonoModCustomAttribute(nameof(MonoModRules.PatchStart))]
    internal class PatchStartAttribute : Attribute { }

    static partial class MonoModRules {
        public static void PatchStart(MethodDefinition method, CustomAttribute attrib) {
            MethodDefinition methodDefinition = method.DeclaringType.FindMethod("System.Void OnColliderCreate()");

            Mono.Collections.Generic.Collection<Instruction> instrs = method.Body.Instructions;
            ILProcessor il = method.Body.GetILProcessor();

            instrs.Insert(0, il.Create(OpCodes.Ldarg_0));
            instrs.Insert(1, il.Create(OpCodes.Call, methodDefinition));
        }
    }
}