using UnityEngine;

namespace MuHua {
	[CreateAssetMenu(fileName = "ShaderConfig", menuName = "Rendering/ShaderConfig")]
	public class ShaderConfig : ScriptableObject {
		public Shader volumetricShader;
	}
}