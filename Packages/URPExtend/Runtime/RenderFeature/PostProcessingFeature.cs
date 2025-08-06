using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace MuHua {
	/// <summary>
	/// 后处理特性
	/// </summary>
	public class PostProcessingFeature : ScriptableRendererFeature {
		// 开启此选项渲染法线图
		public bool NormalTexture = false;
		/// <summary> 体积光着色器 </summary>
		[HideInInspector] public Shader volumetricShader;

		// 获取后处理基类列表
		private List<PostProcessing> postProcessings;

		// 注入Pass
		private PostProcessingPass afterOpaquePass;
		private PostProcessingPass afterSkyboxPass;
		private PostProcessingPass beforePostProcessPass;
		private PostProcessingPass afterPostProcessPass;
		private DepthNormalsPass depthNormalsPass;

		public override void Create() {
			// 自动查找并赋值 volumetricShader
			if (volumetricShader == null) {
				volumetricShader = Shader.Find("MuHua/PostProcessing/VolumetricShader");
				if (volumetricShader == null) {
					Debug.LogWarning("未找到MuHua/PostProcessing/VolumetricShader，请确保 Shader 已正确导入项目。");
				}
			}

			var stack = VolumeManager.instance.stack;
			// 获取 所有继承自 PostProcessing 类型的Volume组件 并增加到列表中
			postProcessings = VolumeManager.instance.baseComponentTypeArray
			.Where(t => t.IsSubclassOf(typeof(PostProcessing)))
			.Select(t => stack.GetComponent(t) as PostProcessing)
			.ToList();

			// 设置 AfterOpaque 的后处理效果
			var afterOpaqueCPPs = postProcessings
				.Where(c => c.InjectionPoint == BasicInjectionPoint.AfterOpaque)
				.OrderBy(c => c.OrderInInjectionPoint)
				.ToList();

			// afterOpaqueCPPs 储存所有 AfterOpaque 类型排序后的新列表
			afterOpaquePass = new PostProcessingPass("不透明物体之后", afterOpaqueCPPs);
			// 对应时机
			afterOpaquePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

			// 筛选出设置为 AfterSkybox 的后处理效果
			var afterSkyboxCPPs = postProcessings
				.Where(c => c.InjectionPoint == BasicInjectionPoint.AfterSkybox)
				.OrderBy(c => c.OrderInInjectionPoint)
				.ToList();

			afterSkyboxPass = new PostProcessingPass("天空盒之后", afterSkyboxCPPs);
			// 对应时机
			afterSkyboxPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

			// 筛选出设置为 BeforePostProcess 的后处理效果
			var beforePostProcessingCPPs = postProcessings
				.Where(c => c.InjectionPoint == BasicInjectionPoint.BeforePostProcess)
				.OrderBy(c => c.OrderInInjectionPoint)
				.ToList();

			beforePostProcessPass = new PostProcessingPass("后处理之后", beforePostProcessingCPPs);
			// 对应时机
			beforePostProcessPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

			// 筛选出设置为 AfterPostProcess 的后处理效果
			var afterPostProcessCPPs = postProcessings
				.Where(c => c.InjectionPoint == BasicInjectionPoint.AfterPostProcess)
				.OrderBy(c => c.OrderInInjectionPoint)
				.ToList();

			afterPostProcessPass = new PostProcessingPass("渲染最后阶段", afterPostProcessCPPs);
			// 对应时机
			afterPostProcessPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
			// 初始化
			depthNormalsPass = new DepthNormalsPass();
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			// 当前摄像机是否开启后处理
			if (!renderingData.cameraData.postProcessEnabled) { return; }
			bool requireNormals = NormalTexture; // 初始标记是否需要渲染法线图

			// 检查每个后处理实例是否需要渲染法线图
			foreach (var postProcess in postProcessings) {
				// 找到一个需要渲染法线图的后处理后即退出循环
				if (postProcess.RenderNormals) { requireNormals = true; break; }
			}

			// 加入各个后处理Pass
			EnqueuePassIfActive(afterOpaquePass, renderer);
			EnqueuePassIfActive(afterSkyboxPass, renderer);
			EnqueuePassIfActive(beforePostProcessPass, renderer);
			EnqueuePassIfActive(afterPostProcessPass, renderer);


			// 根据需要加入 DepthNormalsPass
			if (requireNormals) { renderer.EnqueuePass(depthNormalsPass); }
		}

		private void EnqueuePassIfActive(PostProcessingPass pass, ScriptableRenderer renderer) {
			if (pass != null && pass.SetupPostProcessing()) {
				pass.ConfigureInput(ScriptableRenderPassInput.Color);
				renderer.EnqueuePass(pass);
			}
		}
		//  释放资源
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			afterSkyboxPass.Dispose();
			beforePostProcessPass.Dispose();
			afterPostProcessPass.Dispose();
			if (postProcessings == null) { return; }
			foreach (var item in postProcessings) { item.Dispose(); }
		}

		// 渲染法线Pass
		private class DepthNormalsPass : ScriptableRenderPass {
			// 相机初始化
			public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
				// 设置输入为Normal，让Unity RP添加DepthNormalPrepass Pass
				ConfigureInput(ScriptableRenderPassInput.Normal);
			}

			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
				// 什么都不做，我们只需要在相机初始化时配置DepthNormals即可
			}
		}
	}
}
