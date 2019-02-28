﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DocumentDbTest.Abstractions;
using Generator;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocumentDbTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // prepare data
            List<Document> data;
            using (var stream = new FileStream("Data/data.100000.json", FileMode.Open))
            {
                var serializer = new JsonSerializer();
                data = serializer.Deserialize<List<Document>>(new JsonTextReader(new StreamReader(stream)));
            }
            
            // prepare writer
            var appSettings = LoadAppSettings(args);
            var provider = LoadProvider(appSettings.GetSection("DocumentStore"));

            // warm up
            await RunWarmUp(provider);

            // do it
            await RunSingleWrite(data, provider);
            await RunSingleRead(data, provider);
        }

        private static async Task RunWarmUp(IProvider provider)
        {
            foreach (var document in Generator.Generator.Generate(2))
            {
                await provider.Store(document.Id, document);
                await provider.Get<Document>(document.Id);
            }
        }


        private static async Task RunSingleWrite(IReadOnlyCollection<Document> data, IProvider provider)
        {
            using (var stream = new StreamWriter( 
                new BufferedStream(
                new FileStream($"single-write.{data.Count}.csv", FileMode.Create))))
            {
                var i = 0;
                foreach (var document in data)
                {
                
                    var stopwatch = Stopwatch.StartNew();
                
                    await provider.Store(document.Id, document);
                
                    stopwatch.Stop();

                    stream.WriteLine($"{i},{stopwatch.Elapsed.TotalMilliseconds}");
                    i++;
                }
                
                stream.Flush();
            }
        }
        
        private static async Task RunSingleRead(IReadOnlyCollection<Document> data, IProvider provider)
        {
            using (var stream = new StreamWriter( 
                new BufferedStream(
                    new FileStream($"single-read.{data.Count}.csv", FileMode.Create))))
            {
                var i = 0;
                foreach (var document in data)
                {
                
                    var stopwatch = Stopwatch.StartNew();
                
                    await provider.Get<Document>(document.Id);
                
                    stopwatch.Stop();

                    stream.WriteLine($"{i},{stopwatch.Elapsed.TotalMilliseconds}");
                    i++;
                }
                
                stream.Flush();
            }
        }
        
        private static IConfiguration LoadAppSettings(string[] args)
        {
            var hostSettings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hostsettings.json")
                .AddEnvironmentVariables("NETCORE_")
                .AddCommandLine(args)
                .Build();

            var environment = hostSettings.GetValue<string>("Environment");

            return new ConfigurationBuilder()
                .AddConfiguration(hostSettings)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .Build();
        }

        private static IProvider LoadProvider(IConfiguration configuration)
        {
            var typeName = configuration.GetValue<string>("$type");
            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException("Could not find type: " + typeName);
            }
            
            return (IProvider) Activator.CreateInstance(type, configuration);
        }
    }
}