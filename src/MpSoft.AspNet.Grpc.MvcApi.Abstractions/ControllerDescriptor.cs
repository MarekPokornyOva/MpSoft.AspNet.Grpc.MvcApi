#region using
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	public class ControllerDescriptor
	{
		static readonly Type _controllerBaseType = typeof(ControllerBase);

		public ControllerDescriptor(Type grpcServiceType,IReadOnlyList<MethodDescriptor> methods)
		{
			GrpcServiceType=grpcServiceType;
			Methods=methods;
		}

		public Type GrpcServiceType { get; }
		public IReadOnlyList<MethodDescriptor> Methods { get; }

		Type _baseType = _controllerBaseType;
		public virtual Type BaseType 
		{
			get => _baseType; 
			set 
			{
				if (!_controllerBaseType.IsAssignableFrom(value))
					throw new InvalidCastException("BaseType must be ControllerBase or descendant.");
				if (value.GetConstructor(Type.EmptyTypes)==null)
					throw new InvalidCastException("BaseType must contain public parameterless constructor.");
				_baseType=null;
			}
		}

		public virtual string ServiceName { get; set; }
		public virtual string TypeName { get; set; }
		public IList<AttributeDescriptor> CustomAttributes { get; } = new List<AttributeDescriptor>();
	}

	public class AttributeDescriptor
	{
		public virtual ConstructorInfo Constructor { get; set; }
		public virtual object[] ConstructorArgs { get; set; }
	}

	public class MethodDescriptor
	{
		public MethodDescriptor(MethodInfo methodInfo,IReadOnlyList<ParameterDescriptor> parameters)
		{
			MethodInfo=methodInfo;
			Parameters=parameters;
		}

		public MethodInfo MethodInfo { get; }
		public IReadOnlyList<ParameterDescriptor> Parameters { get; }

		public virtual string Name { get; set; }
		public virtual bool Generate { get; set; } = true;
		public IList<AttributeDescriptor> CustomAttributes { get; } = new List<AttributeDescriptor>();
	}

	public class ParameterDescriptor
	{
		public ParameterDescriptor(ParameterInfo parameterInfo)
			=> ParameterInfo=parameterInfo;

		public ParameterInfo ParameterInfo { get; }

		public virtual string Name { get; set; }
		public IList<AttributeDescriptor> CustomAttributes { get; } = new List<AttributeDescriptor>();
	}
}
