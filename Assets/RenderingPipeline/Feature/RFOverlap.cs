using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 叠加渲染特性
/// </summary>
public class RFOverlap : ScriptableRendererFeature {
    /// <summary> 渲染Event </summary>
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    /// <summary> 渲染通道 </summary>
    private RFOverlapPass overlapPass;

    public override void Create() {
        overlapPass = new RFOverlapPass();
        overlapPass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        renderer.EnqueuePass(overlapPass);
        Dispose();
    }
}
/// <summary>
/// 叠加渲染通道
/// </summary>
public class RFOverlapPass : ScriptableRenderPass {
    public const string ProfilerTag = "Overlap";
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        CommandBuffer command = CommandBufferPool.Get(ProfilerTag);

        OverlapHandle.RenderObjs.RemoveAll(obj => obj == null);
        for (int i = 0; i < OverlapHandle.RenderObjs.Count; i++) {
            DataOverlap obj = OverlapHandle.RenderObjs[i];
            if (!obj.isEnable) { continue; }
            command.DrawRenderer(obj.renderer, obj.material, 0, 0);
        }

        context.ExecuteCommandBuffer(command);
        CommandBufferPool.Release(command);
    }
}
/// <summary>
/// 叠加渲染数据
/// </summary>
public class DataOverlap {
    /// <summary> 是否启用 </summary>
    public bool isEnable;
    /// <summary> 渲染器 </summary>
    public Renderer renderer;
    /// <summary> 渲染材质 </summary>
    public Material material;
}
/// <summary>
/// 叠加渲染处理器
/// </summary>
public static class OverlapHandle {
    /// <summary> 渲染对象 </summary>
    public static List<DataOverlap> RenderObjs = new List<DataOverlap>();
    /// <summary> 添加到渲染队列 </summary>
    public static void Add(DataOverlap overlap) {
        if (RenderObjs.Contains(overlap)) { return; }
        RenderObjs.Add(overlap);
    }
    /// <summary> 移出渲染队列 </summary>
    public static void Remove(DataOverlap overlap) {
        if (!RenderObjs.Contains(overlap)) { return; }
        RenderObjs.Remove(overlap);
    }
}
