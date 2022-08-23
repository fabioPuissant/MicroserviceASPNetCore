using System;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Play.Common.Settings;

namespace Play.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection services)
        {
            
            // Configure RabbitMQ Message Broker
            services.AddMassTransit(configure =>
            {
                // Configure the consumers of the message bus (RabbitMQ)
                configure.AddConsumers(Assembly.GetEntryAssembly());
                
                configure.UsingRabbitMq((context, configurator) => {
                    var configuration = context.GetService<IConfiguration>(); // getting the registerd configuration from appsettings.json 
                                                                        // Configure connection with MongoDB
                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); // Get the right DB in the MongoDB

                    var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                    configurator.Host(rabbitMQSettings.Host); // configure where RabbitMQ Lives (is hosted --> here = localhost)
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                });
            });

            // start MassTransit hosted services (starts the RabbitMQ Service message BUS)
            services.Configure<MassTransitHostOptions>(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(30);
                options.StopTimeout = TimeSpan.FromMinutes(1);
            });

            return services;
        }
    }
}

