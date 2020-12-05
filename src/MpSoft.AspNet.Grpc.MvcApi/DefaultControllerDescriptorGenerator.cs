#region using
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	public class DefaultControllerDescriptorGenerator:IControllerDescriptorGenerator
	{
		readonly static object[] _emptyObjectArray = new object[0];
		public virtual ControllerDescriptor Generate(Type grpcServiceType)
		{
			string serviceName = grpcServiceType.Name;
			IReadOnlyList<MethodDescriptor> methods=new ReadOnlyCollection<MethodDescriptor>(grpcServiceType.GetMethods(BindingFlags.Public|BindingFlags.Instance|BindingFlags.InvokeMethod).Where(x => x.DeclaringType==grpcServiceType)
				.Select(GenerateMethodDescriptor).ToList());

			ControllerDescriptor controllerDescriptor = new ControllerDescriptor(grpcServiceType,methods)
			{
				ServiceName=serviceName,
				TypeName=serviceName+"Controller"
			};
			controllerDescriptor.CustomAttributes.Add(new AttributeDescriptor { Constructor=typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes),ConstructorArgs=_emptyObjectArray });


			return controllerDescriptor;
		}

		static MethodDescriptor GenerateMethodDescriptor(MethodInfo methodInfo)
		{
			string methodName = methodInfo.Name;
			IReadOnlyList<ParameterDescriptor> parms = new ReadOnlyCollection<ParameterDescriptor>(methodInfo.GetParameters().Where(x => !IsCtxParm(x))
				.Select(GenerateParameterDescriptor).ToList());

			MethodDescriptor methodDescriptor = new MethodDescriptor(methodInfo,parms)
			{
				Name=methodName
			};
			methodDescriptor.CustomAttributes.Add(new AttributeDescriptor { Constructor=typeof(HttpPutAttribute).GetConstructor(new[] { typeof(string) }),ConstructorArgs=new[] { methodName } });

			return methodDescriptor;
		}

		static ParameterDescriptor GenerateParameterDescriptor(ParameterInfo parameterInfo)
			=> new ParameterDescriptor(parameterInfo) { Name=parameterInfo.Name };

		internal static bool IsCtxParm(ParameterInfo parameterInfo)
			=> parameterInfo.ParameterType==typeof(ServerCallContext);
	}
}
