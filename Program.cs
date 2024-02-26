class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: dotnet run <hashcatDir> <MD5Hash>"); // Debug tab in Visual Studio 2022 -> Debug Properties (at the bottom) -> Command line arguments
            return;
        }

        string hashcatDir = args[0];
        string hash = args[1];

        MD5toIPV4 mD5toIPV4 = new MD5toIPV4(hashcatDir); // Pass the hashcat directory to the constructor
        string result = await mD5toIPV4.CrackHashAsync(hash); // Crack the hash asynchronously

        if (result != null)
        {
            Console.WriteLine($"Hash cracked: {result}");
        }
        else
        {
            Console.WriteLine("Hash not found.");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
