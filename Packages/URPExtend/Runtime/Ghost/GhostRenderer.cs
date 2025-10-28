using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MuHua {
	/// <summary>
	/// 幽灵渲染器
	/// </summary>
	public class GhostRenderer : MonoBehaviour {
		/// <summary> 替换的材质 </summary>
		[Tooltip("替换材质")] public Material material;

		public (Material, List<int>) To(Material original) {
			if (original == null) return Default(original);

			string shaderName = original.shader.name;
			// 输出调试信息
			// Debug.Log($"Material '{original.name}' uses shader '{shaderName}', passCount={original.passCount}");

			if (shaderName == "Universal Render Pipeline/Lit") { return URPLit(original); }

			return Default(original);
		}

		/// <summary> 处理Universal Render Pipeline/Lit </summary> 
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