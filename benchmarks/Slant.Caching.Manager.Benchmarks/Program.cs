using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Slant.Caching.Manager.Benchmarks
{
    [ExcludeFromCodeCoverage]
    public class CacheManagerBenchConfig : ManualConfig
    {
        public CacheManagerBenchConfig()
        {
            Add(Job.MediumRun
                .With(Platform.X64));
        }
    }

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {

            do
            {
                var config = ManualConfig.Create(DefaultConfig.Instance)
                    .With(BenchmarkDotNet.Analysers.EnvironmentAnalyser.Default)
                    .With(BenchmarkDotNet.Exporters.MarkdownExporter.GitHub)
                    .With(BenchmarkDotNet.Diagnosers.MemoryDiagnoser.Default)
                    .With(StatisticColumn.Mean)
                    .With(StatisticColumn.Median)
                    //.With(StatisticColumn.Min)
                    //.With(StatisticColumn.Max)
                    .With(StatisticColumn.StdDev)
                    .With(StatisticColumn.OperationsPerSecond)
                    .With(BaselineRatioColumn.RatioMean)
                    .With(RankColumn.Arabic);
                    
                    /*
                    .With(Job.) // or another version you target
                        .WithIterationCount(10)
                        .WithWarmupCount(4)
                        .WithLaunchCount(1));
                        */
                
                BenchmarkSwitcher
                    .FromAssembly(typeof(Program).GetTypeInfo().Assembly)
                    .Run(args, config);

                Console.WriteLine("done!");
                Console.WriteLine("Press escape to exit or any key to continue...");
            } while (Console.ReadKey().Key != ConsoleKey.Escape);
        }
    }
}
