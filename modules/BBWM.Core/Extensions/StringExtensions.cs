using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BBWM.Core.Extensions;

public static class StringExtensions
{
    /// <summary>
    ///  Shortens string to Max length
    /// </summary>
    /// <param name="s">String to shortent</param>
    /// <returns>shortened string</returns>
    public static string MaxLength(this string s, int length) =>
        s?[..Math.Min(length, s.Length)];

    public static string ToTitlePhrase(this string phrase)
    {
        var words = phrase.Split(' ');
        var exceptionsOfTitleCase = new string[]
        {
            "a", "an", "the", "and", "but", "or", "off", "nor", "with", "of", "by", "in", "to", "for", "on",
            "at", "upon", "below", "above", "as", "so", "onto"
        };

        for (var i = 0; i < words.Length; i++)
        {
            if (i == 0 || i == words.Length - 1
                || !exceptionsOfTitleCase.Contains(words[i].ToLowerInvariant()))
            {
                words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i]);
            }
            else
            {
                words[i] = words[i].ToLowerInvariant();
            }
        }

        return string.Join(" ", words);
    }

    private static readonly Random random = new();

    public static string RandomAlphaNumberic(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Encrypts a string using AES (Advanced encryption standard) to base64 string 
    /// </summary>
    /// <param name="text">Text to encrypt</param>
    /// <param name="keyString">Encrypting key</param>
    /// <returns></returns>
    public static string AesEncryptBase64(this string text, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        // ! Using simple "using' statement breaks the code. Therefore "using" code block here is applied.
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(text);
        }

        array = memoryStream.ToArray();

        return Convert.ToBase64String(array);
    }

    /// <summary>
    /// Decrypts a base64 string using AES (Advanced encryption standard) to string
    /// </summary>
    /// <param name="cipherText">Cipher text to decrypt</param>
    /// <param name="keyString">Encrypting key</param>
    /// <returns></returns>
    public static string AesDecryptBase64(this string cipherText, string key)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(buffer);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }
}
