using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DocumentDbTest.Data
{
    public static class Generator
    {
        public static readonly string[] CollationKey1 = {"key1", "key2"};

        public static void Generate(int count, Stream stream)
        {
            var serializer = new JsonSerializer();
            serializer.Serialize(new StreamWriter(stream), Generate(count));
        }
        
        public static IEnumerable<Document> Generate(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new Document
                {
                    CollationKey1 = CollationKey1[i % CollationKey1.Length],
                    Id = Guid.NewGuid().ToString(),
                    Description = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow
                };
            }
        }
    }
}