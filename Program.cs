using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TeamcityNUnitResultParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.Equals(args[0], "-help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Usage: <FILE_PATH>");
                return;
            }

            var filename = args[0];
            var filePath = Directory.GetCurrentDirectory() + filename;

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File: {filePath} doesn't exists");
                return;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var xmlDocument = XDocument.Load(stream);

                foreach (var testCase in xmlDocument.XPathSelectElements("//test-case"))
                {
                    var testName = testCase.Attribute("fullname")?.Value ?? "";
                    var result = testCase.Attribute("result")?.Value ?? "";

                    Console.WriteLine($"##teamcity[testStarted name='{testName}']");

                    if (result.Equals("Skipped", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"##teamcity[testIgnored name='{testName}' message='Ignored']");
                    }
                    else if (result.Equals("Inconclusive", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"##teamcity[testIgnored name='{testName}' message='Inconclusive']");
                    }
                    else if (result.Equals("Passed", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Console.WriteLine($"##teamcity[testFailed name='{testName}' message='Failed' details='See {filename}']");
                    }

                    Console.WriteLine($"##teamcity[importData type='nunit' path='{filename}']");
                }
            }
        }
    }
}
