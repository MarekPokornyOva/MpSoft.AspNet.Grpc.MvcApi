#region using
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi
{
	sealed class DefaultServiceCompiler:IServiceCompiler
	{
		public Assembly Compile(ControllerDescriptor controllerDescriptor)
		{
			string baseName = controllerDescriptor.ServiceName;
			Type grpcServiceType = controllerDescriptor.GrpcServiceType;

			AssemblyBuilder asmBldr = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(baseName),AssemblyBuilderAccess.Run);
			ModuleBuilder modBldr = asmBldr.DefineDynamicModule(baseName);
			TypeBuilder typeBuilder = modBldr.DefineType(controllerDescriptor.TypeName,TypeAttributes.Public|TypeAttributes.Class|TypeAttributes.AutoClass|TypeAttributes.AnsiClass|TypeAttributes.BeforeFieldInit|TypeAttributes.AutoLayout,controllerDescriptor.BaseType);
			SetAttributes(controllerDescriptor.CustomAttributes,typeBuilder.SetCustomAttribute);

			FieldBuilder serviceField = typeBuilder.DefineField("_service",grpcServiceType,FieldAttributes.Private|FieldAttributes.InitOnly);
			FieldBuilder ctxFactoryField = typeBuilder.DefineField("_ctxFact",typeof(IServerCallContextFactory),FieldAttributes.Private|FieldAttributes.InitOnly);
			DefineConstructor(typeBuilder,serviceField,ctxFactoryField);

			foreach (MethodDescriptor md in controllerDescriptor.Methods)
				DefineMethod(typeBuilder,md,serviceField,ctxFactoryField);

			return typeBuilder.CreateTypeInfo().Assembly;
		}

		static void DefineConstructor(TypeBuilder typeBuilder,FieldBuilder serviceField,FieldBuilder ctxFactoryField)
		{
			ConstructorBuilder ctorBldr = typeBuilder.DefineConstructor(MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName|MethodAttributes.RTSpecialName,CallingConventions.Standard,new[] { serviceField.FieldType,ctxFactoryField.FieldType });
			ctorBldr.DefineParameter(1,ParameterAttributes.None,"service");
			ctorBldr.DefineParameter(2,ParameterAttributes.None,"ctxFact");

			ILGenerator gen = ctorBldr.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Call,typeBuilder.BaseType.GetConstructor(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,Type.EmptyTypes,null));
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_1);
			gen.Emit(OpCodes.Stfld,serviceField);
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldarg_2);
			gen.Emit(OpCodes.Stfld,ctxFactoryField);
			gen.Emit(OpCodes.Ret);
		}

		static void DefineMethod(TypeBuilder typeBuilder,MethodDescriptor methodDescriptor,FieldBuilder serviceField,FieldBuilder ctxFactoryField)
		{
			MethodInfo method = methodDescriptor.MethodInfo;

			ParameterInfo[] pis = method.GetParameters();
			IReadOnlyCollection<ParameterDescriptor> pisNoCtx = methodDescriptor.Parameters;

			string methodName = methodDescriptor.Name;
			MethodBuilder methBldr = typeBuilder.DefineMethod(methodName,MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.NewSlot|MethodAttributes.Virtual,method.ReturnType,pisNoCtx.Select(x => x.ParameterInfo.ParameterType).ToArray());
			SetAttributes(methodDescriptor.CustomAttributes,methBldr.SetCustomAttribute);

			int a = 1;
			foreach (ParameterDescriptor parmDescriptor in pisNoCtx)
			{
				ParameterBuilder parmBuilder = methBldr.DefineParameter(a++,ParameterAttributes.None,parmDescriptor.Name);
				SetAttributes(parmDescriptor.CustomAttributes,parmBuilder.SetCustomAttribute);
			}			

			ILGenerator gen = methBldr.GetILGenerator();
			gen.Emit(OpCodes.Ldarg_0);
			gen.Emit(OpCodes.Ldfld,serviceField);

			void EmitMethodCall(MethodInfo methodInfo)
				=> gen.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,methodInfo);

			a=1;
			foreach (ParameterInfo pi in pis)
			{
				if (DefaultControllerDescriptorGenerator.IsCtxParm(pi))
				{
					gen.Emit(OpCodes.Ldarg_0);
					gen.Emit(OpCodes.Ldfld,ctxFactoryField);
					gen.Emit(OpCodes.Ldarg_0);
					EmitMethodCall(typeof(ControllerBase).GetProperty(nameof(ControllerBase.HttpContext)).GetMethod);
					EmitMethodCall(typeof(IServerCallContextFactory).GetMethod("Create"));
				}
				else
					gen.Emit(OpCodes.Ldarg,a++);
			}
			EmitMethodCall(method);
			gen.Emit(OpCodes.Ret);
		}

		static void SetAttributes(IList<AttributeDescriptor> attributes,Action<CustomAttributeBuilder> attributeSetter)
		{
			foreach (AttributeDescriptor attribute in attributes)
				attributeSetter(new CustomAttributeBuilder(attribute.Constructor,attribute.ConstructorArgs));
		}
	}
}
