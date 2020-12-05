#region using
using MpSoft.AspNet.Grpc.MvcApi;
using System;
using System.Collections.Generic;
#endregion using

namespace Microsoft.Extensions.DependencyInjection
{
	public static class GrpcHttpServiceCollectionExtensions
	{
		public class GrpcHttpOptions
		{
			public IServiceCollection CompileServices { get; internal set; }
			public IList<Type> GrpcServices { get; } = new List<Type>();
			public IList<IFilter> Filters { get; } = new List<IFilter>();
		}

		public static IMvcBuilder AddGrpcHttp(this IMvcBuilder mvcCoreBuilder,Action<GrpcHttpOptions> configure)
		{
			IServiceCollection compileServices = new ServiceCollection();
			compileServices.AddSingleton<IServiceCompiler,DefaultServiceCompiler>();
			compileServices.AddSingleton<IControllerDescriptorGenerator,DefaultControllerDescriptorGenerator>();
			GrpcHttpOptions options = new GrpcHttpOptions { CompileServices=compileServices };
			configure(options);
			IServiceProvider sp=compileServices.BuildServiceProvider();

			IControllerDescriptorGenerator csg = sp.GetRequiredService<IControllerDescriptorGenerator>();
			IServiceCompiler sc = sp.GetRequiredService<IServiceCompiler>();
			foreach (Type grpcServiceType in options.GrpcServices)
			{
				ControllerDescriptor controllerDescriptor = csg.Generate(grpcServiceType);
				foreach (IFilter filter in options.Filters)
					filter.Filter(controllerDescriptor);
				mvcCoreBuilder=mvcCoreBuilder.AddApplicationPart(sc.Compile(controllerDescriptor));
			}

			mvcCoreBuilder.Services.AddSingleton<IServerCallContextFactory,DefaultServerCallContextFactory>();

			return mvcCoreBuilder;
		}
	}

	public static class TypeListExtensions
	{
		public static IList<Type> Add<T>(this IList<Type> types)
		{
			types.Add(typeof(T));
			return types;
		}

		public static IList<IFilter> Add<T>(this IList<IFilter> filters) where T: class,IFilter, new()
		{
			filters.Add((IFilter)Activator.CreateInstance(typeof(T)));
			return filters;
		}
	}
}
