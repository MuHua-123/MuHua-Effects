using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace MuHua {
	/// <summary>
	/// 后处理通道
	/// </summary>
	public class PostProcessingPass : ScriptableRenderPass {
		// 获取后处理基类列表
		private List<PostProcessing> postProcessings;
		// 存储当前激活的自定义后处理效果的索引
		private List<int> postProcessingIndex;

		// 声明RT
		private RTHandle mSourceRT;
		private RTHandle mDesRT;
		private RTHandle mTempRT0;
		private RTHandle mTempRT1;
		private string mTempRT0Name => "_TemporaryRenderTexture0";
		private string mTempRT1Name => "_IntermediateRenderTarget1";

		// 性能分析的标签
		private string profilerTag;
		// 用于监控每个自定义后处理效果的性能
		private List<ProfilingSampler> profilingSamplers;

		// 构造函数
		public PostProcessingPass(string profilerTag, List<PostProcessing> postProcessings) {
			this.profilerTag = profilerTag;
			this.postProcessings = postProcessings;
			// 初始化后处理效果索引列表
			postProcessingIndex = new List<int>(postProcessings.Count);
			// 性能采样器列表
			profilingSamplers = postProcessings.Select(c => new ProfilingSampler(c.ToString())).ToList();
		}

		// 相机初始化时执行
		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
			var descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.msaaSamples = 1;
			descriptor.depthBufferBits = 0;

			// 分配临时纹理
			RenderingUtils.ReAllocateIfNeeded(ref mTempRT0, descriptor, name: mTempRT0Name);
			RenderingUtils.ReAllocateIfNeeded(ref mTempRT1, descriptor, name: mTempRT1Name);

			foreach (var i in postProcessingIndex) {
				postProcessings[i].OnCameraSetup(cmd, ref renderingData);
			}
		}

		// 相机清除时执行
		public override void OnCameraCleanup(CommandBuffer cmd) {
			mDesRT = null;
			mSourceRT = null;
		}

		// 确定哪些后处理效果是激活的，通过调用它们的 Setup 方法和检查它们的激活状态 (IsActive())
		public bool SetupPostProcessing() {
			// 清空后处理索引列表
			postProcessingIndex.Clear();
			// 遍历后处理效果列表
			for (int i = 0; i < postProcessings.Count; i++) {
				postProcessings[i].Setup();
				// 检查后处理效果是否激活
				if (postProcessings[i].IsActive()) { postProcessingIndex.Add(i); }
			}
			// 返回是否有激活的后处理效果
			return postProcessingIndex.Count != 0;
		}

		// 执行逻辑
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (!SetupPostProcessing()) return;   // 方法就直接返回，不执行任何渲染操作

			//初始化 commandbuffer
			var cmd = CommandBufferPool.Get(profilerTag);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();

			// 设置源和目标RT为本次渲染的RT 在Execute里进行 特殊处理后处理后注入点
			mDesRT = renderingData.cameraData.renderer.cameraColorTargetHandle;
			mSourceRT = renderingData.cameraData.renderer.cameraColorTargetHandle;

			// 根据激活后处理数量执行不同的渲染策略
			if (postProcessingIndex.Count == 1) {
				// 激活 一个后处理
				// 如果只有一个激活的后处理，直接在 mTempRT0 上渲染。
				int index = postProcessingIndex[0];
				using (new ProfilingScope(cmd, profilingSamplers[index])) {
					postProcessings[index].Render(cmd, ref renderingData, mSourceRT, mTempRT0);
				}
			}
			else {
				// 激活多个后处理
				// 如果有多个激活的后处理，依次在 mTempRT0 和 mTempRT1 之间交换并渲染。
				Blitter.BlitCameraTexture(cmd, mSourceRT, mTempRT0);
				for (int i = 0; i < postProcessingIndex.Count; i++) {
					int index = postProcessingIndex[i];
					var customProcessing = postProcessings[index];

					using (new ProfilingScope(cmd, profilingSamplers[index])) {
						customProcessing.Render(cmd, ref renderingData, mTempRT0, mTempRT1);
					}
					CoreUtils.Swap(ref mTempRT0, ref mTempRT1);
				}
			}
			Blitter.BlitCameraTexture(cmd, mTempRT0, mDesRT);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);

		}

		// 释放临时纹理
		public void Dispose() {
			RTHandles.Release(mTempRT0);
			RTHandles.Release(mTempRT1);
		}
	}
}
