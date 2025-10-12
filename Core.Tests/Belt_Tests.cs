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
}
