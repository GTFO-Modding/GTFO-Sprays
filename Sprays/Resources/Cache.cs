using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Sprays.Resources
{
    internal static class Cache
    {
        public static readonly string CacheDirectory = Path.Combine(Paths.BepInExRootPath, "cache", "SprayTextures");

        private static string GetCacheFile(string checksum)
        {
            if (!Directory.Exists(CacheDirectory)) Directory.CreateDirectory(CacheDirectory);
            return Path.Combine(CacheDirectory, checksum);
        }

        public static Spray LoadSprayByChecksum(string checksum, bool loadIntoLookup = true)
        {
            string cacheFile = GetCacheFile(checksum);

            if (!File.Exists(cacheFile)) return null;

            byte[] fileBytes = File.ReadAllBytes(cacheFile);
            byte[] fileChecksum = ChecksumBytes(fileBytes, 0, fileBytes.Length);

            // Ensure the checksum matches, in case the cache has been tampered with
            if (Utilities.StringUtils.FromByteArrayAsHex(fileChecksum) != checksum)
                return null;

            Spray spray = Spray.FromBytes(fileBytes);
            if(loadIntoLookup)
                RuntimeLookup.Sprays.Add(spray);
            return spray;
        }
        public static void CacheSpray(Spray spray)
        {
            string cacheFile = GetCacheFile(spray.Checksum);
            // Check if a file is already cached
            if (File.Exists(cacheFile))
            {
                byte[] existingBytes = File.ReadAllBytes(cacheFile);
                // Recalculate it's checksum
                byte[] existingChecksum = ChecksumBytes(existingBytes, 0, existingBytes.Length);

                // Ensure the checksum matches, and skip writing if it does
                if (Utilities.StringUtils.FromByteArrayAsHex(existingChecksum) == spray.Checksum)
                    return;
            }

            File.WriteAllBytes(cacheFile, spray.TextureData);
        }
        public static byte[] ChecksumBytes(byte[] bytes, int offset, int length)
        {
            using SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes, offset, length);
        }
    }
}
