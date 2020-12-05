#region using
using MpSoft.AspNet.Grpc.MvcApi;
using MpSoft.AspNet.Grpc.MvcApi.CompileCache;
#endregion using

namespace Microsoft.Extensions.DependencyInjection
{
	public static class CacheServiceCollectionExtensions
	{
		public static IServiceCollection AddCacheWithJellequin(this IServiceCollection services)
			=> AddCacheWithJellequin<NullAssemblyCache>(services);

		public static IServiceCollection AddCacheWithJellequin<TCache>(this IServiceCollection services) where TCache: class,IAssemblyCache
		{
			services.AddSingleton<IAssemblyCache,TCache>();
			return services.Decorate<IServiceCompiler,CompilerCache>();
		}

		public static IServiceCollection AddCacheWithJellequin<TCache>(this IServiceCollection services, TCache cache) where TCache : class, IAssemblyCache
		{
			services.AddSingleton<IAssemblyCache>(cache);
			return services.Decorate<IServiceCompiler,CompilerCache>();
		}
	}
}
