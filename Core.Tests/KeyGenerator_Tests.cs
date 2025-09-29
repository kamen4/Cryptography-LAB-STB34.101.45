using System.Numerics;
using Xunit;

namespace Core.Tests;

public class KeyGenerator_Tests
{
    [Fact]
    public void GenerateKey_ReturnsValidPrivateAndPublicKey()
    {
        for (int i = 0; i < 100; i++)
        {
            var curve = EllipticCurve.GetStandardCurve();
            var (d, Q) = KeyGenerator.GenerateKey(curve);

            Assert.True(d > 0 && d < curve.Q);
            Assert.True(curve.IsOnCurve(Q));
            Assert.False(Q.IsInfinity);
        }
    }

    [Fact]
    public void CheckKey_ReturnsTrue_ForValidPublicKey()
    {
        for (int i = 0; i < 100; i++)
        {
            var curve = EllipticCurve.GetStandardCurve();
            var (_, Q) = KeyGenerator.GenerateKey(curve);

            Assert.True(KeyGenerator.CheckKey(Q, curve));
        }
    }

    [Fact]
    public void CheckKey_ReturnsFalse_ForPointNotOnCurve()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var invalidQ = new ECPoint(123, 456);

        Assert.False(KeyGenerator.CheckKey(invalidQ, curve));
    }

    [Fact]
    public void CheckKey_ReturnsFalse_ForOutOfRangeCoordinates()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var outOfRangeQ = new ECPoint(curve.P + 1, curve.P + 1);

        Assert.False(KeyGenerator.CheckKey(outOfRangeQ, curve));
    }

    [Fact]
    public void CheckKey_ReturnsFalse_ForZeroCoordinates()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var zeroQ = new ECPoint(0, 0);

        Assert.False(KeyGenerator.CheckKey(zeroQ, curve));
    }
}
