using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MuHua {
	/// <summary>
	/// 幽灵渲染单元
	/// </summary>
	public class GhostRendererUnit {
		/// <summary> 子网格 </summary>
		public int subMeshIndex;
		/// <summary> 通道 </summary>
		public List<int> pass = new();
		/// <summary> 材质 </summary>
		public Material material;
		/// <summary> 渲染器 </summary>
		public Renderer renderer;
	}
	/// <summary>
	/// 幽灵渲染器
	/// </summary>
	public class GhostRenderer {
		/// <summary> 目标材质 </summary>
		public Material material;
		/// <summary> 渲染器 </summary>
		public Renderer renderer;
		/// <summary> 渲染单元 </summary>
		public List<GhostRendererUnit> units = new();

		public GhostRenderer(Material material, Renderer renderer) {
			this.material = material;
			this.renderer = renderer;
			int maxIndex = renderer.sharedMaterials.Length;
			for (int subMeshIndex = 0; subMeshIndex < maxIndex; subMeshIndex++) {
				Material original = renderer.sharedMaterials[subMeshIndex];
				units.Add(To(original, subMeshIndex));
			}
		}

		/// <summary> 转换单元 </summary>
		public virtual GhostRendererUnit To(Material original, int subMeshIndex) {
			(Material material, List<int> pass) = To(original);
			GhostRendererUnit unit = new GhostRendererUnit();
			unit.subMeshIndex = subMeshIndex;
			unit.pass = pass;
			unit.material = material;
			unit.renderer = renderer;
			return unit;
		}
		/// <summary> 材质转换 </summary> 
		public virtual (Material, List<int>) To(Material original) {
			if (original == null) return Default(original);
			// 着色器名字
			string shaderName = original.shader.name;
			// 输出调试信息
			// Debug.Log($"Material '{original.name}' uses shader '{shaderName}', passCount={original.passCount}");
			// 标准光照材质
			if (shaderName == "Universal Render Pipeline/Lit") { return URPLit(original); }
			// 返回默认渲染
			return Default(original);
		}
		/// <summary> 默认处理 </summary> 
		public virtual (Material, List<int>) Default(Material original) {
			return (original, new List<int>());
		}
		/// <summary> 处理Universal Render Pipeline/Lit </summary> 
		public virtual (Material, List<int>) URPLit(Material original) {
			Material ghost = new Material(material);
			ghost.mainTexture = original.mainTexture;
			List<int> passCount = new List<int>() { original.passCount - 1 };
			return (ghost, passCount);
		}
	}
}