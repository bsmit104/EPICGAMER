using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

class OutlinePass : CustomPass
{
    public Material outlineMaterial;

    protected override void Execute(CustomPassContext ctx)
    {
        if (outlineMaterial == null) return;
        HDUtils.DrawFullScreen(ctx.cmd, outlineMaterial, ctx.cameraColorBuffer);
    }
}