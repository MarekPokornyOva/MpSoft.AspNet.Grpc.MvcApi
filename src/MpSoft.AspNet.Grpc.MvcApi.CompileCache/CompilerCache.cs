#region using
using System.Reflection;
using System.Reflection.Emit;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi.CompileCache
{
	sealed class CompilerCache:IServiceCompiler
	{
		readonly IServiceCompiler _inner;
		readonly IAssemblyCache _assemblyCache;
		public CompilerCache(IServiceCompiler inner,IAssemblyCache assemblyCache)
		{
			_inner=inner;
			_assemblyCache=assemblyCache;
		}

		public Assembly Compile(ControllerDescriptor controllerDescriptor)
			=> _assemblyCache.GetOrSet(controllerDescriptor,stream => { Assembly asm = _inner.Compile(controllerDescriptor); if (stream!=null) asm.SaveAs(stream); return asm; });
	}
}
