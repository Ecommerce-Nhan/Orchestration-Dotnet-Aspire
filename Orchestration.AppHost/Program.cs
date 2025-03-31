var builder = DistributedApplication.CreateBuilder(args);

var apiGateway = builder.AddProject<Projects.APIGateway>("api-gateway");
var authService = builder.AddProject<Projects.AuthService>("auth-service");
var productService = builder.AddProject<Projects.ProductService_Api>("product-service");
var userService = builder.AddProject<Projects.UserService_Api>("user-service");

builder.Build().Run();
