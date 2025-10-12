using System.Numerics;
using System.Security.Cryptography;
using System.Text;


namespace Core.Tests;

public class EDS_Tests
{
    [Fact]
    public void GenerateAndCheck_MultipleRandomKeysAndMessages_ShouldPass()
    {
        var curve = EllipticCurve.GetStandardCurve();
        for (int i = 0; i < 10; i++)
        {
            var (d, Q) = KeyGenerator.GenerateKey(curve);
            byte[] message = RandomNumberGenerator.GetBytes(128);
            var S = EDS.Generate(curve, message, d);
            Assert.True(EDS.Check(curve, message, S, Q));
        }
    }

    [Fact]
    public void Check_ReturnsFalse_ForWrongSignatureLength()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var (_, Q) = KeyGenerator.GenerateKey(curve);
        byte[] message = RandomNumberGenerator.GetBytes(128);
        var S = RandomNumberGenerator.GetBytes(10);
        Assert.False(EDS.Check(curve, message, S, Q));
    }

    [Fact]
    public void Check_ReturnsFalse_ForWrongPublicKey()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var (d, Q) = KeyGenerator.GenerateKey(curve);
        byte[] message = RandomNumberGenerator.GetBytes(128);
        var S = EDS.Generate(curve, message, d);
        var wrongQ = new ECPoint(Q.X + 1, Q.Y + 1);
        Assert.False(EDS.Check(curve, message, S, wrongQ));
    }

    [Fact]
    public void Check_ReturnsFalse_ForModifiedSignature()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var (d, Q) = KeyGenerator.GenerateKey(curve);
        byte[] message = RandomNumberGenerator.GetBytes(128);
        var S = EDS.Generate(curve, message, d);
        S[0] ^= 0x01;

        Assert.False(EDS.Check(curve, message, S, Q));
    }

    [Fact]
    public void Check_ReturnsFalse_ForModifiedMessage()
    {
        var curve = EllipticCurve.GetStandardCurve();
        var (d, Q) = KeyGenerator.GenerateKey(curve);
        byte[] message = RandomNumberGenerator.GetBytes(128);
        var S = EDS.Generate(curve, message, d);

        var messageMod = (byte[])message.Clone();
        messageMod[2] ^= 0b01000;

        Assert.False(EDS.Check(curve, messageMod, S, Q));
    }

    // Тесты из стандарта

    static readonly byte[] belt_hash_OID = MathHelper.GetFromHexString("06092A7000020022651F51").ToByteArray(true);
    
    [Fact]
    public void Generate_ReturnsValid_ForStandart()
    {
        var X = MathHelper.GetFromHexString("B194BAC8 0A08F53B 366D008E 58");
        var H = MathHelper.GetFromHexString("ABEF9725 D4C5A835 97A367D1 4494CC25 42F20F65 9DDFECC9 61A3EC55 0CBA8C75");
        var k = MathHelper.GetFromHexString("4C0E74B2 CD5811AD 21F23DE7 E0FA742C 3ED6EC48 3C461CE1 5C33A77A A308B7D2");
        var S = MathHelper.GetFromHexString("E36B7F03 77AE4C52 4027C387 FADF1B20 CE72F153 0B71F2B5 FD3A8C58 4FE2E1AE D20082E3 0C8AF650 11F4FB54 649DFD3D");
        var d = MathHelper.GetFromHexString("1F66B5B8 4B733967 4533F032 9C74F218 34281FED 0732429E 0C79235F C273E269");

        var curve = EllipticCurve.GetStandardCurve();
        var S_gen = EDS.Generate(curve, X.ToByteArray(true), d, (belt_hash_OID, H.ToByteArray(true), k));
        Assert.Equal(S, new BigInteger(S_gen, true));
    }

    [Fact]
    public void Check_ReturnsTrue_ForStandart()
    {
        var S = MathHelper.GetFromHexString("47A63C8B 9C936E94 B5FAB3D9 CBD78366 290F3210 E163EEC8 DB4E921E 8479D413 8F112CC2 3E6DCE65 EC5FF21D F4231C28");
        var H = MathHelper.GetFromHexString("9D02EE44 6FB6A29F E5C982D4 B13AF9D3 E90861BC 4CEF27CF 306BFB0B 174A154A");
        var Q = new ECPoint(
            MathHelper.GetFromHexString("BD1A5650 179D79E0 3FCEE49D 4C2BD5DD F54CE46D 0CF11E4F F87BF7A8 90857FD0"),
            MathHelper.GetFromHexString("7AC6A603 61E8C817 3491686D 461B2826 190C2EDA 5909054A 9AB84D2A B9D99A90"));
        var curve = EllipticCurve.GetStandardCurve();

        Assert.True(EDS.Check(curve, [], S.ToByteArray(true), Q, (belt_hash_OID, H.ToByteArray(true))));
    }

    [Fact]
    public void Check_ReturnsFalse_ForStandartBitDiff()
    {
        var S =          MathHelper.GetFromHexString("47A63C8B 9C936E94 B5FAB3D9 CBD78366 290F3210 E163EEC8 DB4E921E 8479D413 8F112CC2 3E6DCE65 EC5FF21D F4231C28");
        var S_bit_diff = MathHelper.GetFromHexString("47A63C8B 9C936E94 B5FAB3D9 CBD78366 290F3210 E163EEC8 DB4E921E 8479D413 8F112CC2 3E6DCE65 EC5FF21D F4231C27");
        var H =          MathHelper.GetFromHexString("9D02EE44 6FB6A29F E5C982D4 B13AF9D3 E90861BC 4CEF27CF 306BFB0B 174A154A");
        var H_bit_diff = MathHelper.GetFromHexString("9D02EE44 6FB6A29F E5C982D4 B13AF9D3 E90861BC 4CEF27CF 306BFB0B 174A154B");
        var Q = new ECPoint(
                         MathHelper.GetFromHexString("BD1A5650 179D79E0 3FCEE49D 4C2BD5DD F54CE46D 0CF11E4F F87BF7A8 90857FD0"),
                         MathHelper.GetFromHexString("7AC6A603 61E8C817 3491686D 461B2826 190C2EDA 5909054A 9AB84D2A B9D99A90"));
        var curve = EllipticCurve.GetStandardCurve();
        
        Assert.False(EDS.Check(curve, [], S_bit_diff.ToByteArray(true), Q, (belt_hash_OID, H.ToByteArray(true))));
        Assert.False(EDS.Check(curve, [], S.ToByteArray(true),          Q, (belt_hash_OID, H_bit_diff.ToByteArray(true))));
        Assert.False(EDS.Check(curve, [], S_bit_diff.ToByteArray(true), Q, (belt_hash_OID, H_bit_diff.ToByteArray(true))));
    }
}
