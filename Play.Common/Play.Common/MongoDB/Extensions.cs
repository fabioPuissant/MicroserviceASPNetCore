using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Common.Settings;

namespace Play.Common.MongoDB
{
 


        public static class Extensions
        {
            public static IServiceCollection AddMongo(this IServiceCollection services)
            {
                // Human readible stored into MongoDB
                BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
                BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

                // Setup the connection for that DB 
                services.AddSingleton(serviceProvider =>
                {
                    var configuration = serviceProvider.GetService<IConfiguration>(); // getting the registerd configuration from appsettings.json 

                    // Configure connection with MongoDB
                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>(); // Get the right DB in the MongoDB
                    Console.WriteLine($"FABIO--SERVICENAME= {serviceSettings.ServiceName}");
                    var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                    var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                    Console.WriteLine($"FABIO--CONECTIONSTR= {serviceSettings.ServiceName}");
                    return mongoClient.GetDatabase(serviceSettings.ServiceName);
                });

                return services;
            }

            public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services,
                                                                string collectionName) where T : IEntity
            {
                services.AddSingleton<IRepository<T>>(serviceProvider =>
                {
                    var database = serviceProvider.GetService<IMongoDatabase>(); // already registerd services before this call accessed by GetService!
                    return new MongoRepository<T>(database, collectionName); // now we manually configed/created the dependency that will be used for a MongoReposity
                });

                return services;
            }
        }
}

