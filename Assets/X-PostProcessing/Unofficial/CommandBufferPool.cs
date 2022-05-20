using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering {
	internal class CommandBufferPool
	{
		internal static Stack<CommandBuffer> s_Pool=new Stack<CommandBuffer>();

		internal static CommandBuffer Get() {
			return s_Pool.Count>0?s_Pool.Pop():new CommandBuffer();
		}

		internal static CommandBuffer Get(string name) {
			var tmp=Get();
			tmp.name=name;
			return tmp;
		}

		internal static void Release(CommandBuffer toRelease) {
			if(toRelease!=null) {
				toRelease.Clear();
				//
				if(!s_Pool.Contains(toRelease)) {
					s_Pool.Push(toRelease);
				}
			}
		}
	}
}
