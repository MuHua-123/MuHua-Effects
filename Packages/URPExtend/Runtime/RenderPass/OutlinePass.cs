using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 轮廓渲染设置
	/// </summary>
	public class OutlineSettings {
		/// <summary> 辅助材质 </summary>
		public Material unlit;
		/// <summary> 轮廓材质 </summary>
		public Material outline;
		/// <summary> 混合材质 </summary>
		public Material outlineBlend;
		/// <summary> 渲染对象 </summary>
		public Renderer[] renderObjs = new Renderer[0];
		/// <summary> 渲染事件 </summary>
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
	}
	/// <summary>
	/// 轮廓渲染通道
	/// </summary>
	public class OutlinePass : ScriptableRenderPass {
		public const string ProfilerTag = "Outline";

		/// <summary> 渲染设置 </summary>
		public OutlineSettings settings;

		/// <summary> 临时纹理 </summary>
		public RTHandle tempRTHandle;
		/// <summary> 轮廓纹理 </summary>
		public RTHandle outlineRTHandle;

		/// <summary> 渲染前设置 </summary>
		public void Setup(OutlineSettings settings, in RenderingData renderingData) {
			this.settings = settings;
			renderPassEvent = settings.renderPassEvent;
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = (int)DepthBits.None;
			RenderingUtils.ReAllocateIfNeeded(ref outlineRTHandle, descriptor, name: "OutlineRT");
			RenderingUtils.ReAllocateIfNeeded(ref tempRTHandle, descriptor, name: "TempRT");
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.cameraType == CameraType.SceneView || renderingData.cameraData.cameraType == CameraType.Preview) return;

			CommandBuffer command = CommandBufferPool.Get(ProfilerTag);

			// 设置渲染目标为tempRTHandle
			CoreUtils.SetRenderTarget(command, tempRTHandle);
			// 清除纹理内容
			CoreUtils.ClearRenderTarget(command, ClearFlag.All, Color.clear);
			// 绘制渲染物体
			DrawRenderer(command, settings.unlit);
			// 设置tempRTHandle为outline的主纹理
			settings.outline.SetTexture("_MainTex", tempRTHandle);
			// 渲染出轮廓
			Blitter.BlitTexture(command, tempRTHandle, outlineRTHandle, settings.outline, 0);

			// 设置outlineRTHandle为blend的主纹理
			settings.outlineBlend.SetTexture("_MainTex", outlineRTHandle);
			// 把缓冲区的内容渲染到renderingData
			Blit(command, ref renderingData, settings.outlineBlend, 0);

			// 执行缓冲区
			context.ExecuteCommandBuffer(command);
			// 释放
			CommandBufferPool.Release(command);
			tempRTHandle.Release();
			outlineRTHandle?.Release();
		}

		public void DrawRenderer(CommandBuffer command, Material material) {
			for (int i = 0; i < settings.renderObjs.Length; i++) {
				Renderer renderer = settings.renderObjs[i];
				if (renderer == null) { continue; }

				// 遍历所有的子网格
				for (int subMeshIndex = 0; subMeshIndex < renderer.sharedMaterials.Length; subMeshIndex++) {
					command.DrawRenderer(renderer, material, subMeshIndex, 0);
				}
			}
		}

	}
}