#if UNITY_POST_PROCESSING_STACK_V2
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

namespace YouSingStudio.Rendering.Universal {
	#region Nested Types

	[System.Serializable]
	public class PostProcessSettings {
		public RenderPassEvent renderPassEvent=RenderPassEvent.AfterRenderingPostProcessing;
		public PostProcessResources resources;
		public PostProcessProfile profile;
		public List<PostProcessEffectSettings> settings;
	}

	#endregion Nested Types

	public class PostProcessPass
		:ScriptableRenderPass<PostProcessPass,PostProcessSettings>
	{
		#region Fields

		protected static readonly int k_TempTargetId0=Shader.PropertyToID("_PP_Temp0");
		protected static readonly int k_TempTargetId1=Shader.PropertyToID("_PP_Temp1");
		protected static readonly int k_AfterPostProcessTexture=Shader.PropertyToID("_AfterPostProcessTexture");

		[System.NonSerialized]protected RenderTargetIdentifier m_Destination;

		#endregion Fields

		#region Methods

		protected virtual RenderTargetIdentifier GetDestination(ref CameraData data) {
			if(data.postProcessEnabled&&renderPassEvent>=RenderPassEvent.AfterRenderingPostProcessing) {
				return k_AfterPostProcessTexture;
			}else {
				return data.renderer.cameraColorTarget;
			}
		}

		public override bool Setup(ScriptableRenderer renderer,ref RenderingData renderingData,PostProcessSettings featureSettings) {
			settings=featureSettings;
			//
			if(PostProcessExtension.s_PostProcessResources==null) {
				PostProcessExtension.s_PostProcessResources=settings.resources;
			}
			renderPassEvent=settings.renderPassEvent;
			//
			return true;
		}

		public override void Execute(ScriptableRenderContext context,ref RenderingData renderingData) {
			ref CameraData data=ref renderingData.cameraData;
			Camera camera=data.camera;
			RenderTextureDescriptor rtd=data.cameraTargetDescriptor;
			rtd.width=camera.scaledPixelWidth;
			rtd.height=camera.scaledPixelHeight;
			m_Destination=GetDestination(ref data);
			List<PostProcessEffectSettings> list=settings.settings.Count!=0
				?settings.settings:settings.profile.settings;
			//
			CommandBuffer buffer=CommandBufferPool.Get();
				buffer.GetTemporaryRT(k_TempTargetId0,rtd);
				buffer.GetTemporaryRT(k_TempTargetId1,rtd);
				buffer.Blit(m_Destination,k_TempTargetId0);
				int num=buffer.sizeInBytes;
				camera.Blit(k_TempTargetId0,k_TempTargetId1,list,false,buffer);
				if(buffer.sizeInBytes!=num) {
					buffer.Blit(camera.GetContext().destination,m_Destination);
					buffer.ReleaseTemporaryRT(k_TempTargetId0);
					buffer.ReleaseTemporaryRT(k_TempTargetId1);
					context.ExecuteCommandBuffer(buffer);
				}
			CommandBufferPool.Release(buffer);
		}

		#endregion Methods
	}
}
#endif