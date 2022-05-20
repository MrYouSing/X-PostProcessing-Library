using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace YouSingStudio.Rendering.Universal {
	public abstract partial class ScriptableRenderPass<TRenderPass,TSettings>
		:ScriptableRenderPass
		where TRenderPass:ScriptableRenderPass<TRenderPass,TSettings>
	{
		#region Fields
		
		public static TRenderPass s_Instance;

		public TSettings settings;

		#endregion Fields

		#region Methods

		public abstract bool Setup(ScriptableRenderer renderer,ref RenderingData renderingData,TSettings featureSettings);

		#endregion Methods
	}
}