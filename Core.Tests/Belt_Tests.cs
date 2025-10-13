using System.Security.Cryptography;

namespace Core.Tests;

public class Belt_Tests
{
    [Fact]
    public void Block_Encrypt_ShouldMatchReference_Multiple()
    {
        for (int i = 0; i < 10; i++)
        {
            byte[] X = RandomNumberGenerator.GetBytes(16);
            byte[] K = RandomNumberGenerator.GetBytes(32);
            byte[] Y_lib = new byte[16];
            var Y_own = Belt.Block(X, K);
            var err = TZICrypt.tzi_belt_ecb_encr(X, (uint)X.Length, K, Y_lib);
            Assert.Equal(Err_type.TZI_OK, err);
            Assert.Equal(Y_lib, Y_own);
        }
    }

    [Fact]
    public void Hash_ShouldMatchReference_Multiple()
    {
        for (int i = 0; i < 10; i++)
        {
            byte[] X = RandomNumberGenerator.GetBytes(128);
            byte[] Y_lib = new byte[32];
            var Y_own = Belt.Hash(X);
            var err = TZICrypt.tzi_belt_hash(X, (uint)X.Length, Y_lib);
            Assert.Equal(Err_type.TZI_OK, err);
            Assert.Equal(Y_lib, Y_own);
        }
    }

    [Fact]
    public void Block_KnownVectors_ShouldMatchReference()
    {
        for (int i = 0; i < 5; i++)
        {
            byte[] X = Enumerable.Range(0, 16).Select(j => (byte)(i * 16 + j)).ToArray();
            byte[] K = Enumerable.Range(0, 32).Select(j => (byte)(255 - i * 32 - j)).ToArray();
            byte[] Y_lib = new byte[16];
            var Y_own = Belt.Block(X, K);
            var err = TZICrypt.tzi_belt_ecb_encr(X, (uint)X.Length, K, Y_lib);
            Assert.Equal(Err_type.TZI_OK, err);
            Assert.Equal(Y_lib, Y_own);
        }
    }

    // Тесты из стандарта

    [Fact]
    public void Block_ReturnsValid_ForStandart()
    {
        var X = Convert.FromHexString("B194BAC8 0A08F53B 366D008E 584A5DE4".Replace(" ", ""));
        var K = Convert.FromHexString("E9DEE72C 8F0C0FA6 2DDB49F4 6F739647 06075316 ED247A37 39CBA383 03A98BF6".Replace(" ", ""));
        var Y = Convert.FromHexString("69CCA1C9 3557C9E3 D66BC3E0 FA88FA6E".Replace(" ", ""));

        var Y_gen = Belt.Block(X, K);

        Assert.Equal(Y, Y_gen);
    }

    [Fact]

    public void Compress_ReturnsValid_ForStandart()
    {
        var X = Convert.FromHexString("B194BAC8 0A08F53B 366D008E 584A5DE4 8504FA9D 1BB6C7AC 252E72C2 02FDCE0D 5BE3D612 17B96181 FE6786AD 716B890B 5CB0C0FF 33C356B8 35C405AE D8E07F99".Replace(" ", ""));
        var S = Convert.FromHexString("46FE7425 C9B181EB 41DFEE3E 72163D5A".Replace(" ", ""));
        var Y = Convert.FromHexString("ED2F5481 D593F40D 87FCE37D 6BC1A2E1 B7D1A2CC 975C82D3 C0497488 C90D99D8".Replace(" ", ""));
        var gen = Belt.Compress(X);
        Assert.Equal(gen.S, S);
        Assert.Equal(gen.Y, Y);
    }

    [Fact]
    public void Hash_ReturnsValid_ForStandart()
    {
        var X1 = Convert.FromHexString("B194BAC8 0A08F53B 366D008E 58".Replace(" ", ""));
        var Y1 = Convert.FromHexString("ABEF9725 D4C5A835 97A367D1 4494CC25 42F20F65 9DDFECC9 61A3EC55 0CBA8C75".Replace(" ", ""));
        var X2 = Convert.FromHexString("B194BAC8 0A08F53B 366D008E 584A5DE4 8504FA9D 1BB6C7AC 252E72C2 02FDCE0D".Replace(" ", ""));
        var Y2 = Convert.FromHexString("749E4C36 53AECE5E 48DB4761 227742EB 6DBE13F4 A80F7BEF F1A9CF8D 10EE7786".Replace(" ", ""));
        var X3 = Convert.FromHexString("B194BAC8 0A08F53B 366D008E 584A5DE4 8504FA9D 1BB6C7AC 252E72C2 02FDCE0D 5BE3D612 17B96181 FE6786AD 716B890B".Replace(" ", ""));
        var Y3 = Convert.FromHexString("9D02EE44 6FB6A29F E5C982D4 B13AF9D3 E90861BC 4CEF27CF 306BFB0B 174A154A".Replace(" ", ""));

        var Y1_gen = Belt.Hash(X1);
        var Y2_gen = Belt.Hash(X2);
        var Y3_gen = Belt.Hash(X3);

        Assert.Equal(Y1, Y1_gen);
        Assert.Equal(Y2, Y2_gen);
        Assert.Equal(Y3, Y3_gen);
    }
}
