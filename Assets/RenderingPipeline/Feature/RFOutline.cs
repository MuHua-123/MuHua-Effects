using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 轮廓渲染特性
/// </summary>
public class RFOutline : ScriptableRendererFeature {
    [Tooltip("渲染对象")] public float size = 5;
    [Tooltip("渲染对象")] public Material unlit;
    [Tooltip("轮廓材质")] public Material outline;
    [Tooltip("混合颜色")] public Material color;
    /// <summary> 渲染Event </summary>
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    /// <summary> 渲染通道 </summary>
    private RFOutlinePass OutlineBlendRenderPass;
    public override void Create() {
        OutlineHandle.Size = size;
        OutlineHandle.Unlit = unlit;
        OutlineHandle.Outline = outline;
        OutlineHandle.Color = color;
        OutlineBlendRenderPass = new RFOutlinePass();
        OutlineBlendRenderPass.renderPassEvent = renderPassEvent;
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        OutlineBlendRenderPass.Setup(renderingData);
        renderer.EnqueuePass(OutlineBlendRenderPass);
        Dispose();
    }
}
/// <summary>
/// 轮廓渲染通道
/// </summary>
public class RFOutlinePass : ScriptableRenderPass {
    public const string ProfilerTag = "Outline";
    /// <summary> 临时纹理 </summary>
    public RTHandle TempRTHandel;
    /// <summary> 轮廓纹理 </summary>
    public RTHandle OutlineRTHandel;
    /// <summary> 渲染前设置 </summary>
    public void Setup(in RenderingData renderingData) {
        if (!OutlineHandle.IsValid) { return; }
        OutlineHandle.Outline.SetFloat("_Size", OutlineHandle.Size);
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = (int)DepthBits.None;
        RenderingUtils.ReAllocateIfNeeded(ref OutlineRTHandel, descriptor, name: "OutlineRT");
        RenderingUtils.ReAllocateIfNeeded(ref TempRTHandel, descriptor, name: "TempRT");
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        if (!OutlineHandle.IsValid) { return; }
        if (renderingData.cameraData.cameraType == CameraType.SceneView) { return; }
        if (renderingData.cameraData.cameraType == CameraType.Preview) { return; }
        CommandBuffer command = CommandBufferPool.Get(ProfilerTag);
        //在临时纹理上渲染物体的轮廓
        CoreUtils.SetRenderTarget(command, TempRTHandel);
        DrawRenderer(command, OutlineHandle.Unlit);
        OutlineHandle.Outline.SetTexture("_MainTex", TempRTHandel);
        Blitter.BlitTexture(command, TempRTHandel, OutlineRTHandel, OutlineHandle.Outline, 0);
        //轮廓+颜色 混合到源上
        OutlineHandle.Color.SetTexture("_MainTex", OutlineRTHandel);
        Blit(command, ref renderingData, OutlineHandle.Color);
        context.ExecuteCommandBuffer(command);
        CommandBufferPool.Release(command);
        TempRTHandel.Release();
        OutlineRTHandel?.Release();
    }
    public void DrawRenderer(CommandBuffer command, Material material) {
        for (int i = 0; i < OutlineHandle.Datas.Count; i++) {
            DataOutline outline = OutlineHandle.Datas[i];
            //无效数据排除队列
            if (outline == null || outline.RenderObjs == null) {
                OutlineHandle.Remove(outline); continue;
            }
            //如果有无效对象则排除队列
            if (!DrawRenderer(outline, command, material)) {
                OutlineHandle.Remove(outline);
            }
        }
    }
    public bool DrawRenderer(DataOutline outline, CommandBuffer command, Material material) {
        bool isValid = true;
        Renderer[] renderers = outline.RenderObjs;
        for (int i = 0; i < renderers.Length; i++) {
            Renderer renderer = renderers[i];
            if (renderer == null) { isValid = false; continue; }
            command.DrawRenderer(renderer, material, 0, 0);
        }
        return isValid;
    }
}
/// <summary>
/// 轮廓渲染数据
/// </summary>
public class DataOutline {
    /// <summary> 渲染对象列表 </summary>
    public Renderer[] RenderObjs;
}
/// <summary>
/// 轮廓渲染处理器
/// </summary>
public static class OutlineHandle {
    /// <summary> 轮廓大小 </summary>
    public static float Size = 5;
    /// <summary> 辅助材质 </summary>
    public static Material Unlit;
    /// <summary> 轮廓材质 </summary>
    public static Material Outline;
    /// <summary> 混合颜色 </summary>
    public static Material Color;
    /// <summary> 渲染对象 </summary>
    public static List<DataOutline> Datas = new List<DataOutline>();
    /// <summary> 是否有效 </summary>
    public static bool IsValid => Unlit != null && Outline != null && Color != null;
    /// <summary> 添加到渲染队列 </summary>
    public static void Add(DataOutline outline) {
        if (Datas.Contains(outline)) { return; }
        Datas.Add(outline);
    }
    /// <summary> 移出渲染队列 </summary>
    public static void Remove(DataOutline outline) {
        if (!Datas.Contains(outline)) { return; }
        Datas.Remove(outline);
    }
    /// <summary> 清空队列 </summary>
    public static void Clear() {
        if (Datas != null) { Datas.Clear(); }
        Datas = new List<DataOutline>();
    }
}