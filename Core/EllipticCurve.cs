using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
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
    public const string SEED_DEFAULT_STB_128_BASE_16 = "5E38010000000000";
    public const string Q_DEFAULT_STB_128_BASE_16 = "07663D2699BF5A7EFC4DFB0DD68E5CD9FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
    public const string GY_DEFAULT_STB_128_BASE_16 = "936A510418CF291E52F608C4663991785D83D651A3C9E45C9FD616FB3CFCF76B";

    public BigInteger P { get; }
    public BigInteger A { get; }
    public BigInteger B { get; }
    public BigInteger Seed { get; }
    public BigInteger Q { get; }
    public ECPoint G { get; }

    public EllipticCurve(BigInteger p, BigInteger a, BigInteger b, BigInteger seed, BigInteger q, ECPoint g)
    {
        P = p; A = a; B = b; Seed = seed; Q = q; G = g;
    }

    /// <summary>
    /// Стандартные параметры СТБ 34.101.45, l=128
    /// </summary>
    public static EllipticCurve GetStandardCurve()
    {
        return new EllipticCurve(
            GetFromHexString(P_DEFAULT_STB_128_BASE_16),
            GetFromHexString(A_DEFAULT_STB_128_BASE_16),
            GetFromHexString(B_DEFAULT_STB_128_BASE_16),
            GetFromHexString(SEED_DEFAULT_STB_128_BASE_16),
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
    public static ECPoint ComputeBasePoint(BigInteger p, BigInteger b)
    {
        return new(BigInteger.Zero, BigInteger.ModPow(b, (p + 1) / 4, p));
    }

    public static BigInteger GetFromHexString(string s)
    {
        byte[] bytes = Convert.FromHexString(s);
        return new(bytes, isUnsigned: true, isBigEndian: false);
    }

    /// <summary>
    /// Алгоритм проверки параметров эллиптической кривой
    /// </summary>
    public bool CheckParams()
    {
        //1. Определить l как минимальное натуральное число, для которого p < 2^2l
        int l = 1;
        BigInteger pow2l = 4;
        while (P >= pow2l)
        {
            l++;
            pow2l <<= 2;
        }

        //2. Если нарушается одно из условий, то возвратить НЕТ:
        //   1) 𝑙 ∈ {128, 192, 256};
        if (l != 128 && l != 192 && l != 256)
        {
            return false;
        }
        //   2) 2^2l-1 < p,q < 2^2l;
        if (P <= (pow2l >> 1) || P >= pow2l || Q <= (pow2l >> 1) || Q >= pow2l)
        {
            return false;
        }
        //   3) p, q — простые;
        if (!TestMillerRabin(P, 100) || !TestMillerRabin(Q, 100))
        {
            return false;
        }
        //   4) p = 3 (mod 4);
        if (P % 4 != 3)
        {
            return false;
        }
        //   5) p != q;
        if (P == Q)
        {
            return false;
        }
        //   6) p^m != 1 (mod q) для 𝑚 = 1, 2, . . . , 50;
        for (int m = 1; m <= 50; m++)
        {
            if (BigInteger.ModPow(P, m, Q) == 1)
            {
                return false;
            }
        }

        //3. Установить t <- <p>2l || <a>2l
        var t = Combine(
            FillTrailingZerosLittleEnd(P.ToByteArray(true), 2 * l / 8),
            FillTrailingZerosLittleEnd(A.ToByteArray(true), 2 * l / 8));

        //4. Установить B <- belt-hash(t || seed) || belt-hash(t || (seed + <1>64))
        byte[] B_bytes = Combine(
            BeltHash(Combine(t, FillTrailingZerosLittleEnd(Seed.ToByteArray(true), 8))),
            BeltHash(Combine(t, FillTrailingZerosLittleEnd((Seed + 1).ToByteArray(true), 8)))
        );
        BigInteger B = new BigInteger(B_bytes, true) % P;

        //5. Если нарушается одно из условий, то возвратить НЕТ:
        //  1) 0 < a, b < p;
        if (A <= 0 || A >= P || B <= 0 || B >= P)
        {
            return false;
        }
        //  2) b = B (mod p);
        if (B != this.B)
        {
            return false;
        }
        //  3) 4a^3 + 27b^2 != 0 (mod p);
        if ((4 * BigInteger.ModPow(A, 3, P) + 27 * BigInteger.ModPow(B, 2, P)) % P == 0)
        {
            return false;
        }
        //  4) (b|p) = 1;
        if (LegendreSymbol(B, P) != 1)
        {
            return false;
        }
        //  5) G = (0, b^(p+1)/4 (mod p))
        if (G.X != 0 || G.Y != BigInteger.ModPow(B, (P + 1) / 4, P))
        {
            return false;
        }
        //  6) qG = O
        if (!ECPoint.MultiplyScalar(G, Q, this).IsInfinity)
        {
            return false;
        }

        //6. Возратить ДА
        return true;
    }

    /// <summary>
    /// Тест Миллера — Рабина на простоту
    /// </summary>
    public static bool TestMillerRabin(BigInteger num, int certainty)
    {
        if (num == 2 || num == 3)
            return true;
        if (num < 2 || num % 2 == 0)
            return false;

        BigInteger d = num - 1;
        int s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[num.ToByteArray().LongLength];
        BigInteger a;

        for (int i = 0; i < certainty; i++)
        {
            do
            {
                rng.GetBytes(bytes);
                a = new BigInteger(bytes);
            }
            while (a < 2 || a >= num - 2);

            BigInteger x = BigInteger.ModPow(a, d, num);
            if (x == 1 || x == num - 1)
                continue;

            for (int r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, num);
                if (x == 1)
                    return false;
                if (x == num - 1)
                    break;
            }

            if (x != num - 1)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Конкатенация масивов байт
    /// </summary>
    public static byte[] Combine(byte[] a, byte[] b)
    {
        byte[] result = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, result, 0, a.Length);
        Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
        return result;
    }

    /// <summary>
    /// Обертка для вызова belt-hash
    /// </summary>
    public static byte[] BeltHash(byte[] data)
    {
        byte[] hash = new byte[32];
        TZICrypt.tzi_belt_hash(data, (uint)data.Length, hash);
        return hash;
    }

    /// <summary>
    /// Символ Лежандра 
    /// </summary>
    public static int LegendreSymbol(BigInteger u, BigInteger p)
    {
        if (u == 0)
        {
            return 0;
        }
        BigInteger ls = BigInteger.ModPow(u, (p - 1) / 2, p);
        return ls == p - 1 ? -1 : (int)ls;
    }

    /// <summary>
    /// Добавление нулей справа от массива байт до нужного размера
    /// </summary>
    public static byte[] FillTrailingZerosLittleEnd(byte[] arr, int totalByteLen)
    {
        if (arr.Length < totalByteLen)
        {
            byte[] newArr = new byte[totalByteLen];
            Buffer.BlockCopy(arr, 0, newArr, 0, arr.Length);
            return newArr;
        }
        return arr[..totalByteLen];
    }
}