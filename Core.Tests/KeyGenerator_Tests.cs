using System.Numerics;
using System.Security.Cryptography;
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

    [Fact]
    public void GenerateOneTimeKey_ReturnValid_ForStandartParams()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var (d, Q) = KeyGenerator.GenerateKey(curve);
        string msg = "!@#$%^&*()_+QWERTYUIOPASDFGHJKLZXCVBNM<>1234567890-=qwertyuiop[]asdfghjkl;'zxcvbnm,./";
        var k = KeyGenerator.GenerateOneTimeKey(curve.Q, d, SHA256.HashData(msg.Select(c => (byte)c).ToArray()));
        Assert.True(k < curve.Q && k > 0);
    }

    // Тесты из стандарта

    // Г.1. Генерация пары ключей
    [Fact]
    public void GenerateKey_ReturnsExpectedValues_FromStandard()
    {
        var curve = EllipticCurve.GetStandardCurve();
        BigInteger d = MathHelper.GetFromHexString("1F66B5B8 4B733967 4533F032 9C74F218 34281FED 0732429E 0C79235F C273E269");
        var (priv, pub) = KeyGenerator.GenerateKey(curve, d);
        Assert.Equal(d, priv);
        Assert.Equal(MathHelper.GetFromHexString("BD1A5650 179D79E0 3FCEE49D 4C2BD5DD F54CE46D 0CF11E4F F87BF7A8 90857FD0"), pub.X);
        Assert.Equal(MathHelper.GetFromHexString("7AC6A603 61E8C817 3491686D 461B2826 190C2EDA 5909054A 9AB84D2A B9D99A90"), pub.Y);
    }

    // Г.6. Генерация одноразового ключа
    [Fact]
    public void GenerateOneTimeKey_ReturnsExpectedValue_FromStandardTNull()
    {
        var curve = EllipticCurve.GetStandardCurve();
        BigInteger d = MathHelper.GetFromHexString("1F66B5B8 4B733967 4533F032 9C74F218 34281FED 0732429E 0C79235F C273E269");
        byte[] H = MathHelper.GetFromHexString("ABEF9725 D4C5A835 97A367D1 4494CC25 42F20F65 9DDFECC9 61A3EC55 0CBA8C75").ToByteArray(true);
        byte[] theta = MathHelper.GetFromHexString("D61E3A91 0550E3BC AD5BF4F5 26FB8DAA DEA9C132 E0BAEE03 169DF4DF 9BD6C20C").ToByteArray(true);

        var k = KeyGenerator.GenerateOneTimeKey(curve.Q, d, H, null, theta);
        Assert.Equal(MathHelper.GetFromHexString("829614D8 411DBBC4 E1F2471A 40045864 40FD8C95 53FAB6A1 A45CE417 AE97111E"), k);
    }

    // Г.7. Генерация одноразового ключа с заданным t
    [Fact]
    public void GenerateOneTimeKey_ReturnsExpectedValue_FromStandard()
    {
        var curve = EllipticCurve.GetStandardCurve();
        BigInteger d = MathHelper.GetFromHexString("1F66B5B8 4B733967 4533F032 9C74F218 34281FED 0732429E 0C79235F C273E269");
        byte[] H = MathHelper.GetFromHexString("9D02EE44 6FB6A29F E5C982D4 B13AF9D3 E90861BC 4CEF27CF 306BFB0B 174A154A").ToByteArray(true);
        byte[] t = MathHelper.GetFromHexString("BE329713 43FC9A48 A02A885F 194B09A1 7ECDA4D0 1544AF").ToByteArray(true);
        byte[] theta = MathHelper.GetFromHexString("AE443163 32A85C3B 9F6B31EE EADFF088 D30FE507 021AC86A 3EC8E087 4ED33648").ToByteArray(true);

        var k = KeyGenerator.GenerateOneTimeKey(curve.Q, d, H, t, theta);
        Assert.Equal(MathHelper.GetFromHexString("7ADC8713 283EBFA5 47A2AD9C DFB245AE 0F7B968D F0F91CB7 85D1F932 A3583107"), k);
    }
}
