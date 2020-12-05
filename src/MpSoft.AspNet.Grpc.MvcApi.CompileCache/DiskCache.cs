#region using
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
#endregion using

namespace MpSoft.AspNet.Grpc.MvcApi.CompileCache
{
	public class DiskCache:IAssemblyCache
	{
		readonly string _baseDirectory;
		public DiskCache(string baseDirectory)
		{
			_baseDirectory=baseDirectory;
		}

		public Assembly GetOrSet(ControllerDescriptor controllerDescriptor,Func<Stream,Assembly> assemblyProvider)
		{
			string path = Path.Combine(_baseDirectory,GetFileName(controllerDescriptor));
			if (File.Exists(path))
			{
				byte[] asmBytes;
				using (MemoryStream ms = new MemoryStream())
				{
					using (Stream file = File.OpenRead(path))
						file.CopyTo(ms);
					asmBytes=ms.ToArray();
				}
				return Assembly.Load(asmBytes);
			}

			using (Stream file = File.Create(path))
				return assemblyProvider(file);
		}

		protected virtual string CalculateHash(ControllerDescriptor controllerDescriptor)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(ms);

				void WriteCustomAttributes(IList<AttributeDescriptor> attributes)
				{
					foreach (AttributeDescriptor ad in attributes)
					{
						sw.Write(ad.Constructor.GetHashCode());
						foreach (object o in ad.ConstructorArgs)
							sw.Write(o);
					}
				}

				sw.Write(controllerDescriptor.GrpcServiceType.GetHashCode());
				sw.Write(controllerDescriptor.BaseType.GetHashCode());
				sw.Write(controllerDescriptor.ServiceName);
				sw.Write(controllerDescriptor.TypeName);
				WriteCustomAttributes(controllerDescriptor.CustomAttributes);

				foreach (MethodDescriptor md in controllerDescriptor.Methods)
				{
					sw.Write(md.MethodInfo.GetHashCode());
					sw.Write(md.Name);
					sw.Write(md.Generate ? 1 : 0);
					WriteCustomAttributes(md.CustomAttributes);

					foreach (ParameterDescriptor pd in md.Parameters)
					{
						sw.Write(pd.ParameterInfo.GetHashCode());
						sw.Write(pd.Name);
						WriteCustomAttributes(pd.CustomAttributes);
					}
				}
				sw.Flush();

				ms.Position=0;
				return string.Concat(SHA1Managed.Create().ComputeHash(ms).Select(x => x.ToString("x2")));
			}
		}

		protected virtual string GetFileName(ControllerDescriptor controllerDescriptor)
			=> CalculateHash(controllerDescriptor);
	}
}
