using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend;

public class HashCalculator
{
    // Used to decrypt/encrypt passwords with secret + salt
    public static readonly string ServerSecurityHash = Program.Configuration.GetValue<string>("ServerSecret")!;
    
    public static string ComputeSHA256Hash(string input) {
        // Covert the string to bytes, and then to hashed bytes
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);

        // Convert the hash bytes to a hexadecimal string
        StringBuilder sb = new();
        foreach (byte b in hashBytes) {
            sb.Append(b.ToString("x2")); // "x2" formats each byte as a two-character hexadecimal
        }
        return sb.ToString();
    }

    public static bool IsSHA256(string input) {
        // Regular expression pattern for SHA256 hash (64 hexadecimal characters)
        string pattern = @"^[a-fA-F0-9]{64}$";

        // Check if the input string matches the pattern
        return Regex.IsMatch(input, pattern);
    }
}
