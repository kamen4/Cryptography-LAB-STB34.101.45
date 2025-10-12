using System.Numerics;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core;

public static class EDS
{
    public static byte[] Generate(EllipticCurve curve, byte[] X, BigInteger d, (byte[] OID, byte[] H, BigInteger k)? data = null)
    {
        int l = GetL(curve.P);

        byte[] OID = data is not null ? data.Value.OID : MathHelper.GetOIDBytesSHA2l(l);
        byte[] H = data is not null ? data.Value.H : l switch
        {
            128 => SHA256.HashData(X),
            192 => SHA384.HashData(X),
            256 => SHA512.HashData(X),
            _ => throw new ArgumentException("Invalid Curve", nameof(curve))
        };

        var k = data is not null ? data.Value.k : KeyGenerator.GenerateOneTimeKey(curve.Q, d, H);
        var R = ECPoint.MultiplyScalar(curve.G, k, curve);
        var S_0 = 
            MathHelper.FillByteArray(
                MathHelper.BeltHash(
                    MathHelper.Combine(
                        OID,
                        MathHelper.Combine(
                            MathHelper.FillByteArray(R.X.ToByteArray(true), 2 * l / 8),
                            H
                        )
                    )
                ),
                l/8
            );
        var S_1_num = (k - new BigInteger(H, true) % curve.P - (new BigInteger(S_0, true) % curve.P + (BigInteger.One << l)) * d) % curve.Q;
        if (S_1_num < 0)
        {
            S_1_num += curve.Q;
        }
        var S_1 = MathHelper.FillByteArray(S_1_num.ToByteArray(true), 2 * l / 8);
        return MathHelper.Combine(S_0, S_1);
    }   

    public static bool Check(EllipticCurve curve, byte[] X, byte[] S, ECPoint Q, (byte[] OID, byte[] H)? data = null)
    {
        int l = GetL(curve.P);

        if (S.Length != 3 * l / 8)
        {
            return false;
        }

        var S_bytes = S;
        var S_0_bytes = new byte[l / 8];
        var S_1_bytes = new byte[2 * l / 8];
        Array.Copy(S_bytes, 0, S_0_bytes, 0, l / 8);
        Array.Copy(S_bytes, l / 8, S_1_bytes, 0, 2 * l / 8);

        var S_0 = new BigInteger(S_0_bytes, true);
        var S_1 = new BigInteger(S_1_bytes, true);

        var S_1_mod = S_1 % curve.P;
        if (S_1_mod < 0)
        {
            S_1_mod += curve.P;
        }
        if (S_1_mod >= curve.Q)
        {
            return false;
        }

        byte[] OID = data is not null ? data.Value.OID : MathHelper.GetOIDBytesSHA2l(l);
        byte[] H = data is not null ? data.Value.H : l switch
        {
            128 => SHA256.HashData(X),
            192 => SHA384.HashData(X),
            256 => SHA512.HashData(X),
            _ => throw new ArgumentException("Invalid Curve", nameof(curve))
        };

        var R = ECPoint.Add(
                ECPoint.MultiplyScalar(curve.G, S_1_mod + new BigInteger(H, true) % curve.Q, curve),
                ECPoint.MultiplyScalar(Q, S_0 % curve.P + (BigInteger.One << l), curve),
                curve
            );

        if (R.IsInfinity)
        {
            return false;
        }

        var t = MathHelper.FillByteArray(
                MathHelper.BeltHash(
                    MathHelper.Combine(
                        OID,
                        MathHelper.Combine(
                            MathHelper.FillByteArray(R.X.ToByteArray(true), 2 * l / 8),
                            H
                        )
                    )
                ),
                l / 8
            );

        if (new BigInteger(t, true) != S_0) 
        {
            return false; 
        }

        return true;
    }

    private static int GetL(BigInteger P)
    {
        int l = 1;
        BigInteger pow2l = 4;
        while (P >= pow2l)
        {
            l++;
            pow2l <<= 2;
        }
        return l;
    }
}
