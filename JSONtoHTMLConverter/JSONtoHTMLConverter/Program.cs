using Newtonsoft.Json;
using System;
using System.IO;

namespace JSONtoHTMLConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: JSONtoHTMLConverter.exe input.json output.html");
                return;
            }

            string inputFile = args[0];
            string outputFile = args[1];

            HTMLFile file = new HTMLFile();

            string json = File.ReadAllText(inputFile);

            file.Convert(json, outputFile);
        }
    }
}
