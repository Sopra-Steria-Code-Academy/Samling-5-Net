var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("eventdriven");

builder.AddProject<Projects.EventDrivenApp>("eventdrivenapp")
    .WithReference(rabbitmq)
    .WithReference(postgres)
    .WaitFor(rabbitmq)
    .WaitFor(postgres);

builder.Build().Run();
