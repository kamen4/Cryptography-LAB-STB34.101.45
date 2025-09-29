using System.Numerics;
using System.Security.Cryptography;

using static System.String;

namespace Core;

/// <summary>
/// Эллиптическая кривая над F_p.
/// y^2 = x^3 + ax + b (mod p)
/// q — порядок базовой точки G
/// </summary>
public class EllipticCurve
{
    public const string P_DEFAULT_STB_128_BASE_16 = "43FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
    public const string A_DEFAULT_STB_128_BASE_16 = "40FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
    public const string B_DEFAULT_STB_128_BASE_16 = "F1039CD66B7D2EB253928B976950F54CBEFBD8E4AB3AC1D2EDA8F315156CCE77";
    public const string Q_DEFAULT_STB_128_BASE_16 = "07663D2699BF5A7EFC4DFB0DD68E5CD9FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
    public const string GY_DEFAULT_STB_128_BASE_16 = "936A510418CF291E52F608C4663991785D83D651A3C9E45C9FD616FB3CFCF76B";

    public BigInteger P { get; }
    public BigInteger A { get; }
    public BigInteger B { get; }
    public BigInteger Q { get; }
    public ECPoint G { get; }

    public EllipticCurve(BigInteger p, BigInteger a, BigInteger b, BigInteger q, ECPoint g)
    {
        P = p; A = a; B = b; Q = q; G = g;
    }

    /// <summary>
    /// Стандартные параметры СТБ 34.101.45, l=128
    /// </summary>
    #warning LAB_1_1
    public static EllipticCurve GetStandardCurve()
    {
        return new EllipticCurve(
            GetFromHexString(P_DEFAULT_STB_128_BASE_16),
            GetFromHexString(A_DEFAULT_STB_128_BASE_16),
            GetFromHexString(B_DEFAULT_STB_128_BASE_16),
            GetFromHexString(Q_DEFAULT_STB_128_BASE_16),
            new ECPoint(
                BigInteger.Zero,
                GetFromHexString(GY_DEFAULT_STB_128_BASE_16)
            )    
        );
    }

    /// <summary>
    /// Принадлежность кривой
    /// </summary>
    public bool IsOnCurve(ECPoint point)
    {
        if (point.IsInfinity)
        {
            return true;
        }
        BigInteger left = BigInteger.ModPow(point.Y, 2, P);
        BigInteger right = (BigInteger.ModPow(point.X, 3, P) + A * point.X + B) % P;
        return left == right;
    }

    /// <summary>
    /// Алгоритм вычисления базовой точки
    /// </summary>
    #warning LAB_2
    public static ECPoint ComputeBasePoint(BigInteger p, BigInteger b)
    {
        return new(BigInteger.Zero, BigInteger.ModPow(b, (p + 1) / 4, p));
    }

    public static BigInteger GetFromHexString(string s)
    {
        byte[] bytes = Convert.FromHexString(s);
        return new(bytes, isUnsigned: true, isBigEndian: false);
    }
}