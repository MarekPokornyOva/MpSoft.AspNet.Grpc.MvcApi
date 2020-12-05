# Installation

1. create new gRPC service project
2. add MpSoft.AspNet.Grpc.MvcApi nuget package
3. the legacy client applications might be able to communicate only via HTTP 1.1. To configure the application to use both HTTP 1.1 and HTTP/2, set Kernel endpoint defaults (optional)
```
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols":  "Http1AndHttp2"
    }
  }
```
4. add standard MVC controllers' services
```
	services.AddControllers();
```
5. add gRPC controllers generator
```
	services.AddControllers()
		.AddGrpcHttp();
```
6. add gRPC services which should be called by REST
```
	services.AddControllers()
		.AddGrpcHttp(options=>
		{
			options.GrpcServices.Add<GreeterService>();
		});
	services.AddSingleton<GreeterService>();
```
7. add gRPC controllers generator filters to modify default behavior (optional)
```
	services.AddControllers()
		.AddGrpcHttp(options=>
		{
			options.GrpcServices.Add<GreeterService>();
			options.Filters.Add<HttpMethodFilter>();
		});
```
8. add compile cache (optional)
```
	services.AddControllers()
		.AddGrpcHttp(options=>
		{
			options.GrpcServices.Add<GreeterService>();
			options.CompileServices.AddCacheWithJellequin(new DiskCache(_env.ContentRootPath))
		});
```
9. add standard Swagger generator (optional)
```
	services.AddSwaggerGen(c =>
	{
		c.SwaggerDoc("v1",new OpenApiInfo { Title="gRPC HTTP API Example",Version="v1" });
	});
```
and
```
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json","gRPC HTTP API Example V1");
	});
```
10. add controllers' endpoints
```
	app.UseEndpoints(endpoints =>
	{
		endpoints.MapGrpcService<GreeterService>();
		endpoints.MapControllers();
	});
```
See the samples for detail understanding of the usage and configuration.