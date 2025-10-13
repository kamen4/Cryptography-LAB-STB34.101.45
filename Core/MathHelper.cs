using System.Numerics;
using System.Security.Cryptography;

namespace Core;

public static class MathHelper
{
    /// <summary>
    /// Генерация BigInteger из строки байт
    /// </summary>
    public static BigInteger GetFromHexString(string s)
    {
        byte[] bytes = Convert.FromHexString(s.Replace(" ", ""));
        return new(bytes, isUnsigned: true, isBigEndian: false);
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
        //byte[] hash = new byte[32];
        //TZICrypt.tzi_belt_hash(data, (uint)data.Length, hash);
        //return hash;
        return Belt.Hash(data);
    }

    /// <summary>
    /// Обертка для вызова belt-block
    /// </summary>
    public static byte[] BeltBlock(byte[] data, byte[] key)
    {
        //if (key.Length != 32)
        //{
        //    throw new ArgumentException("Key must be 32 bytes length", nameof(key));
        //}
        //uint xsize = (uint)data.Length;
        //byte[] encr = new byte[xsize];
        //TZICrypt.tzi_belt_ecb_encr(data, xsize, key, encr);
        //return encr;
        return Belt.Block(data, key);
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
    public static byte[] FillByteArray(byte[] arr, int totalByteLen)
    {
        if (arr.Length < totalByteLen)
        {
            byte[] newArr = new byte[totalByteLen];
            Buffer.BlockCopy(arr, 0, newArr, 0, arr.Length);
            return newArr;
        }
        return arr[..totalByteLen];
    }

    /// <summary>
    /// Получение OID для SHA-2 в зависимости от l
    /// </summary>
    public static byte[] GetOIDBytesSHA2l(int l)
    {
        return Convert.FromHexString(l switch
        {
            128 => "0609608648016503040201",
            192 => "0609608648016503040202",
            256 => "0609608648016503040203",
            _ => throw new NotImplementedException("expecting l in {128,192,256}")
        });
    }
}
