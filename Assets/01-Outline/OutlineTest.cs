using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using MuHua;

/// <summary>
/// 轮廓测试
/// </summary>
public class OutlineTest : MonoBehaviour {
	public UniversalRendererData rendererData;
	public Renderer[] objs1;
	public Renderer[] objs2;

	private void Start() {
		Settings("OutlineFeatureA", objs1);
		Settings("OutlineFeatureB", objs2);
	}
	private void Settings(string featureName, Renderer[] objs) {
		if (rendererData == null) { return; }
		ScriptableRendererFeature feature = rendererData.rendererFeatures.Find(feature => feature.name == featureName);
		if (!(feature is OutlineFeature outlineFeature)) { return; }
		outlineFeature.Add(objs, true);
	}
}
