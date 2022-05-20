using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace YouSingStudio.Rendering.Universal {
	public partial class ScriptableRendererFeature<TRendererFeature,TSettings,TRenderPass>
		:ScriptableRendererFeature
		where TRendererFeature:ScriptableRendererFeature<TRendererFeature,TSettings,TRenderPass>
		where TRenderPass:ScriptableRenderPass<TRenderPass,TSettings>,new()
	{
		#region Fields

		public static TRendererFeature s_Instance;

		[UnityEngine.Serialization.FormerlySerializedAs("m_Settings")]
		public TSettings settings;
		[System.NonSerialized]
		public TRenderPass renderPass;

		#endregion Fields

		public ScriptableRendererFeature() {
			s_Instance=(TRendererFeature)this;
		}

		public override void Create() {
			//
			if(renderPass==null) {
				renderPass=new TRenderPass();
			}
			//
			name=GetType().Name.Replace("RendererFeature",string.Empty);
		}

		public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData renderingData) {
			bool shouldAdd=renderPass.Setup(renderer,ref renderingData,settings);
			if(shouldAdd) {
				renderer.EnqueuePass(renderPass);
			}
		}
	}
}
