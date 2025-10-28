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
		public void Add(Renderer renderer, bool isClear) {
			if (isClear) { Clear(); }
			if (ghostPass.renderObjs.Contains(renderer)) { return; }
			ghostPass.renderObjs.Add(renderer);
		}
		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer[] renderers, bool isClear) {
			if (isClear) { Clear(); }
			ghostPass.renderObjs.AddRange(renderers);
		}
		/// <summary> 移出渲染队列 </summary>
		public void Remove(Renderer renderer) {
			if (!ghostPass.renderObjs.Contains(renderer)) { return; }
			ghostPass.renderObjs.Remove(renderer);
		}
		/// <summary> 清空队列 </summary>
		public void Clear() {
			ghostPass.renderObjs?.Clear();
			ghostPass.renderObjs = new List<Renderer>();
		}
	}
}