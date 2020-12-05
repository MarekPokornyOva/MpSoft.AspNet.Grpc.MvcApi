#region using
using System;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	public interface IControllerDescriptorGenerator
	{
		ControllerDescriptor Generate(Type grpcServiceType);
	}
}
