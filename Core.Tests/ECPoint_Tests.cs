using System.Numerics;
using Xunit;

namespace Core.Tests;

public class ECPoint_Tests
{
    [Fact]
    public void ToString_ReturnsCoordinates_WhenNotInfinity()
    {
        var point = new ECPoint(1, 2);
        Assert.Equal("(1, 2)", point.ToString());
    }

    [Fact]
    public void ToString_ReturnsInf_WhenInfinity()
    {
        Assert.Equal("Inf", ECPoint.Infinity.ToString());
    }

    [Fact]
    public void Equals_ReturnsTrue_ForIdenticalPoints()
    {
        var p1 = new ECPoint(1, 2);
        var p2 = new ECPoint(1, 2);
        Assert.True(p1.Equals(p2));
    }

    [Fact]
    public void Equals_ReturnsFalse_ForDifferentPoints()
    {
        var p1 = new ECPoint(1, 2);
        var p2 = new ECPoint(2, 1);
        Assert.False(p1.Equals(p2));
    }

    [Fact]
    public void Equals_ReturnsTrue_ForBothInfinity()
    {
        Assert.True(ECPoint.Infinity.Equals(ECPoint.Infinity));
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenComparedWithNull()
    {
        var p = new ECPoint(1, 2);
        Assert.False(p.Equals(null));
    }

    [Fact]
    public void GetHashCode_IsEqual_ForIdenticalPoints()
    {
        var p1 = new ECPoint(1, 2);
        var p2 = new ECPoint(1, 2);
        Assert.Equal(p1.GetHashCode(), p2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_IsNotEqual_ForInfinityAndPoint()
    {
        var p = new ECPoint(1, 2);
        Assert.NotEqual(p.GetHashCode(), ECPoint.Infinity.GetHashCode());
    }

    [Fact]
    public void ModInverse_ReturnsCorrectResult()
    {
        BigInteger a = 3;
        BigInteger p = 11;
        Assert.Equal(4, ECPoint.ModInverse(a, p));
    }

    [Fact]
    public void Add_ReturnsOtherPoint_WhenFirstIsInfinity()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        Assert.Equal(p, ECPoint.Add(ECPoint.Infinity, p, curve));
    }

    [Fact]
    public void Add_ReturnsOtherPoint_WhenSecondIsInfinity()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        Assert.Equal(p, ECPoint.Add(p, ECPoint.Infinity, curve));
    }

    [Fact]
    public void Add_ReturnsInfinity_WhenPointsAreInverse()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var inverse = new ECPoint(p.X, curve.P - p.Y);
        var result = ECPoint.Add(p, inverse, curve);
        Assert.True(result.IsInfinity);
    }

    [Fact]
    public void Add_ReturnsValidPoint_WhenDoubling()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var doubled = ECPoint.Add(p, p, curve);
        Assert.True(curve.IsOnCurve(doubled));
    }

    [Fact]
    public void Add_ReturnsValidPoint_WhenAddingDistinctPoints()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var q = ECPoint.Add(p, p, curve);
        var r = ECPoint.Add(p, q, curve);
        Assert.True(curve.IsOnCurve(r));
    }

    [Fact]
    public void Double_ReturnsValidPoint()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var doubled = ECPoint.Double(p, curve);
        Assert.True(curve.IsOnCurve(doubled));
    }

    [Fact]
    public void Double_EqualsAddWithSelf()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        Assert.Equal(ECPoint.Add(p, p, curve), ECPoint.Double(p, curve));
    }

    [Fact]
    public void MultiplyScalar_ReturnsInfinity_WhenScalarIsZero()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var result = ECPoint.MultiplyScalar(p, 0, curve);
        Assert.True(result.IsInfinity);
    }

    [Fact]
    public void MultiplyScalar_ReturnsPoint_WhenScalarIsOne()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var result = ECPoint.MultiplyScalar(p, 1, curve);
        Assert.Equal(p, result);
    }

    [Fact]
    public void MultiplyScalar_ReturnsDouble_WhenScalarIsTwo()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var result = ECPoint.MultiplyScalar(p, 2, curve);
        Assert.Equal(ECPoint.Double(p, curve), result);
    }

    [Fact]
    public void MultiplyScalar_ReturnsCorrectResult_WhenScalarIsThree()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var p = curve.G;
        var expected = ECPoint.Add(p, ECPoint.Double(p, curve), curve);
        var result = ECPoint.MultiplyScalar(p, 3, curve);
        Assert.Equal(expected, result);
    }
}
