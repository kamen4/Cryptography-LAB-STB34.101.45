using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Core;

namespace ConsoleApp;

internal class Program
{
    static void Main()
    {
        var rnd = new Random((int)DateTime.Now.ToFileTime());
        for (int i = 0; i < 1; i++)
        {
            var curve = EllipticCurve.GetStandardCurve();
            var (d, Q) = KeyGenerator.GenerateKey(curve);
            string msg = String.Concat(Enumerable.Range(1, (int)(rnd.NextDouble() * 50)).Select(x => (char)rnd.Next((int)'a', (int)'z' + 1)));
            var k = KeyGenerator.GenerateOneTimeKey(curve.Q, d, SHA256.HashData(msg.Select(c => (byte)c).ToArray()));
            Console.WriteLine(@$"
q = {curve.Q}
d = {d}
M = {msg}
==
k = {k}
");
        }
    }
}
