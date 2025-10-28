using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MuHua;

public class GhostTest : MonoBehaviour {
	public Material material;
	public GhostFeature ghostFeature;
	public Renderer[] objs;


	private void Start() {
		ghostFeature.Settings(new GhostConversion(material));
		ghostFeature.Add(objs, true);
	}
}
public class GhostConversion : MaterialConversion {

	public Material material;

	public GhostConversion(Material material) => this.material = material;

	public (Material, List<int>) To(Material original) {
		List<int> passCount = new List<int>();
		if (original == null) return (original, passCount);

		string shaderName = original.shader.name;
		// 输出调试信息
		// Debug.Log($"Material '{original.name}' uses shader '{shaderName}', passCount={original.passCount}");

		if (shaderName == "Universal Render Pipeline/Lit") {
			passCount.Add(original.passCount - 1);
			Material ghost = new Material(material);
			ghost.mainTexture = original.mainTexture;
			return (ghost, passCount);
		}

		return (original, passCount);
	}
}