#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace YouSingStudio.Rendering.Universal {
	public class PostProcessFeature
		:ScriptableRendererFeature<PostProcessFeature,PostProcessSettings,PostProcessPass>
	{
	}
}
#endif