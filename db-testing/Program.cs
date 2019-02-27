using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentDbTest.Abstractions;
using DocumentDbTest.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace DocumentDbTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

//            var count = 10000;
//            using (var stream = new FileStream($"data.{count}.json", FileMode.Create))
//            {
//                Generator.Generate(count + 2, stream);
//                return;
//            }

            // prepare data
            IEnumerable<Document> data;
            using (var stream = new FileStream("data.10000.json", FileMode.Open))
            {
                var serializer = new JsonSerializer();
                data = serializer.Deserialize<List<Document>>(new JsonTextReader(new StreamReader(stream)));
            }
            
            // prepare writer
            var appSettings = LoadAppSettings();
            var provider = LoadProvider(appSettings.GetSection("DocumentStore"));

            // warm up
            await RunWarmUp(provider);

            // do it
            await RunSingleWrite(data, provider);
            await RunSingleRead(data, provider);
        }

        private static async Task RunWarmUp(IProvider provider)
        {
            foreach (var document in Generator.Generate(2))
            {
                await provider.Store(document.Id, document);
                await provider.Get<Document>(document.Id);
            }
        }


        private static async Task RunSingleWrite(IEnumerable<Document> data, IProvider provider)
        {
            using (var stream = new StreamWriter( 
                new BufferedStream(
                new FileStream("single-write.csv", FileMode.Create))))
            {
                var i = 0;
                foreach (var document in data)
                {
                
                    var stopwatch = Stopwatch.StartNew();
                
                    await provider.Store(document.Id, document);
                
                    stopwatch.Stop();

                    stream.WriteLine($"{i},{stopwatch.Elapsed}");
                    i++;
                }    
            }
        }
        
        private static async Task RunSingleRead(IEnumerable<Document> data, IProvider provider)
        {
            using (var stream = new StreamWriter( 
                new BufferedStream(
                    new FileStream("single-read.csv", FileMode.Create))))
            {
                var i = 0;
                foreach (var document in data)
                {
                
                    var stopwatch = Stopwatch.StartNew();
                
                    await provider.Get<Document>(document.Id);
                
                    stopwatch.Stop();

                    stream.WriteLine($"{i},{stopwatch.Elapsed}");
                    i++;
                }    
            }
        }
        
        private static IConfiguration LoadAppSettings()
        {
            var hostSettings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hostsettings.json")
                .AddEnvironmentVariables("NETCORE_")
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