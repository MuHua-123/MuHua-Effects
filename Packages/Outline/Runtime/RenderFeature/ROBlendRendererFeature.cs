using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua.Outline {
    public class ROBlendRendererFeature : ScriptableRendererFeature {
        public float OutlineSize = 5;
        public Material OutlineColor;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        private ROOutlineBlendRenderPass OutlineBlendRenderPass;
        public override void Create() {
            OutlineBlendRenderPass = new ROOutlineBlendRenderPass();
            OutlineBlendRenderPass.renderPassEvent = renderPassEvent;
            ROModel.I.OutlineColor = OutlineColor;
            Debug.Log("sss");
        }
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
            OutlineBlendRenderPass.Setup();
            renderer.EnqueuePass(OutlineBlendRenderPass);
            Dispose();
        }
    }
    public class ROOutlineBlendRenderPass : ScriptableRenderPass {
        public const string ProfilerTag = "OutlineBlend";
        public const string OutlineBlend_Shader = "Shader Graphs/OutlineBlend";

        public void Setup() {
            if (ROModel.I.OutlineColor == null) {
                Shader outlineBlend = Shader.Find(OutlineBlend_Shader);
                ROModel.I.OutlineColor = CoreUtils.CreateEngineMaterial(outlineBlend);
            }
            ROModel.I.OutlineColor.SetTexture("_MainTex", ROModel.I.OutlineRT);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer command = CommandBufferPool.Get(ProfilerTag);
            Blit(command, ref renderingData, ROModel.I.OutlineColor);
            context.ExecuteCommandBuffer(command);
            CommandBufferPool.Release(command);
        }
    }
}