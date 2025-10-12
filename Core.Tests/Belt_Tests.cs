namespace Core.Tests;

public class Belt_Tests
{
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
}
