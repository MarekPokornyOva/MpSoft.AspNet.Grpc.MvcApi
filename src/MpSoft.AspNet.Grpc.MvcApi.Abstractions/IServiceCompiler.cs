#region using
using System.Reflection;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	public interface IServiceCompiler
	{
		Assembly Compile(ControllerDescriptor controllerDescriptor);
	}
}
