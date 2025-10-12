using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Core;

public static class Pari
{
    public static BigInteger CountPoints(BigInteger p, BigInteger a, BigInteger b)
    {
        string script = $@"
default(parisize, 100000000);
default(parisizemax, 80000000);
E = ellinit([0,0,0,{a:D},{b:D}], {p:D});
s = ellcard(E);
print(s);
quit;
";

        var psi = new ProcessStartInfo
        {
            FileName = "gp.exe",
            Arguments = "-q",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var proc = Process.Start(psi) ?? throw new Exception("Could not start gp.exe");
        proc.StandardInput.WriteLine(script);
        proc.StandardInput.Close();

        string stdoutTask = proc.StandardOutput.ReadToEnd();
        string stderrTask = proc.StandardError.ReadToEnd();

        proc.WaitForExit();

        if (proc.ExitCode != 0)
            throw new Exception($"gp.exe failed (exit {proc.ExitCode}). stderr:\n{stderrTask}");

        var firstLine = stdoutTask.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        if (BigInteger.TryParse(firstLine, out var result))
            return result;
        else
            throw new Exception($"Could not parse gp output. stdout:\n{stdoutTask}\nstderr:\n{stderrTask}");
    }
}
