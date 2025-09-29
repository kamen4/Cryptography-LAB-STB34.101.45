using System.Numerics;
using Xunit;

namespace Core.Tests;

public class EllipticCurve_Tests
{
    [Fact]
    public void GetStandardCurve_ReturnsValidCurve()
    {
        var curve = EllipticCurve.GetStandardCurve();
        Assert.NotNull(curve);
        Assert.True(curve.P > 0);
        Assert.True(curve.A > 0);
        Assert.True(curve.B > 0);
        Assert.True(curve.Q > 0);
        Assert.NotNull(curve.G);
    }

    [Fact]
    public void IsOnCurve_ReturnsTrue_ForBasePoint()
    {
        var curve = EllipticCurve.GetStandardCurve();
        Assert.True(curve.IsOnCurve(curve.G));
    }

    [Fact]
    public void IsOnCurve_ReturnsTrue_ForInfinity()
    {
        var curve = EllipticCurve.GetStandardCurve();
        Assert.True(curve.IsOnCurve(ECPoint.Infinity));
    }

    [Fact]
    public void IsOnCurve_ReturnsFalse_ForInvalidPoint()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var invalid = new ECPoint(123, 456);
        Assert.False(curve.IsOnCurve(invalid));
    }

    [Fact]
    public void ComputeBasePoint_ReturnsBasePoint_ForStandartCurve()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var basePoint = EllipticCurve.ComputeBasePoint(curve.P, curve.B);
        Assert.True(curve.G.Equals(basePoint));
    }

    [Fact]
    public void GetFromHexString_ParsesCorrectly()
    {
        string hex = "0A00";
        var value = EllipticCurve.GetFromHexString(hex);
        Assert.Equal(10, value);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var p = new BigInteger(17);
        var a = new BigInteger(2);
        var b = new BigInteger(3);
        var seed = new BigInteger(3);
        var q = new BigInteger(19);
        var g = new ECPoint(1, 2);
        var curve = new EllipticCurve(p, a, b, seed, q, g);

        Assert.Equal(p, curve.P);
        Assert.Equal(a, curve.A);
        Assert.Equal(b, curve.B);
        Assert.Equal(seed, curve.Seed);
        Assert.Equal(q, curve.Q);
        Assert.Equal(g, curve.G);
    }
}
