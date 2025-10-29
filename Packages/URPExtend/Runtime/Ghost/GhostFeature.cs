using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 幽灵 - 渲染功能
	/// </summary>
	public class GhostFeature : ScriptableRendererFeature {
		/// <summary> 渲染Event </summary>
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

		/// <summary> 渲染通道 </summary>
		private GhostPass ghostPass = new();

		public override void Create() { }

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			ghostPass.renderPassEvent = renderPassEvent;

			renderer.EnqueuePass(ghostPass);
			Dispose();
		}

		/// <summary> 设置材质转换器 </summary>
		public void Settings(MaterialConversion conversion) {
			ghostPass.conversion = conversion;
		}

		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer renderer, bool isClear) => ghostPass.Add(renderer, isClear);
		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer[] renderers, bool isClear) => ghostPass.Add(renderers, isClear);
		/// <summary> 移出渲染队列 </summary>
		public void Remove(Renderer renderer) => ghostPass.Remove(renderer);
		/// <summary> 清空队列 </summary>
		public void Clear() => ghostPass.Clear();
	}
}