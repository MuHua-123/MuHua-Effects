using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 轮廓 - 渲染功能
	/// </summary>
	public class OutlineFeature : ScriptableRendererFeature {
		/// <summary> 辅助材质 </summary>
		[Tooltip("辅助材质")] public Material unlit;
		/// <summary> 轮廓材质 </summary>
		[Tooltip("轮廓材质")] public Material outline;
		/// <summary> 混合材质 </summary>
		[Tooltip("混合材质")] public Material outlineBlend;
		/// <summary> 渲染Event </summary>
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
		/// <summary> 是否有效 </summary>
		public bool IsValid => unlit != null && outline != null && outlineBlend != null;

		/// <summary> 渲染通道 </summary>
		private OutlinePass outlinePass = new();

		public override void Create() { }

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			if (!IsValid) { return; }

			outlinePass.unlit = unlit;
			outlinePass.outline = outline;
			outlinePass.outlineBlend = outlineBlend;
			outlinePass.renderPassEvent = renderPassEvent;

			outlinePass.Setup(renderingData);
			renderer.EnqueuePass(outlinePass);
			Dispose();
		}

		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer renderer, bool isClear) {
			if (isClear) { Clear(); }
			if (outlinePass.renderObjs.Contains(renderer)) { return; }
			outlinePass.renderObjs.Add(renderer);
		}
		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer[] renderers, bool isClear) {
			if (isClear) { Clear(); }
			outlinePass.renderObjs.AddRange(renderers);
		}
		/// <summary> 移出渲染队列 </summary>
		public void Remove(Renderer renderer) {
			if (!outlinePass.renderObjs.Contains(renderer)) { return; }
			outlinePass.renderObjs.Remove(renderer);
		}
		/// <summary> 清空队列 </summary>
		public void Clear() {
			outlinePass.renderObjs?.Clear();
			outlinePass.renderObjs = new List<Renderer>();
		}
	}
}