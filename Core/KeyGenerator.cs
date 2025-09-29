using System.Numerics;
using System.Security.Cryptography;

namespace Core;

public class KeyGenerator
{
    /// <summary>
    /// Алгоритм генерации пары ключей
    /// </summary>
    #warning LAB_4
    public static (BigInteger d, ECPoint Q) GenerateKey(EllipticCurve curve)
    {
        BigInteger d;
        do
        {
            var bytes = RandomNumberGenerator.GetBytes(2 * 128 / 8);
            d = new BigInteger(bytes, true) % curve.Q;
        } while (d == 0);

        var Q = ECPoint.MultiplyScalar(curve.G, d, curve);
        return (d, Q);
    }

    /// <summary>
    /// Алгоритм проверки открытого ключа
    /// </summary>
    #warning LAB_5
    public static bool CheckKey(ECPoint Q, EllipticCurve curve)
    {
        if (Q.X < 0 || Q.X >= curve.P || Q.Y < 0 || Q.Y >= curve.P)
        {
            return false;
        }
        if (!curve.IsOnCurve(Q))
        {
            return false;
        }

        return true;
    }
}
