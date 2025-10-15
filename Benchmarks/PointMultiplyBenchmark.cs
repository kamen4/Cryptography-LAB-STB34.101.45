using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Core;

namespace Benchmarks;

[MemoryDiagnoser]
[RankColumn]
public class PointMultiplyBenchmark
{
    private EllipticCurve _curve = null!;
    private ECPoint _P = null!;
    private BigInteger _d;

    private List<(int, int)> _chainForChainMethod = null!;

    [GlobalSetup]
    public void Setup()
    {
        _curve = EllipticCurve.GetStandardCurve();
        _P = _curve.G;
        _d = BigInteger.Parse("123456789ABCDEF0123456789ABCDEF0123456789", System.Globalization.NumberStyles.HexNumber);
        _chainForChainMethod = ECPoint.BuildAdditiveChainBerlekamp(_d);
    }

    [Benchmark(Baseline = true, Description = "Binary multiply")]
    public ECPoint MultiplyScalar_Binary()
    {
        return ECPoint.MultiplyScalar(_P, _d, _curve);
    }

    [Benchmark(Description = "NAF multiply")]
    public ECPoint MultiplyScalar_NAF()
    {
        return ECPoint.MultiplyScalarNAF(_P, _d, _curve);
    }

    [Benchmark(Description = "Additive chain multiply prebuild chain")]
    public ECPoint MultiplyScalar_AdditiveChain_PrebuildChain()
    {
        return ECPoint.MultiplyScalarAdditiveChain(_P, _d, _curve, _chainForChainMethod);
    }

    [Benchmark(Description = "Additive chain multiply")]
    public ECPoint MultiplyScalar_AdditiveChain()
    {
        return ECPoint.MultiplyScalarAdditiveChain(_P, _d, _curve);
    }

    [Benchmark(Description = "Window method (w=4)")]
    public ECPoint MultiplyScalar_Window()
    {
        return ECPoint.MultiplyScalarWindow(_P, _d, _curve, 4);
    }

    [Benchmark(Description = "Sliding window (w=4)")]
    public ECPoint MultiplyScalar_SlidingWindow()
    {
        return ECPoint.MultiplyScalarSlidingWindow(_P, _d, _curve, 4);
    }

    [Benchmark(Description = "Jacobian coordinates")]
    public ECPoint MultiplyScalar_Jacobian()
    {
        return ECPoint.MultiplyScalarJacobian(_P, _d, _curve);
    }
}