using System.Numerics;

namespace Core;

public class ECPoint
{
    public BigInteger X { get; }
    public BigInteger Y { get; }
    public bool IsInfinity { get; }

    public ECPoint(BigInteger x, BigInteger y)
    {
        X = x; Y = y; IsInfinity = false;
    }
    private ECPoint() { IsInfinity = true; }

    public static ECPoint Infinity => new();

    public override string ToString() => IsInfinity ? "Inf" : $"({X}, {Y})";
    public override bool Equals(object? obj)
    {
        if (obj is ECPoint other)
        {
            if (IsInfinity && other.IsInfinity) return true;
            return X == other.X && Y == other.Y && IsInfinity == other.IsInfinity;
        }
        return false;
    }
    public override int GetHashCode() => (X, Y, IsInfinity).GetHashCode();

    /// <summary>
    /// Вычисляет dP
    /// Бинарный метод справа налево
    /// </summary>
    public static ECPoint MultiplyScalar(ECPoint P, BigInteger d, EllipticCurve curve)
    {
        ECPoint U = ECPoint.Infinity;
        ECPoint V = P;
        while (d > 0)
        {
            if ((d & 1) == 1)
            {
                U = Add(U, V, curve);
            }
            V = Double(V, curve);
            d >>= 1;
        }
        return U;
    }

    /// <summary>
    /// Сложение двух точек
    /// </summary>
    public static ECPoint Add(ECPoint p, ECPoint q, EllipticCurve curve)
    {
        if (p.IsInfinity)
        {
            return q;
        }
        if (q.IsInfinity)
        {
            return p;
        }
        if (p.X == q.X && (p.Y != q.Y || p.Y == 0))
        {
            return ECPoint.Infinity;
        }

        BigInteger lambda;
        if (p.X != q.X)
        {
            lambda = ((q.Y - p.Y) * ModInverse(q.X - p.X, curve.P)) % curve.P;
        }
        else
        {
            lambda = ((3 * BigInteger.ModPow(p.X, 2, curve.P) + curve.A) * ModInverse(2 * p.Y, curve.P)) % curve.P;
        }

        BigInteger xR = (BigInteger.ModPow(lambda, 2, curve.P) - p.X - q.X) % curve.P;
        BigInteger yR = (lambda * (p.X - xR) - p.Y) % curve.P;
        if (xR < 0) xR += curve.P;
        if (yR < 0) yR += curve.P;
        return new ECPoint(xR, yR);
    }

    /// <summary>
    /// Удвоение точки
    /// </summary>
    public static ECPoint Double(ECPoint p, EllipticCurve curve) => Add(p, p, curve);

    /// <summary>
    /// Обратное по модулю (т Ферма)
    /// </summary>
    public static BigInteger ModInverse(BigInteger a, BigInteger p)
    {
        a %= p;
        if (a < 0) a += p;
        return BigInteger.ModPow(a, p - 2, p);
    }

    /// <summary>
    /// Трюк шамира для dP+eQ
    /// </summary>
    public static ECPoint ShamirTrick(BigInteger d, ECPoint P, BigInteger e, ECPoint Q, EllipticCurve curve)
    {
        var R = Add(P, Q, curve);
        var U = Infinity;

        int l = (int)Math.Max(d.GetBitLength(), e.GetBitLength());
        for (int i = l - 1; i >= 0; i--)
        {
            U = Double(U, curve);
            bool di = (d & (BigInteger.One << i)) != 0;
            bool ei = (e & (BigInteger.One << i)) != 0;
            if (di && ei)
            {
                U = Add(U, R, curve);
            }
            else if (di)
            {
                U = Add(U, P, curve);
            }
            else if (ei)
            {
                U = Add(U, Q, curve);
            }
        }
        return U;
    }
}
