using System;
using System.Security.Cryptography;
using System.Text;

public class HashCalculator
{
    public static readonly string ServerSecurityHash = Program.Configuration.GetValue<string>("ServerSecret");
    public static string ComputeSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // "x2" formats each byte as a two-character hexadecimal
            }
            return sb.ToString();
        }
    }
}
