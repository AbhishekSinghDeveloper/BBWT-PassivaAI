using BBWM.Core.Exceptions;

using System.Data.HashFunction;
using System.Data.HashFunction.Blake2;

namespace BBWM.Core.ModelHashing;

internal static class HashingHelper
{
    internal static string AppendHashToKey(int key, byte[] salt)
    {
        return $"{key}-{GetHashFromKey(key, salt)}";
    }

    internal static int? GetKeyFromHashString(string keyWithHash, byte[] salt)
    {
        var parts = keyWithHash.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out int id))
        {
            if (GetHashFromKey(id, salt) == parts[1])
            {
                return id;
            }
            throw new ForbiddenException("Invalid value of hashed model's field");
        }

        return null;
    }

    internal static byte[] GetTypeHash(Type type)
    {
        var config = new Blake2BConfig
        {
            HashSizeInBits = 128
        };

        var blake2BFactory = Blake2BFactory.Instance;
        var blake2BHash = blake2BFactory.Create(config);

        return blake2BHash.ComputeHash(type.FullName).Hash;
    }

    private static string GetHashFromKey(int key, byte[] salt)
    {
        var config = new Blake2BConfig
        {
            HashSizeInBits = 64,
            Salt = salt
        };

        var blake2BFactory = Blake2BFactory.Instance;
        var blake2BHash = blake2BFactory.Create(config);

        var hash = blake2BHash.ComputeHash(key);
        return BitConverter.ToString(hash.Hash).Replace("-", "");
    }
}
