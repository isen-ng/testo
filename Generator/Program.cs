using System.IO;

namespace Generator
{
    static class Program
    {
        private static void Main(string[] args)
        {
            const int count = 500000;
            using (var stream = new FileStream($"data.{count}.json", FileMode.Create))
            {
                Generator.Generate(count, stream);
                stream.Flush();
            }
        }
    }
}