#region using
using System;
using System.IO;
using System.Reflection;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi.CompileCache
{
	public interface IAssemblyCache
	{
		Assembly GetOrSet(ControllerDescriptor controllerDescriptor,Func<Stream,Assembly> assemblyProvider);
	}
}
