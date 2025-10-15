using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using BenchmarkDotNet.Running;
using Benchmarks;
using Core;

namespace ConsoleApp;

internal class Program
{
    static void Main()
    {
        var _ = BenchmarkRunner.Run<PointMultiplyBenchmark>();
    }
}
