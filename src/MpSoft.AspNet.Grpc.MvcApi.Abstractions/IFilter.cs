namespace MpSoft.AspNet.Grpc.MvcApi
{
	public interface IFilter
	{
		void Filter(ControllerDescriptor controllerDescriptor);
	}
}
