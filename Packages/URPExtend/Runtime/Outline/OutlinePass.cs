using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 轮廓 - 渲染通道
	/// </summary>
	public class OutlinePass : ScriptableRenderPass {
		/// <summary> 分析器标签 </summary>
		public const string ProfilerTag = "Outline";

		/// <summary> 辅助材质 </summary>
		public Material unlit;
		/// <summary> 轮廓材质 </summary>
		public Material outline;
		/// <summary> 混合材质 </summary>
		public Material outlineBlend;
		/// <summary> 渲染对象 </summary>
		public List<Renderer> renderObjs = new();

		/// <summary> 临时纹理 </summary>
		public RTHandle tempRTHandle;
		/// <summary> 轮廓纹理 </summary>
		public RTHandle outlineRTHandle;

		/// <summary> 渲染前设置 </summary>
		public void Setup(in RenderingData renderingData) {
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = (int)DepthBits.None;
			RenderingUtils.ReAllocateIfNeeded(ref outlineRTHandle, descriptor, name: "OutlineRT");
			RenderingUtils.ReAllocateIfNeeded(ref tempRTHandle, descriptor, name: "OutlineTempRT");
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.cameraType == CameraType.SceneView) { return; }
			if (renderingData.cameraData.cameraType == CameraType.Preview) { return; }

			CommandBuffer command = CommandBufferPool.Get(ProfilerTag);

			// 设置渲染目标为tempRTHandle
			CoreUtils.SetRenderTarget(command, tempRTHandle);
			// 清除纹理内容
			CoreUtils.ClearRenderTarget(command, ClearFlag.All, Color.clear);
			// 绘制渲染物体
			DrawRenderer(command, unlit);
			// 设置tempRTHandle为outline的主纹理
			outline.mainTexture = tempRTHandle;
			// 渲染出轮廓
			Blitter.BlitTexture(command, tempRTHandle, outlineRTHandle, outline, 0);

			// 设置outlineRTHandle为blend的主纹理
			outlineBlend.mainTexture = outlineRTHandle;
			// 把缓冲区的内容渲染到renderingData
			Blit(command, ref renderingData, outlineBlend, 0);

			// 执行缓冲区
			context.ExecuteCommandBuffer(command);
			// 释放
			CommandBufferPool.Release(command);
			tempRTHandle.Release();
			outlineRTHandle?.Release();
		}

		private void DrawRenderer(CommandBuffer command, Material material) {
			renderObjs.RemoveAll(obj => obj == null);
			renderObjs.ForEach(renderer => DrawRenderer(command, material, renderer));
		}
		private void DrawRenderer(CommandBuffer command, Material material, Renderer renderer) {
			// 遍历所有的子网格
			int maxIndex = renderer.sharedMaterials.Length;
			// 遍历所有的子网格
			for (int subMeshIndex = 0; subMeshIndex < maxIndex; subMeshIndex++) {
				command.DrawRenderer(renderer, material, subMeshIndex, 0);
			}
		}
	}
}