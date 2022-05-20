#if UNITY_POST_PROCESSING_STACK_V2
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace YouSingStudio.Rendering {
	public static partial class PostProcessExtension
	{
		#region Nested Types

		internal class PostProcessContext {
			public PostProcessRenderContext context;
			public List<PostProcessBundle> bundles;
			public Dictionary<PostProcessEffectSettings,PostProcessEffectRenderer> renderers;

			public virtual void SetEnabled(bool value) {
				if(value) {
					//
					if(bundles==null) {
						bundles=new List<PostProcessBundle>();
					}
					//
					if(context.propertySheets==null) {
						s_PostProcessRenderContext_SetPropertySheets.Invoke(context,Args(new PropertySheetFactory()));
					}
					if(context.resources==null) {
						s_PostProcessRenderContext_SetResources.Invoke(context,Args(s_PostProcessResources));
					}
					//
					if(renderers==null) {
						renderers=new Dictionary<PostProcessEffectSettings,PostProcessEffectRenderer>();
					}
				}else {
					/* PostProcessBundle.Release() will destroy the settings.
					foreach (var bundle in bundles) {
						s_PostProcessBundle_Release.Invoke(bundle,System.Array.Empty<object>());
					}
					*/
					bundles.Clear();
					//
					context.propertySheets.Release();
					//
					foreach (var it in renderers) {
						it.Value?.Release();
					}
					renderers.Clear();
				}
			}
			
			public virtual PostProcessEffectRenderer CreateRenderer(PostProcessEffectSettings settings) {
				PostProcessBundle bundle=System.Activator.CreateInstance(s_PostProcessBundle,k_BindingFlags,null,Args(settings),null) as PostProcessBundle;
				PostProcessEffectRenderer renderer=s_PostProcessBundle_GetRenderer.Invoke(bundle,System.Array.Empty<object>()) as PostProcessEffectRenderer;
				bundles.Add(bundle);
				renderers[settings]=renderer;
				//
				return renderer;
			}
		}

		#endregion Nested Types

		#region Fields

		public static readonly BindingFlags k_BindingFlags=(BindingFlags)(-1)&~BindingFlags.DeclaredOnly;
		public static readonly int k_RenderViewportScaleFactor=Shader.PropertyToID("_RenderViewportScaleFactor");
		public static readonly int k_SMAA_Flip=Shader.PropertyToID("_SMAA_Flip");

		public static bool s_IsInited;
		public static object[] s_Args_One=new object[1];
		public static System.Type s_PostProcessRenderContext;
		public static MethodInfo s_PostProcessRenderContext_SetPropertySheets;
		public static MethodInfo s_PostProcessRenderContext_SetResources;
		public static System.Type s_PostProcessBundle;
		public static MethodInfo s_PostProcessBundle_GetRenderer;
		public static MethodInfo s_PostProcessBundle_Release;
		public static PostProcessResources s_PostProcessResources=null;

		internal static Dictionary<Camera,PostProcessContext> s_Contexts;

		#endregion Fields

		#region Methods
		
		internal static object[] Args(object arg0) {
			s_Args_One[0]=arg0;
			return s_Args_One;
		}

		internal static string GetTempName(string str) {
			return string.Format("{0} (Temp)",str);
		}

		public static System.Action<RenderTexture,RenderTexture> OnRenderImage(this Object thiz) {
			if(thiz!=null) {
				MethodInfo mi=thiz.GetType().GetMethod("OnRenderImage",k_BindingFlags);
				if(mi!=null) {
					return System.Delegate.CreateDelegate(typeof(System.Action<RenderTexture,RenderTexture>),thiz,mi)
						as System.Action<RenderTexture,RenderTexture>;
				}
			}
			return null;
		}

		public static bool IsActiveAndEnabled(this PostProcessEffectSettings thiz,PostProcessRenderContext context) {
			return thiz.active&&thiz.IsEnabledAndSupported(context);
		}

		public static void Flip(this PostProcessRenderContext thiz) {
			if(thiz!=null) {
				CommandBuffer command=thiz.command;
				command.GetTemporaryRT(k_SMAA_Flip,thiz.width,thiz.height,0,FilterMode.Bilinear,thiz.sourceFormat,RenderTextureReadWrite.Linear,1,false,RenderTextureMemoryless.None,thiz.camera.allowDynamicResolution);
					command.Blit(thiz.destination,k_SMAA_Flip,new Vector2(1.0f,-1.0f),new Vector2(0.0f,1.0f));
					command.Blit(k_SMAA_Flip,thiz.destination);
				command.ReleaseTemporaryRT(k_SMAA_Flip);
			}
		}

		public static void Swap(this PostProcessRenderContext thiz) {
			if(thiz!=null) {
				var tmp=thiz.source;thiz.source=thiz.destination;thiz.destination=tmp;
			}
		}

		public static void Blit(this Camera thiz,RenderTargetIdentifier src,RenderTargetIdentifier dst,IList<PostProcessEffectSettings> settings,bool flip=true,CommandBuffer buffer=null) {
			PostProcessRenderContext context=thiz.GetContext();
			PostProcessContext myCtx=s_Contexts[thiz];
			if(buffer==null) {
				context.Begin();
			}else {
				context.command=buffer;
			}
			//
			context.source=src;
			context.destination=dst;
			PostProcessEffectSettings it;
			for(int i=0,imax=settings.Count,j=0;i<imax;++i) {
				it=settings[i];
				if(it!=null&&it.IsActiveAndEnabled(context)) {
					//
					if(j>0) {context.Swap();}
					++j;
					//
					it.GetRenderer(myCtx).Render(context);
				}
			}
			//
			if(buffer==null) {
				context.End(flip);
			}else {
				context.command=null;
			}
		}

		internal static void Init() {
			if(s_IsInited) {
				return;
			}
			s_IsInited=true;
			//
			s_PostProcessRenderContext=typeof(PostProcessRenderContext);
			s_PostProcessRenderContext_SetPropertySheets=s_PostProcessRenderContext.GetProperty("propertySheets",k_BindingFlags).GetSetMethod(true);
			s_PostProcessRenderContext_SetResources=s_PostProcessRenderContext.GetProperty("resources",k_BindingFlags).GetSetMethod(true);
			s_PostProcessBundle=typeof(PostProcessBundle);
			s_PostProcessBundle_GetRenderer=s_PostProcessBundle.GetProperty("renderer",k_BindingFlags).GetGetMethod(true);
			s_PostProcessBundle_Release=s_PostProcessBundle.GetMethod("Release",k_BindingFlags);
			s_Contexts=new Dictionary<Camera,PostProcessContext>();
			//
			if(s_PostProcessResources==null) {
				s_PostProcessResources=Resources.Load<PostProcessResources>("PostProcessResources");
			}
			if(s_PostProcessResources!=null) {
				MethodInfo mi=typeof(RuntimeUtilities).GetMethod("UpdateResources",k_BindingFlags);
				mi.Invoke(null,Args(s_PostProcessResources));
			}else {
				Debug.LogWarning("PostProcessResources==null");
			}
		}

		public static PostProcessRenderContext GetContext(this Camera thiz) {
			if(!s_IsInited) {Init();}
			if(thiz==null) {return null;}
			//
			if(!s_Contexts.TryGetValue(thiz,out var context)||context==null) {
				context=new PostProcessContext{
					context=new PostProcessRenderContext()
				};
				context.context.camera=thiz;
				context.SetEnabled(true);
				//
				s_Contexts[thiz]=context;
			}
			Shader.SetGlobalFloat(k_RenderViewportScaleFactor,1.0f);
			return context.context;
		}

		public static void ReleaseContext(this Camera thiz) {
			if(!s_IsInited) {Init();}
			if(thiz==null) {return;}
			//
			if(s_Contexts.TryGetValue(thiz,out var context)&&context!=null) {
				context.SetEnabled(false);
				//
				s_Contexts.Remove(thiz);// TODO Pool????
			}
		}

		public static void Begin(this PostProcessRenderContext thiz) {
			if(!s_IsInited) {Init();}
			//
			if(thiz!=null) {
				CommandBuffer command=thiz.command;
				if(command==null) {
					command=CommandBufferPool.Get(GetTempName(thiz.camera.name));
					thiz.command=command;
				}else {
					command.Clear();
				}
			}
		}

		public static bool End(this PostProcessRenderContext thiz,bool flip=true) {
			if(!s_IsInited) {Init();}
			//
			if(thiz!=null) {
				CommandBuffer command=thiz.command;
				if(command.sizeInBytes!=0) {
					if(flip) {thiz.Flip();}
					Graphics.ExecuteCommandBuffer(command);
					if(command.name==GetTempName(thiz.camera.name)) {
						CommandBufferPool.Release(command);
						thiz.command=null;
					}
					return true;
				}
			}
			return false;
		}

		internal static PostProcessEffectRenderer GetRenderer(this PostProcessEffectSettings thiz,PostProcessContext context) {
			if(!s_IsInited) {Init();}
			if(thiz==null) {return null;}
			//
			if(!context.renderers.TryGetValue(thiz,out var renderer)||renderer==null) {
				renderer=context.CreateRenderer(thiz);
			}
			return renderer;
		}

		public static PostProcessEffectRenderer GetRenderer(this PostProcessEffectSettings thiz,Camera camera) {
			if(camera!=null) {
				return thiz.GetRenderer(camera.GetContext());
			}
			return null;
		}

		public static PostProcessEffectRenderer GetRenderer(this PostProcessEffectSettings thiz,PostProcessRenderContext context) {
			if(context!=null) {
				Camera camera=context.camera;
				if(camera!=null) {
					return thiz.GetRenderer(camera.GetContext());
				}
			}
			return null;
		}

		#endregion Methods
	}
}
#endif