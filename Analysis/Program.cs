/**
 * Author: Morten Miland Andersen
 * Email: 10407527@ucn.dk
 * Professionsbachelor Softwareudvikling, 1. Semester
 * Professionshøjskolen UCN, Sofiensdalsvej 60, 9200 Aalborg, Danmark
 * Code for Extended Abstract "Undersøgelse af tids- og hukommelseseffektivitet af primtalsalgoritmer implementeret i C#"
 * Keywords: prime number, algorithm, benchmark, C#, Trial Division, Sieve of Eratosthenes, Dijkstra
 * Date: 25-04-2024
 */

using System.Diagnostics;
//custom Tuple to use as return type
using BenchmarkResult = (long ElapsedTime, System.TimeSpan CpuTime, long MemoryUsage);

class Program
{
    #region MAIN
    static void Main(string[] args)
    {
        //BENCHMARK SETUP
        int _numberOfPrimes = 100_000;          //<---- change benchmark parameters
        int _warmup_cycles = 5;                 //<---- change benchmark parameters
        int _benchmark_cycles = 10;             //<---- change benchmark parameters
        Stopwatch _stopwatch = new();
        string _folder = @"c:\repos\data\";     //<---- change to wanted destination folder (for .txt with benchmark data)

        SystemInfo();                           //used to get Systeminfo printed in the Console App

        Console.WriteLine($"Configuration: No. of Primes: {_numberOfPrimes}, Warmup Cycles: {_warmup_cycles}, Benchmark Cycles: {_benchmark_cycles}\n");

        //WARMUP
        Console.WriteLine($"¤¤¤ WARMUP STARTED ¤¤¤\n");                                                             //console feedback for user
        _ = BenchmarkLoop(_warmup_cycles, _numberOfPrimes, _stopwatch, SieveOfEratosthenes);
        _ = BenchmarkLoop(_warmup_cycles, _numberOfPrimes, _stopwatch, TrialDivision);
        _ = BenchmarkLoop(_warmup_cycles, _numberOfPrimes, _stopwatch, Dijkstra);
        Console.WriteLine($"¤¤¤ WARMUP FINISHED ¤¤¤\n");                                                            //console feedback for user

        //RUNNING BENCHMARK(S)
        Console.WriteLine($"¤¤¤ BENCHMARK STARTED ¤¤¤\n");                                                          //console feedback for user
        var metrics_for_Sieve = BenchmarkLoop(_benchmark_cycles, _numberOfPrimes, _stopwatch, SieveOfEratosthenes);
        var metrics_for_Trial = BenchmarkLoop(_benchmark_cycles, _numberOfPrimes, _stopwatch, TrialDivision);
        var metrics_for_Dijkstra = BenchmarkLoop(_benchmark_cycles, _numberOfPrimes, _stopwatch, Dijkstra);
        Console.WriteLine($"¤¤¤ BENCHMARK FINISHED ¤¤¤\n");                                                         //console feedback for user

        //PRINT METRICS FROM BENCHMARK(S)
        Console.WriteLine($"¤¤¤ BENCHMARK RESULTS START ¤¤¤\n");                                                    //console feedback for user
        PrintBenchmark("Sieve of Eratosthenes", metrics_for_Sieve);
        PrintBenchmark("Trial Division", metrics_for_Trial);
        PrintBenchmark("Dijkstra", metrics_for_Dijkstra);
        Console.WriteLine($"¤¤¤ BENCHMARK RESULTS END ¤¤¤\n");                                                      //console feedback for user
        
        //SAVE BENCHMARKS TO FILE
        SaveToFile("Sieve", _folder, _numberOfPrimes, _warmup_cycles, _benchmark_cycles, metrics_for_Sieve);
        SaveToFile("Trial", _folder, _numberOfPrimes, _warmup_cycles, _benchmark_cycles, metrics_for_Trial);
        SaveToFile("Dijkstra", _folder, _numberOfPrimes, _warmup_cycles, _benchmark_cycles, metrics_for_Dijkstra);

        Console.ReadLine();                                                                                         //to prevent "press key to close" in console when finished
    }
    #endregion //MAIN

    #region BENCHMARK METHOD

    delegate List<int> AlgorithmMethodDelegate(int numberOfPrimes);

    static BenchmarkResult Benchmark(int numberOfPrimes, Stopwatch stopwatch, AlgorithmMethodDelegate method)
    {
        stopwatch.Start();
        _ = method(numberOfPrimes);                                     //<---- change - comment/uncomment if "check" below is used to check last 20 primes from algorithm
        //var result = method(numberOfPrimes);                          //<---- change - uncomment/comment if "check" below is used to check last 20 primes from algorithm
        stopwatch.Stop();
        //var check = result.GetRange(numberOfPrimes - 20, 20);         //<---- change - prints out last 20 primes of the result set
        //Console.WriteLine("\n");                                      //<---- change
        //foreach (int i in check)                                      //<---- change
        //{                                                             //<---- change
        //    Console.WriteLine(i);                                     //<---- change
        //}                                                             //<---- change

        long elapsedTime = stopwatch.ElapsedMilliseconds;
        TimeSpan cpuTime = Process.GetCurrentProcess().TotalProcessorTime;      //CURRENTLY NOT USED
        long memoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024;     //convert to KB

        return (elapsedTime, cpuTime, memoryUsage);
    }

    static List<BenchmarkResult> BenchmarkLoop(int cycles, int numberOfPrimes, Stopwatch stopwatch, AlgorithmMethodDelegate method)
    {
        List<BenchmarkResult> BenchmarkResultsList = new(cycles);

        Console.WriteLine($"Running {cycles} Cycles... ");              //console feedback for user
        for (int i = 1; i <= cycles; i++)
        {
            Console.Write($"#{i} ");                                    //console feedback for user

            GC.Collect();
            stopwatch.Reset();
            var result = Benchmark(numberOfPrimes, stopwatch, method);
            stopwatch.Stop();
            BenchmarkResultsList.Add(result);
            GC.Collect();
        }
        Console.WriteLine("\n");

        return BenchmarkResultsList;
    }

    static void PrintBenchmark(string nameOfAlgorithm, List<BenchmarkResult> benchmarkResultsList)
    {
        Console.WriteLine(nameOfAlgorithm);
        foreach (var result in benchmarkResultsList)
        {
            //Console.WriteLine($"Time Elapsed: {result.Item1} ms, CPU Time Elapsed: {result.Item2}, Memory Usage: {result.Item3} KB");
            Console.WriteLine($"Time Elapsed: {result.Item1} ms, Memory Usage: {result.Item3} KB");
        }
        var elapsedTimes = benchmarkResultsList.Select(t => t.ElapsedTime).ToList();
        double avgTime = elapsedTimes.Average();
        double varianceTime = elapsedTimes.Select(t => (t - avgTime) * (t - avgTime)).Sum() / elapsedTimes.Count();
        double stdTime = Math.Sqrt(varianceTime);
        Console.WriteLine($"\nTIME average: {avgTime:F1} ms, min: {elapsedTimes.Min()} ms, max: {elapsedTimes.Max()} ms, standard deviation: {stdTime:F2} ms");

        var memoryUsage = benchmarkResultsList.Select(m => m.MemoryUsage).ToList();
        double avgMemory = memoryUsage.Average();
        double varianceMemory = memoryUsage.Select(m => (m - avgMemory) * (m - avgMemory)).Sum() / elapsedTimes.Count();
        double stdMemory = Math.Sqrt(varianceMemory);
        Console.WriteLine($"MEMORY average: {avgMemory:F1} KB, min: {memoryUsage.Min()} KB, max: {memoryUsage.Max()} KB, standard deviation: {stdMemory:F2} KB\n");
    }

    static void SaveToFile(string nameOfAlgorithm, string folder, int numberOfPrimes, int warmupCycles, int benchmarkCycles, List<BenchmarkResult> benchmarkData)
    {
        string filename = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_" + nameOfAlgorithm + "_" + numberOfPrimes + ".txt";
        string fullName = folder + filename;
        using (StreamWriter writer = new StreamWriter(fullName))
        {
            writer.WriteLine($"Configuration: No. of Primes: {numberOfPrimes}, Warmup Cycles: {warmupCycles}, Benchmark Cycles: {benchmarkCycles}");
            foreach (var line in benchmarkData)
            {
                writer.WriteLine($"{line.ElapsedTime}, {line.MemoryUsage}");
            }
            Console.WriteLine($"File saved. {fullName}");
        }
    }
    static void SystemInfo() //from "Microbenchmarks in Java and C#" by Peter Sestoft @ Version 0.8.0 of 2015-09-16
    {
        Console.WriteLine("# OS {0}",
        Environment.OSVersion.VersionString);
        Console.WriteLine("# .NET vers. {0}",
        Environment.Version);
        Console.WriteLine("# 64-bit OS {0}",
        Environment.Is64BitOperatingSystem);
        Console.WriteLine("# 64-bit proc {0}",
        Environment.Is64BitProcess);
        Console.WriteLine("# CPU {0}; {1} \"procs\"",
        Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
        Environment.ProcessorCount);
        Console.WriteLine("# Date {0:s}", DateTime.Now);
        Console.WriteLine();
    }
    #endregion //BENCHMARK METHOD

    #region ALGORITHM METHODS
    //TRIAL DIVISION
    public static List<int> TrialDivision(int numberOfPrimes)
    {
        List<int> primes = new(numberOfPrimes);                         // int size [-2,147,483,648 to 2,147,483,647]
        int candidate = 2;

        while (primes.Count < numberOfPrimes)
        {
            if (IsPrime(candidate))                                     //check using IsPrime method below (TRUE/FALSE)
            {
                primes.Add(candidate);                                  //(if TRUE then) add prime to list
            }
            candidate++;                            // er der forskel mellem  ++  og  += 1    og  candidate = candidate +1?
        }
        return primes;
    }

    private static bool IsPrime(int candidate)
    {
        if (candidate <= 1) return false;                               //assumption / lemma?
        if (candidate == 2 || candidate == 3) return true;              //assumption / lemma? (3 added to make division loop simpler)

        int sqrtCandidate = (int)Math.Floor(Math.Sqrt(candidate));      //assumption / lemma?
        //check divisors
        for (int divisor = 2; divisor <= sqrtCandidate; divisor++)
        {
            if (candidate % divisor == 0) return false;                 //not prime
        }
        return true;                                                    //prime
    }

    //SIEVE OF ERATOSTHENES
    public static List<int> SieveOfEratosthenes(int numberOfPrimes)     // int size [-2,147,483,648 to 2,147,483,647]
    {
        //bool array default value(s) = FALSE
        bool[] sieve = new bool[numberOfPrimes * 20];                   // array size based on https://t5k.org/howmany.html#better (good up to approx 50e10^6 (50 millions) prime numbers) (numbers reach about 1e9)
        List<int> primes = new(numberOfPrimes);
        int candidate = 2;

        while(primes.Count < numberOfPrimes)
        {
            if (!sieve[candidate])                                      //if candidate index @ sieve is "FALSE" then...
            {
                primes.Add(candidate);                                  //...candidate is prime

                for(int multiple = (candidate + candidate); multiple < sieve.Length; multiple += candidate)
                {
                    sieve[multiple] = true;                            //sets all multiples (of candidate) to TRUE
                }
            }
            candidate++;                            // er der forskel mellem  ++  og  += 1    og  candidate = candidate +1?
        }
        return primes;
    }

    //DIJKSTRA
    public static List<int> Dijkstra(int numberOfPrimes)                                // int size [-2,147,483,648 to 2,147,483,647]
    {
        List<int> primes = new(numberOfPrimes) { 2 };                                   //assumption / lemma? adding 2 to prime list to avoid checking all even candidates
        int sqrtNumberOfPrimes = (int)Math.Floor(Math.Sqrt((double)numberOfPrimes));    //assumption / lemma?
        Dictionary<int, int> multiples = new(sqrtNumberOfPrimes);

        for (int candidate = 3; primes.Count < numberOfPrimes; candidate += 2)
        {
            if (multiples.TryGetValue(candidate, out int increment))                     //TRUE if the candidate exists as a multiple (key)
            {
                multiples.Remove(candidate);
                int newMultiple = candidate + increment;                                //remove old multiple (key+value), create new key by increasing multiple (candidate) by increment (value)
                while (multiples.TryGetValue(newMultiple, out int _))
                {                  //check if new multiple already exist (for other prime number)
                    newMultiple += increment;                                           //if TRUE then add increment and try again
                }
                multiples.TryAdd(newMultiple, increment);
                //Console.WriteLine($"old mult (key): {candidate}, new mult (key): {newMultiple}, increment (value): {increment}");   //<---- change - SPAM (use LOW VALUE for _numberOfPrimes)
            }
            else
            {
                primes.Add(candidate);
                //Console.WriteLine($"new prime added to list: {candidate}");             //<---- change - SPAM (use LOW VALUE for _numberOfPrimes)
                if (multiples.Count < sqrtNumberOfPrimes)
                {
                    multiples.TryAdd(candidate * candidate, candidate + candidate);     //multiple = square of candidate ; increment is always double the original prime number
                }
            }
        }
        return primes;
    }
    #endregion //ALGORITHM METHODS

}