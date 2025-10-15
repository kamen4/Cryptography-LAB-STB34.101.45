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

    public static ECPoint Subtract(ECPoint p, ECPoint q, EllipticCurve curve)
    {
        var negQ = new ECPoint(q.X, curve.P - q.Y);
        return Add(p, negQ, curve);
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

    /// <summary>
    /// Неформальная двоичная форма записи (NAF)
    /// </summary>
    public static sbyte[] NAF(BigInteger d)
    {
        List<sbyte> di = [];
        int i = 0;
        while (d >= 1)
        {
            if (!d.IsEven)
            {
                di.Add((sbyte)(2 - (d & 3)));
                d -= di[i];
            }
            else
            {
                di.Add(0);
            }
            d >>= 1;
            i++;
        }
        return [.. di];
    }


    /// <summary>
    /// Вычисляет dP при помощи NAF
    /// </summary>
    public static ECPoint MultiplyScalarNAF(ECPoint P, BigInteger d, EllipticCurve curve)
    {
        var naf = NAF(d);
        var U = Infinity;
        var l = naf.Length;
        for (int i = l - 1; i >= 0; i--)
        {
            U = Double(U, curve);
            if (naf[i] == 1)
            {
                U = Add(U, P, curve);
            }
            else if (naf[i] == -1)
            {
                U = Subtract(U, P, curve);
            }
        }
        return U;
    }

    /// <summary>
    /// Кратная точка при помощи адитивных цепочек
    /// </summary>
    public static ECPoint MultiplyScalarAdditiveChain(ECPoint P, BigInteger d, EllipticCurve curve, List<(int, int)>? chain = null)
    {
        chain ??= BuildAdditiveChainBerlekamp(d);
        List<ECPoint> points = [P];
        for (int i = 1; i < chain.Count; i++)
        {
            var (j, k) = chain[i];
            points.Add(Add(points[j], points[k], curve));
        }
        return points[^1];
    }

    /// <summary>
    /// Алгоритм Берликампа для построения аддитивной цепочки
    /// Возвращает список пар (j, k), где a[i] = a[j] + a[k]
    /// </summary>
    public static List<(int j, int k)> BuildAdditiveChainBerlekamp(BigInteger d)
    {
        if (d < 1)
        {
            throw new ArgumentException("d must be positive", nameof(d));
        }

        List<BigInteger> chainValues = [BigInteger.One];
        List<(int j, int k)> chain = [(0, 0)];

        if (d == 1)
        {
            return chain;
        }

        while (chainValues[^1] < d)
        {
            BigInteger bestNext = 0;
            (int j, int k) bestPair = (0, 0);

            int n = chainValues.Count;

            for (int j = n - 1; j >= 0; j--)
            {
                for (int k = j; k >= 0; k--)
                {
                    BigInteger candidate = chainValues[j] + chainValues[k];
                    if (candidate > chainValues[^1] && 
                        candidate <= d &&
                        candidate > bestNext)
                    {
                        bestNext = candidate;
                        bestPair = (j, k);
                    }
                }
            }

            if (bestNext == 0)
            {
                bestNext = chainValues[^1] * 2;
                bestPair = (chainValues.Count - 1, chainValues.Count - 1);
            }

            chainValues.Add(bestNext);
            chain.Add(bestPair);
        }

        return chain;
    }

    /// <summary>
    /// Кратная точка: Оконный метод
    /// </summary>
    public static ECPoint MultiplyScalarWindow(ECPoint P, BigInteger d, EllipticCurve curve, int w = 4)
    {
        if (P.IsInfinity || d == 0)
        {
            return Infinity;
        }
        if (w < 1)
        {
            w = 4;
        }

        int tableSize = 1 << w;

        var T = new ECPoint[tableSize];
        T[0] = Infinity;
        T[1] = P;
        for (int i = 2; i < tableSize; i++)
        {
            T[i] = Add(T[i - 1], P, curve);
        }

        int bitlen = (int)d.GetBitLength();
        int windows = (bitlen + w - 1) / w;

        ECPoint R = Infinity;
        BigInteger mask = (BigInteger.One << w) - 1;

        for (int k = windows - 1; k >= 0; k--)
        {
            for (int t = 0; t < w; t++)
            {
                R = Double(R, curve);
            }
            int shift = k * w;
            int digit = (int)((d >> shift) & mask);
            if (digit != 0)
            {
                R = Add(R, T[digit], curve);
            }
        }

        return R;
    }

    /// <summary>
    /// Кратная точка: Метод скользящего окна
    /// </summary>
    public static ECPoint MultiplyScalarSlidingWindow(ECPoint P, BigInteger d, EllipticCurve curve, int w = 4)
    {
        int m = 1 << (w - 1);
        var precomp = new ECPoint[m];
        precomp[0] = P;
        ECPoint twoP = Double(P, curve);
        for (int i = 1; i < m; i++)
        {
            precomp[i] = Add(precomp[i - 1], twoP, curve);
        }

        ECPoint R = Infinity;
        int iBit = (int)d.GetBitLength() - 1;
        while (iBit >= 0)
        {
            
            if ((d & (BigInteger.One << iBit)) == 0)
            {
                R = Double(R, curve);
                iBit--;
            }
            else
            {
                int j = Math.Max(iBit - w + 1, 0);
                while ((d & (BigInteger.One << j)) == 0)
                {
                    j++;
                }
                int winVal = (int)((d >> j) & ((1 << (iBit - j + 1)) - 1));
                for (int k = 0; k < iBit - j + 1; k++)
                {
                    R = Double(R, curve);
                }
                R = Add(R, precomp[(winVal - 1) / 2], curve);
                iBit = j - 1;
            }
        }
        return R;
    }

    /// <summary>
    /// Якобиановы координаты вычисление dP
    /// </summary>
    public static ECPoint MultiplyScalarJacobian(ECPoint P, BigInteger d, EllipticCurve curve)
    {
        if (P.IsInfinity || d.IsZero)
        {
            return Infinity;
        }
        if (d < 0)
        {
            throw new ArgumentException("Scalar must be non-negative", nameof(d));
        }

        BigInteger pmod = curve.P;

        BigInteger Mod(BigInteger x)
        {
            x %= pmod;
            if (x < 0) x += pmod;
            return x;
        }

        // (X : Y : Z) -> (X/Z^2, Y/Z^3)
        (BigInteger X, BigInteger Y, BigInteger Z, bool Inf) ToJac(ECPoint A)
        {
            if (A.IsInfinity) return (BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, true);
            return (Mod(A.X), Mod(A.Y), BigInteger.One, false); // Z=1
        }

        ECPoint FromJac((BigInteger X, BigInteger Y, BigInteger Z, bool Inf) J)
        {
            if (J.Inf || J.Z.IsZero) return Infinity;
            BigInteger Zinv = ModInverse(J.Z, pmod);
            BigInteger Z2 = Mod(Zinv * Zinv);
            BigInteger Z3 = Mod(Z2 * Zinv);
            BigInteger x = Mod(J.X * Z2);
            BigInteger y = Mod(J.Y * Z3);
            return new ECPoint(x, y);
        }

        (BigInteger X, BigInteger Y, BigInteger Z, bool Inf) JacDouble((BigInteger X, BigInteger Y, BigInteger Z, bool Inf) Pj)
        {
            if (Pj.Inf || Pj.Y.IsZero) return (BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, true);

            BigInteger X1 = Mod(Pj.X);
            BigInteger Y1 = Mod(Pj.Y);
            BigInteger Z1 = Mod(Pj.Z);

            // A = X1^2
            // B = Y1^2
            // C = B^2
            // D = 2 * ((X1 + B)^2 - A - C)
            // E = 3*A + a*Z1^4
            // F = E^2
            // X3 = F - 2*D
            // Y3 = E*(D - X3) - 8*C
            // Z3 = 2*Y1*Z1

            BigInteger A = Mod(BigInteger.ModPow(X1, 2, pmod));
            BigInteger B = Mod(BigInteger.ModPow(Y1, 2, pmod));
            BigInteger C = Mod(BigInteger.ModPow(B, 2, pmod));

            BigInteger X1plusB = Mod(X1 + B);
            BigInteger D = Mod(2 * (Mod(BigInteger.ModPow(X1plusB, 2, pmod)) - A - C));
            BigInteger Z1_4 = Mod(BigInteger.ModPow(Z1, 4, pmod));
            BigInteger E = Mod(3 * A + Mod(curve.A * Z1_4));
            BigInteger F = Mod(BigInteger.ModPow(E, 2, pmod));

            BigInteger X3 = Mod(F - 2 * D);
            BigInteger Y3 = Mod(E * (D - X3) - 8 * C);
            BigInteger Z3 = Mod(2 * Y1 * Z1);

            return (X3, Y3, Z3, false);
        }

        (BigInteger X, BigInteger Y, BigInteger Z, bool Inf) JacAddAffine((BigInteger X, BigInteger Y, BigInteger Z, bool Inf) Pj, ECPoint Qaff)
        {
            if (Pj.Inf) return ToJac(Qaff);
            if (Qaff.IsInfinity) return Pj;
            BigInteger X1 = Mod(Pj.X);
            BigInteger Y1 = Mod(Pj.Y);
            BigInteger Z1 = Mod(Pj.Z);
            BigInteger X2 = Mod(Qaff.X);
            BigInteger Y2 = Mod(Qaff.Y);
            BigInteger Z1sqr = Mod(BigInteger.ModPow(Z1, 2, pmod));
            BigInteger U2 = Mod(X2 * Z1sqr);
            BigInteger Z1cub = Mod(Z1sqr * Z1);
            BigInteger S2 = Mod(Y2 * Z1cub);
            BigInteger U1 = X1;
            BigInteger S1 = Y1;
            BigInteger H = Mod(U2 - U1);
            BigInteger R = Mod(S2 - S1);
            
            if (H.IsZero)
            {
                if (R.IsZero)
                {
                    return JacDouble(Pj);
                }
                return (BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, true);
            }
            
            BigInteger H2 = Mod(H * H);
            BigInteger H3 = Mod(H2 * H);
            BigInteger U1H2 = Mod(U1 * H2);
            BigInteger X3 = Mod(Mod(BigInteger.ModPow(R, 2, pmod)) - H3 - 2 * U1H2);
            BigInteger Y3 = Mod(R * (U1H2 - X3) - S1 * H3);
            BigInteger Z3 = Mod(Z1 * H);

            return (X3, Y3, Z3, false);
        }

        var Rj = (BigInteger.Zero, BigInteger.Zero, BigInteger.Zero, true);

        int bitlen = (int)d.GetBitLength();

        for (int i = bitlen - 1; i >= 0; i--)
        {
            Rj = JacDouble(Rj);
            if (((d >> i) & 1) != 0)
            {
                Rj = JacAddAffine(Rj, P);
            }
        }

        return FromJac(Rj);
    }

}
