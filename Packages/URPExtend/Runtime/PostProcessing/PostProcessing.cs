using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MuHua {
	// 声明注入点 
	public enum BasicInjectionPoint {
		AfterOpaque,
		AfterSkybox,
		BeforePostProcess,
		AfterPostProcess
	}
	/// <summary>
	/// PostProcessing是在Unity的通用渲染管道（URP）中实现后处理效果的基类。
	/// </summary>
	public abstract class PostProcessing : VolumeComponent, IPostProcessComponent, IDisposable {
		/// <summary> Shader的材质 </summary>
		protected Material material = null;
		// 添加 RenderNormals 属性
		public bool RenderNormals { get; set; } = false;
		// 注入点
		public virtual BasicInjectionPoint InjectionPoint => BasicInjectionPoint.AfterPostProcess;
		// 设置注入点的顺序
		public virtual int OrderInInjectionPoint => 0;

		/// <summary>  配置当前后处理 </summary>
		public abstract void Setup();
		/// <summary>  当相机初始化时执行 </summary>
		public virtual void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) { }
		/// <summary> 执行渲染 </summary>
		public abstract void Render(CommandBuffer cmd, ref RenderingData renderingData, RTHandle source, RTHandle destination);

		// 在OnCameraSetUp函数中使用，渲染时用CoreUtils.SetKeyword
		protected void SetKeyword(string keyword, bool enabled = true) {
			if (enabled) material.EnableKeyword(keyword);
			else material.DisableKeyword(keyword);
		}

		#region IPostProcessComponent
		/// <summary> 后处理是否激活 </summary>
		public abstract bool IsActive();
		/// <summary> 是否支持瓦片渲染 </summary>
		public virtual bool IsTileCompatible() => false;
		#endregion

		#region IDisposable  
		// IDisposable接口用于释放资源，防止资源泄漏。
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary> 释放材质资源 </summary>
		public virtual void Dispose(bool disposing) {
			if (!disposing || material == null) { return; }
			UnityEngine.Object.DestroyImmediate(material);
			material = null;
		}
		#endregion
	}
}
