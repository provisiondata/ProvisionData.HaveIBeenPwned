namespace HaveIBeenPwnd
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(String[] args)
        {
            if (args.Length != 2)
            {
                Usage();
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.Error.WriteLine($"File Not Found: {args[0]}");
                Usage();
                return;
            }
            if (!File.Exists(args[1]))
            {
                Console.Error.WriteLine($"File Not Found: {args[1]}");
                Usage();
                return;
            }

            var hashSet = await LoadUsersAsync(args[0]);

            using (var pwnd = new StreamReader(args[1]))
            {
                var timer = Stopwatch.StartNew();
                Int64 count = 0;
                String line = null;
                do
                {
                    count++;
                    line = await pwnd.ReadLineAsync();
                    if (line is null)
                        break;
                    var split = line.Trim().ToUpperInvariant().Split(':');
                    if (hashSet.ContainsKey(split[0]))
                    {
                        Console.WriteLine();
                        Console.WriteLine(split[0] + "," + split[1] + "," + String.Join(',', hashSet[split[0]]));
                    }
                    if (count % 1000000 == 0)
                        Console.Error.Write(".");
                }
                while (line is not null);
                timer.Stop();
                Console.Error.WriteLine();
                Console.Error.WriteLine($"Scanned {count} hashes in {timer}.");
            }
        }

        private static void Usage()
        {
            Console.Error.WriteLine("HaveIBeenPwnd <hashes.txt> <pwned.txt>");
            Console.Error.WriteLine("hashes.txt -> One line per hash, formatted as USER:HASH");
            Console.Error.WriteLine("pwned.txt  -> One line per hash, formatted as HASH:FREQ");
            Console.Error.WriteLine("STDOUT     => <HASH>:<FREQ>,<pwnduser>[,pwnduser, ...]<CRLF>");
            Console.Error.WriteLine("STDERR     => ........etc. Each . is 1,000,000 hashes.");
        }

        private static async Task<Dictionary<String, IList<String>>> LoadUsersAsync(String v)
        {
            Int64 count = 0;
            var hashesFile = await File.ReadAllLinesAsync(v);
            var hashSet = new Dictionary<String, IList<String>>();
            foreach (var line in hashesFile)
            {
                count++;
                if (String.IsNullOrWhiteSpace(line.Trim()) || line.IndexOf(':') < 0)
                {
                    Console.Error.WriteLine($"Line {count}: INVALID => {line}");
                    continue;
                }

                var split = line.Trim().ToUpperInvariant().Split(':');
                if (hashSet.ContainsKey(split[1]))
                {
                    hashSet[split[1]].Add(split[0]);
                }
                else
                {
                    hashSet.Add(split[1], new List<String>() { split[0] });
                }
            }
            return hashSet;
        }
    }
}
