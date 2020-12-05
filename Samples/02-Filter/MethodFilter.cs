using Microsoft.AspNetCore.Mvc;
using MpSoft.AspNet.Grpc.MvcApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Filter
{
	public class HttpMethodFilter:IFilter
	{
		public void Filter(ControllerDescriptor controllerDescriptor)
		{
			MethodDescriptor methodDescriptor = controllerDescriptor.Methods[0];

			AttributeDescriptor attributeDescriptor = methodDescriptor.CustomAttributes[0];
			//Change HTTP method to GET
			attributeDescriptor.Constructor=typeof(HttpGetAttribute).GetConstructor(new[] { typeof(string) });
			//Change route to start with "api/v1/greeter"
			attributeDescriptor.ConstructorArgs[0]="api/v1/greeter/"+(string)attributeDescriptor.ConstructorArgs[0];

			ParameterDescriptor parameterDescriptor = methodDescriptor.Parameters[0];
			//Add FromQuery attribute
			parameterDescriptor.CustomAttributes.Add(
				new AttributeDescriptor 
				{
					Constructor=typeof(FromQueryAttribute).GetConstructor(Type.EmptyTypes),
					ConstructorArgs=new object[0] 
				});
		}
	}
}
