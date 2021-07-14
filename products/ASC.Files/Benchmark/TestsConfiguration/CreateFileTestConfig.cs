﻿
using ASC.Files.Benchmark.TestsConfiguration;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace ASC.Files.Benchmark.TestsConfiguration
{
    public class CreateFileTestConfig : ConfigurationBase
    {
        public CreateFileTestConfig()
        {
            AddJob(Job.Default
                .WithStrategy(BenchmarkDotNet.Engines.RunStrategy.Monitoring)
                .WithToolchain(InProcessEmitToolchain.Instance)
                .WithIterationCount(int.Parse(appConfig["CreateFileTest:IterationCount"])));
        }
    }
}
