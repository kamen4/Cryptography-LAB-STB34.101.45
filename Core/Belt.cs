using System.ComponentModel.DataAnnotations;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core;

public static class Belt
{
    private const string H_string = @"
B1 94 BA C8 0A 08 F5 3B 36 6D 00 8E 58 4A 5D E4
85 04 FA 9D 1B B6 C7 AC 25 2E 72 C2 02 FD CE 0D
5B E3 D6 12 17 B9 61 81 FE 67 86 AD 71 6B 89 0B
5C B0 C0 FF 33 C3 56 B8 35 C4 05 AE D8 E0 7F 99
E1 2B DC 1A E2 82 57 EC 70 3F CC F0 95 EE 8D F1
C1 AB 76 38 9F E6 78 CA F7 C6 F8 60 D5 BB 9C 4F
F3 3C 65 7B 63 7C 30 6A DD 4E A7 79 9E B2 3D 31
3E 98 B5 6E 27 D3 BC CF 59 1E 18 1F 4C 5A B7 93
E9 DE E7 2C 8F 0C 0F A6 2D DB 49 F4 6F 73 96 47
06 07 53 16 ED 24 7A 37 39 CB A3 83 03 A9 8B F6
92 BD 9B 1C E5 D1 41 01 54 45 FB C9 5E 4D 0E F2
68 20 80 AA 22 7D 64 2F 26 87 F9 34 90 40 55 11
BE 32 97 13 43 FC 9A 48 A0 2A 88 5F 19 4B 09 A1
7E CD A4 D0 15 44 AF 8C A5 84 50 BF 66 D2 E8 8A
A2 D7 46 52 42 A8 DF B3 69 74 C5 51 EB 23 29 21
D4 EF D9 B4 3A 62 28 75 91 14 10 EA 77 6C DA 1D";

    public static readonly byte[] H;

    static Belt()
    {
        H = H_string
            .Replace('\n', ' ')
            .Replace('\r', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Convert.ToByte(x, 16))
            .ToArray();
    }

    public static byte Subst(byte u)
    {
        int i = u >> 4;
        int j = u & 0x0F;
        return H[i * 16 + j];
    }

    public static byte[][] Split(byte[] w, int blockBytes)
    {
        int n = (w.Length + blockBytes - 1) / blockBytes;
        byte[][] outp = new byte[n][];
        for (int i = 0; i < n; i++)
        {
            outp[i] = new byte[blockBytes];
            Array.Copy(w, i * blockBytes, outp[i], 0, Math.Min(blockBytes, w.Length - i * blockBytes));
        }

        return outp;
    }

    public static byte[] Block(byte[] X, byte[] K)
    {
        if (X.Length != 128 / 8 || K.Length != 256 / 8)
        {
            throw new ArgumentException("X and K must be 128 and 256 bits long");
        }

        var Xs = Split(X, 32 / 8);
        var Ks = Split(K, 32 / 8);

        byte[] k(int i) => Ks[(i - 1) & 7];

        byte[] RotHi(byte[] src, int bitCount)
        {
            if (src == null || src.Length == 0)
            {
                return [];
            }
            if (src.Length != 4)
            {
                throw new ArgumentException("RotHi requires 4-byte arrays for Belt algorithm");
            }

            uint value = (uint)(src[0] | (src[1] << 8) | (src[2] << 16) | (src[3] << 24));

            int shift = bitCount % 32;
            uint result = (value << shift) | (value >> (32 - shift));

            return [
                (byte)(result & 0xFF),
                (byte)((result >> 8) & 0xFF),
                (byte)((result >> 16) & 0xFF),
                (byte)((result >> 24) & 0xFF)
            ];
        }

        byte[] G(byte[] u, int r)
        {
            int n = u.Length;
            var a = new byte[n];
            for (int i = 0; i < n; i++)
            {
                a[i] = Subst(u[i]);
            }
            return RotHi(a, r);
        }

        byte[] xor(byte[] a, byte[] b)
        {
            var outp = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                outp[i] = (byte)(a[i] ^ b[i]);
            }
            return outp;
        }

        byte[] plus(byte[] a, byte[] b)
        {
            var A = new BigInteger(a, true);
            var B = new BigInteger(b, true);
            var n2 = BigInteger.One << 32;
            var sum = (A + B) % n2;
            if (sum < 0) sum += n2;
            return MathHelper.FillByteArray(sum.ToByteArray(true), 4);
        }

        byte[] minus(byte[] a, byte[] b)
        {
            var A = new BigInteger(a, true);
            var B = new BigInteger(b, true);
            var diff = A - B;
            if (diff < 0) diff += BigInteger.One << 32;
            return MathHelper.FillByteArray(diff.ToByteArray(true), 4);
        }

        void swap(ref byte[] a, ref byte[] b)
        {
            (b, a) = (a, b);
        }

        var a = (byte[])Xs[0].Clone();
        var b = (byte[])Xs[1].Clone();
        var c = (byte[])Xs[2].Clone();
        var d = (byte[])Xs[3].Clone();
        byte[] e;

        for (int i = 1; i <= 8; i++)
        {
            b = xor(b, G(plus(a, k(7 * i - 6)), 5));
            c = xor(c, G(plus(d, k(7 * i - 5)), 21));
            a = minus(a, G(plus(b, k(7 * i - 4)), 13));
            e = xor(G(plus(b, plus(c, k(7 * i - 3))), 21), [(byte)i, 0, 0, 0]);
            b = plus(b, e);
            c = minus(c, e);
            d = plus(d, G(plus(c, k(7 * i - 2)), 13));
            b = xor(b, G(plus(a, k(7 * i - 1)), 21));
            c = xor(c, G(plus(d, k(7 * i)), 5));
            swap(ref a, ref b);
            swap(ref c, ref d);
            swap(ref b, ref c);
        }

        var Y = new byte[16];
        Buffer.BlockCopy(b, 0, Y, 0, 4);
        Buffer.BlockCopy(d, 0, Y, 4, 4);
        Buffer.BlockCopy(a, 0, Y, 8, 4);
        Buffer.BlockCopy(c, 0, Y, 12, 4);
        return Y;
    }
}
