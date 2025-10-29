using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 幽灵 - 渲染通道
	/// </summary>
	public class GhostPass : ScriptableRenderPass {
		/// <summary> 分析器标签 </summary>
		public const string ProfilerTag = " Ghost";

		/// <summary> 材质转换器 </summary>
		public MaterialConversion conversion;
		/// <summary> 渲染对象 </summary>
		public List<Renderer> renderObjs = new();

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.cameraType == CameraType.SceneView) { return; }
			if (renderingData.cameraData.cameraType == CameraType.Preview) { return; }
			if (conversion == null) { return; }

			CommandBuffer command = CommandBufferPool.Get(ProfilerTag);
			// 绘制渲染物体
			DrawRenderer(command);
			// 执行缓冲区
			context.ExecuteCommandBuffer(command);
			// 释放
			CommandBufferPool.Release(command);
		}

		private void DrawRenderer(CommandBuffer command) {
			renderObjs.RemoveAll(obj => obj == null);
			renderObjs.ForEach(renderer => DrawRenderer(command, renderer));
		}
		private void DrawRenderer(CommandBuffer command, Renderer renderer) {
			// 遍历所有的子网格
			int maxIndex = renderer.sharedMaterials.Length;
			for (int subMeshIndex = 0; subMeshIndex < maxIndex; subMeshIndex++) {
				Material original = renderer.sharedMaterials[subMeshIndex];
				(Material material, List<int> pass) = conversion.To(original);
				pass.ForEach(index => command.DrawRenderer(renderer, material, subMeshIndex, index));
			}
		}
	}
	/// <summary>
	/// 材质转换器
	/// </summary>
	public interface MaterialConversion {
		public (Material, List<int>) To(Material original);
	}
}