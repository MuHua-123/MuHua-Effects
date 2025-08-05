using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	/// <summary>
	/// 渲染轮廓功能
	/// </summary>
	public class OutlineFeature : ScriptableRendererFeature {
		[Tooltip("辅助材质")] public Material unlit;
		[Tooltip("轮廓材质")] public Material outline;
		[Tooltip("混合材质")] public Material outlineBlend;
		/// <summary> 渲染Event </summary>
		public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

		/// <summary> 渲染对象 </summary>
		[HideInInspector] public List<Renderer> RenderObjs = new List<Renderer>();
		/// <summary> 是否有效 </summary>
		public bool IsValid => unlit != null && outline != null && outlineBlend != null;

		/// <summary> 渲染通道 </summary>
		private OutlinePass outlinePass;
		/// <summary> 渲染设置 </summary>
		private OutlineSettings settings;

		public override void Create() {
			outlinePass = new OutlinePass();
			settings = new OutlineSettings();
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			if (!IsValid) { return; }

			RenderObjs.RemoveAll(obj => obj == null);

			settings.unlit = unlit;
			settings.outline = outline;
			settings.outlineBlend = outlineBlend;
			settings.renderObjs = RenderObjs.ToArray();
			settings.renderPassEvent = renderPassEvent;

			outlinePass.Setup(settings, renderingData);
			renderer.EnqueuePass(outlinePass);
			Dispose();
		}

		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer renderer, bool isClear) {
			if (isClear) { Clear(); }
			if (!RenderObjs.Contains(renderer)) { RenderObjs.Add(renderer); }
		}
		/// <summary> 添加到渲染队列 </summary>
		public void Add(Renderer[] renderers, bool isClear) {
			if (isClear) { Clear(); }
			RenderObjs.AddRange(renderers);
		}
		/// <summary> 移出渲染队列 </summary>
		public void Remove(Renderer renderer) {
			if (RenderObjs.Contains(renderer)) { RenderObjs.Remove(renderer); }
		}
		/// <summary> 清空队列 </summary>
		public void Clear() {
			RenderObjs?.Clear();
			RenderObjs = new List<Renderer>();
		}
	}
}