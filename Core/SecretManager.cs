using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LazyEncrypt.Core
{
    public static class SecretManager
    {
        private static string secretCache;

        // Paths for secret storage
        private static readonly string KeyPathPersistent = Path.Combine(Application.persistentDataPath, "lazy_enc_key.dat");
        private static readonly string SecretPathPersistent = Path.Combine(Application.persistentDataPath, "lazy_enc.dat");
        private static readonly string KeyPathStreaming = Path.Combine(Application.streamingAssetsPath, "lazy_enc_key.dat");
        private static readonly string SecretPathStreaming = Path.Combine(Application.streamingAssetsPath, "lazy_enc.dat");

        /// <summary>
        /// Retrieves and decrypts the stored secret.
        /// </summary>
        public static string GetSecret()
        {
            if (secretCache != null) return secretCache;

            if (!File.Exists(KeyPathPersistent) || !File.Exists(SecretPathPersistent))
            {
                Debug.LogError("❌ Critical Error: Obfuscation key or secret file is missing!");
                return null;
            }

            var obfuscationKey = File.ReadAllText(KeyPathPersistent).Trim();
            var obfuscatedSecret = File.ReadAllText(SecretPathPersistent).Trim();
            secretCache = XorDecrypt(obfuscatedSecret, obfuscationKey);
            return secretCache;
        }

        /// <summary>
        /// Clears the cached secret, forcing a reload next time it's requested.
        /// </summary>
        public static void ClearSecretCache()
        {
            Debug.Log("🔄 Clearing Secret Cache");
            secretCache = null;
        }

        /// <summary>
        /// Manually updates the secret and key in PersistentDataPath from StreamingAssets.
        /// This should be triggered by the developer when they update the key.
        /// </summary>
        public static void UpdateSecretFiles()
        {
            Debug.Log("🔄 Updating Secret & Key from StreamingAssets...");
            EnsureUpdatedFile(KeyPathStreaming, KeyPathPersistent);
            EnsureUpdatedFile(SecretPathStreaming, SecretPathPersistent);
            ClearSecretCache();
        }

        /// <summary>
        /// Ensures a file in PersistentDataPath is up to date with the one in StreamingAssets.
        /// </summary>
        private static void EnsureUpdatedFile(string sourcePath, string destinationPath)
        {
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"⚠️ Warning: {sourcePath} does not exist. Skipping update.");
                return;
            }

            bool needsUpdate = true;

            if (File.Exists(destinationPath))
            {
                string existingContent = File.ReadAllText(destinationPath).Trim();
                string newContent = File.ReadAllText(sourcePath).Trim();
                needsUpdate = !existingContent.Equals(newContent);
            }

            if (needsUpdate)
            {
                Debug.Log($"🔄 Copying {sourcePath} to {destinationPath}");

                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.WebGLPlayer ||
                    Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    using UnityWebRequest www = UnityWebRequest.Get(sourcePath);
                    www.SendWebRequest();

                    while (!www.isDone) { } // Wait for request completion

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(destinationPath, www.downloadHandler.data);
                        Debug.Log($"✅ Updated {destinationPath}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Failed to update {destinationPath}: {www.error}");
                    }
                }
                else
                {
                    File.Copy(sourcePath, destinationPath, true);
                    Debug.Log($"✅ Updated {destinationPath}");
                }
            }
            else
            {
                Debug.Log($"✅ {destinationPath} is already up to date.");
            }
        }

        /// <summary>
        /// XOR-based encryption for basic obfuscation.
        /// </summary>
        public static string XorEncrypt(string text, string key)
        {
            var result = new StringBuilder();
            for (var i = 0; i < text.Length; i++)
            {
                result.Append((char)(text[i] ^ key[i % key.Length]));
            }
            return result.ToString();
        }

        /// <summary>
        /// XOR decryption (same as encryption due to XOR properties).
        /// </summary>
        public static string XorDecrypt(string encryptedText, string key)
        {
            return XorEncrypt(encryptedText, key);
        }
    }
}
