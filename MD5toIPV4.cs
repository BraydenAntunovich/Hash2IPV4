using System.Diagnostics;
using System.Runtime.InteropServices;

public class MD5toIPV4
{
    private readonly string hashcatDir;
    private readonly string hashcatExecutablePath;
    private readonly string potfilePath;
    private readonly string maskPatternsPath;
    private Process? hashcatProcess = null;

    public MD5toIPV4(string hashcatDir)
    {
        this.hashcatDir = hashcatDir;
        this.hashcatExecutablePath = Path.Combine(hashcatDir, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "hashcat.exe" : "hashcat");
        SetExecutablePermissions(this.hashcatExecutablePath);
        this.potfilePath = Path.Combine(hashcatDir, "hashcat.potfile");
        this.maskPatternsPath = Path.Combine(Directory.GetCurrentDirectory(), "ipv4.hcmask");
        GenerateMaskPatternsFile();
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => KillHashcatProcess();
        Console.CancelKeyPress += (sender, e) => KillHashcatProcess();
    }

    private void KillHashcatProcess()
    {
        if (hashcatProcess != null && !hashcatProcess.HasExited)
        {
            hashcatProcess.Kill();
            hashcatProcess = null;
        }
    }

    private void GenerateMaskPatternsFile()
    {
        var patterns = new List<string>();

        string[] octetPatterns = {
            "1?d?d", "2?1?d", "25?2", "?3?d", "?d"
            // Valid IPv4 octet: 0-255 (and you don't want leading zeroes as they are not valid octets,
            // which is why we require using this large set of masks for hashcat - 625 to be exact [5^4 = 625])
            // "1?d?d" = 100-199
            // "2?1?d" = 200-249
            // "25?2" = 250-255
            // "?3?d" = 10-99
            // "?d" = 0-9
            // ?1 = 01234
            // ?2 = 012345
            // ?3 = 123456789
        };

        foreach (var firstMask in octetPatterns)
        {
            foreach (var secondMask in octetPatterns)
            {
                foreach (var thirdMask in octetPatterns)
                {
                    foreach (var fourthMask in octetPatterns)
                    {
                        // As above: ?1 = 01234, ?2 = 012345, ?3 = 123456789
                        string pattern = $"01234,012345,123456789,{firstMask}.{secondMask}.{thirdMask}.{fourthMask}";
                        patterns.Add(pattern);
                    }
                }
            }
        }

        File.WriteAllLines(this.maskPatternsPath, patterns);
    }

    private void SetExecutablePermissions(string filePath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = startInfo };

            process.Start();
            process.WaitForExit(1337); // In case chmod takes longer than expected
        }
    }

    public async Task<string> CrackHashAsync(string hash, bool displayHashcatConsole = false)
    {
        string? result = null;

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = this.hashcatExecutablePath,
            Arguments = $"-m 0 -O -a 3 {hash} -w 3 \"{this.maskPatternsPath}\" --potfile-path \"{this.potfilePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = this.hashcatDir
        };

        hashcatProcess = new Process { StartInfo = startInfo };
        hashcatProcess.Start();

        hashcatProcess.BeginOutputReadLine();
        hashcatProcess.OutputDataReceived += (sender, args) =>
        {
            if (displayHashcatConsole) Console.WriteLine(args.Data);
            if (args.Data != null)
            {
                if (args.Data.Contains("Status...........: Cracked") ||
                    args.Data.Contains("Recovered........: 1/1 (100.00%)") ||
                    (args.Data.Contains("Guess.Queue") && args.Data.Contains("100.00%")))
                {
                    if (!hashcatProcess.HasExited) hashcatProcess.Kill();
                }
            }
        };

        hashcatProcess.BeginErrorReadLine();

        if (displayHashcatConsole) hashcatProcess.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

        hashcatProcess.WaitForExit();

        // After hashcat finishes, check the potfile for the hash and its corresponding plaintext
        if (File.Exists(this.potfilePath))
        {
            string[] potfileContents = await File.ReadAllLinesAsync(this.potfilePath);
            foreach (string line in potfileContents)
            {
                if (line.Contains(hash))
                {
                    result = line.Split(':')[1];
                    break;
                }
            }
        }

        if (File.Exists(this.maskPatternsPath)) File.Delete(this.maskPatternsPath);

        if (Directory.Exists("kernel")) Directory.Delete("kernel", true);

#pragma warning disable CS8603 // Possible null reference return.
        return result;
#pragma warning restore CS8603 // Possible null reference return.
    }
}