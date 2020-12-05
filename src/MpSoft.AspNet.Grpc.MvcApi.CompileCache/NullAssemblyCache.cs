#region using
using System;
using System.IO;
using System.Reflection;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi.CompileCache
{
	class NullAssemblyCache:IAssemblyCache
	{
		public Assembly GetOrSet(ControllerDescriptor controllerDescriptor,Func<Stream,Assembly> assemblyProvider)
			=> assemblyProvider(null);
	}
}
