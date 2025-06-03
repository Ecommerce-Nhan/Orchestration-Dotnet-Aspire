var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");  

var apiGateway = builder.AddProject<Projects.APIGateway>("api-gateway");
var authService = builder.AddProject<Projects.AuthService>("auth-service").WithReference(redis);
var productService = builder.AddProject<Projects.ProductService_Api>("product-service").WithReference(redis);
var userService = builder.AddProject<Projects.UserService_Api>("user-service").WithReference(redis);

builder.Build().Run();