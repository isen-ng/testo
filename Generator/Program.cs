using System.IO;

namespace Generator
{
    static class Program
    {
        private static void Main(string[] args)
        {
            Generate(1000);
            Generate(10000);
            Generate(100000);
        }

        private static void Generate(int count)
        {
            using (var stream = new FileStream($"data.{count}.json", FileMode.Create))
            {
                Generator.Generate(count, stream);
                stream.Flush();
            }
        }
    }
}