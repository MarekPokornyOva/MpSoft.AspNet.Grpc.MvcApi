#region using
using Grpc.Core;
using Microsoft.AspNetCore.Http;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	public interface IServerCallContextFactory
	{
		ServerCallContext Create(HttpContext httpContext);
	}
}
