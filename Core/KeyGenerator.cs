using System.Numerics;
using System.Security.Cryptography;

namespace Core;

public class KeyGenerator
{
    /// <summary>
    /// Алгоритм генерации пары ключей
    /// </summary>
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

    /// <summary>
    /// Алгоритм генерации одноразового ключа
    /// </summary>
    public static BigInteger GenerateOneTimeKey(BigInteger q,
        BigInteger d,
        byte[] H)
    {
        if (q <= 1)
        {
            throw new ArgumentException("q must be > 1", nameof(q));
        }
        ArgumentNullException.ThrowIfNull(H);
        int l = -1;
        foreach (int cand in new[] { 128, 192, 256 })
        {
            BigInteger bound = BigInteger.One << (2 * cand);
            if (q < bound) { l = cand; break; }
        }
        if (l == -1)
        {
            throw new ArgumentException("q is too large (expecting l in {128,192,256})", nameof(q));
        }

        if (!(BigInteger.One << (2 * l - 1) < q && q < BigInteger.One << (2 * l)))
        {
            throw new ArgumentException("q does not satisfy 2^{2l-1} < q < 2^{2l}", nameof(q));
        }

        int n = l / 64; // число 128-битных блоков r_i
        int bytesH = 2 * l / 8;
        if (H.Length != bytesH)
        {
            throw new ArgumentException($"H length must be {bytesH} bytes for l={l}", nameof(H));
        }

        // 1. Выбрать t произвольным образом
        byte[] t = [];

        // 2. theta = belt-hash(OID(h) || <d>_{2l} || t)
        byte[] d2l = MathHelper.FillByteArray(d.ToByteArray(true), 2 * l / 8);
        byte[] oid = MathHelper.GetOIDBytesSHA2l(l);
        byte[] thetaBytes = MathHelper.Combine(oid, MathHelper.Combine(d2l, t));
        byte[] theta = MathHelper.BeltHash(thetaBytes);

        // 3. r <- H (r = r1 || r2 || ... || rn)
        byte[][] rBlocks = new byte[n][];
        for (int i = 0; i < n; i++)
        {
            rBlocks[i] = new byte[16];
            Buffer.BlockCopy(H, i * 16, rBlocks[i], 0, 16);
        }

        static BigInteger RtoBigInt(byte[][] blocks)
        {
            int total = blocks.Length * 16;
            byte[] all = new byte[total];
            for (int i = 0; i < blocks.Length; i++)
                Buffer.BlockCopy(blocks[i], 0, all, i * 16, 16);
            return new BigInteger(all, isUnsigned: true, isBigEndian: false);
        }

        //4.
        ulong icntr = 0;
        while (true)
        {
            icntr++;
            byte[] s = new byte[16];
            //1)
            if (n == 2)
            {
                Buffer.BlockCopy(rBlocks[0], 0, s, 0, 16);
            }
            //2)
            else if (n == 3)
            {
                for (int b = 0; b < 16; b++)
                {
                    s[b] = (byte)(rBlocks[0][b] ^ rBlocks[1][b]);
                }
                Buffer.BlockCopy(rBlocks[1], 0, rBlocks[0], 0, 16);
            }
            //3)
            else
            {
                for (int b = 0; b < 16; b++)
                {
                    s[b] = (byte)(rBlocks[0][b] ^ rBlocks[1][b] ^ rBlocks[2][b]);
                }
                Buffer.BlockCopy(rBlocks[1], 0, rBlocks[0], 0, 16);
                Buffer.BlockCopy(rBlocks[2], 0, rBlocks[1], 0, 16);
            }

            //4)
            byte[] beltOut = MathHelper.BeltBlock(s, theta);
            byte[] iBlock = new byte[16];
            byte[] tmp = BitConverter.GetBytes(icntr);
            Buffer.BlockCopy(tmp, 0, iBlock, 0, Math.Min(8, tmp.Length));
            byte[] new_rn_1 = new byte[16];
            for (int b = 0; b < 16; b++)
            {
                byte rn = rBlocks[n - 1][b];
                byte bo = beltOut[b];
                byte ib = iBlock[b];
                new_rn_1[b] = (byte)(bo ^ rn ^ ib);
            }
            Buffer.BlockCopy(new_rn_1, 0, rBlocks[n - 2], 0, 16);

            //5)
            Buffer.BlockCopy(s, 0, rBlocks[n - 1], 0, 16);

            //6)
            if ((icntr % (ulong)(2 * n)) == 0)
            {
                BigInteger rValue = RtoBigInt(rBlocks) % q;
                if (rValue >= 1 && rValue < q)
                {
                    //5. 6.
                    Array.Clear(theta, 0, theta.Length);
                    Array.Clear(s, 0, s.Length);
                    return rValue;
                }
            }
        }
    }
}
