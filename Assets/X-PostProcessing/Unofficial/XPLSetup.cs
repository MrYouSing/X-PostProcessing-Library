#if false&&UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YouSingStudio.Editor {
	public class XPLSetup {
		[MenuItem("Tools/Setup X-PostProcessing Library")]
		public static void Setup(MenuCommand cmd) {
			string root="Assets/X-PostProcessing";
			string fn;
			fn=Path.Combine(root,"X-PostProcessing.Runtime.asmdef");
			if(!File.Exists(fn)) {
					File.WriteAllText(fn,@"{
    ""name"": """+Path.GetFileNameWithoutExtension(fn)+@""",
    ""references"": [
        ""Unity.Postprocessing.Runtime"",
        ""Unity.RenderPipelines.Universal.Runtime""
    ]
}");
			}
			fn=Path.Combine(root,"Editor/X-PostProcessing.Editor.asmdef");
			if(!File.Exists(fn)) {
					File.WriteAllText(fn,@"{
    ""name"": """+Path.GetFileNameWithoutExtension(fn)+@""",
    ""references"": [
        ""Unity.Postprocessing.Runtime"",
        ""Unity.Postprocessing.Editor"",
        ""X-PostProcessing.Runtime""
    ]
}");
			}
			foreach(string dir in Directory.GetDirectories(Path.Combine(root,"Effects"))) {
				fn=Path.Combine(dir,"Editor");
				if(Directory.Exists(fn)) {
					fn=Path.Combine(dir,"Editor/X-PostProcessing.Editor.asmref");
					File.WriteAllText(fn,"{\"reference\": \"X-PostProcessing.Editor\"}");
				}
			}
			AssetDatabase.Refresh();
		}
	}
}
#endif