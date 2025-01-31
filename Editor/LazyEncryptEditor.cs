#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using LazyEncrypt.Core;

namespace LazyEncrypt.Editor
{
    /// <summary>
    /// Unity Editor window for managing secret key, obfuscation, and hashing.
    /// Allows developers to create, encrypt, and test secrets.
    /// </summary>
    public class LazyEncryptEditor : EditorWindow
    {
        private string obfuscationKey = "DefaultKey"; // Key used for XOR encryption
        private string secretInput = ""; // Raw secret before encryption
        private string obfuscatedSecret = ""; // XOR-obfuscated secret
        private string decryptedSecret = ""; // Decrypted secret for testing
        private string testHashInput = ""; // Input string for SHA-256 hashing
        private string testHashOutput = ""; // Output hash for verification

        // Paths for storing encrypted key and secret inside StreamingAssets
        private const string KeyPath = "Assets/StreamingAssets/lazy_enc_key.dat";
        private const string SecretPath = "Assets/StreamingAssets/lazy_enc.dat";

        /// <summary>
        /// Opens the LazyEncrypt Manager window in Unity.
        /// </summary>
        [MenuItem("Tools/LazyEncrypt/Encryption Manager")]
        public static void ShowWindow()
        {
            GetWindow<LazyEncryptEditor>("LazyEncrypt Manager").minSize = new Vector2(400, 400);
        }

        private Vector2 scrollPosition;

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🔐 LazyEncrypt", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));

            DrawObfuscationKeySection();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawSecretManagementSection();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawDecryptionSection();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawUpdateSection();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            DrawHashingTestSection();
            GUILayout.Space(40);

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// UI for managing the obfuscation key.
        /// </summary>
        private void DrawObfuscationKeySection()
        {
            GUILayout.Label("🛠 Obfuscation Key", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This key is used to XOR-encrypt your secret. Store it securely.", MessageType.Info);
            obfuscationKey = EditorGUILayout.TextField(new GUIContent("🔑 Obfuscation Key", "Enter a custom obfuscation key."), obfuscationKey);

            if (GUILayout.Button(new GUIContent("Save Key", "Save the obfuscation key to StreamingAssets.")))
            {
                SaveObfuscationKey();
            }

            GUILayout.Space(10);
        }

        /// <summary>
        /// UI for encrypting and storing secrets.
        /// </summary>
        private void DrawSecretManagementSection()
        {
            GUILayout.Label("🔏 Secret Management", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Enter your secret below, obfuscate it, and store it securely.", MessageType.Info);

            secretInput = EditorGUILayout.TextField(new GUIContent("📦 Secret String", "Enter the original secret before obfuscation."), secretInput);

            if (GUILayout.Button(new GUIContent("Obfuscate Secret", "Apply XOR obfuscation to the secret.")))
            {
                obfuscatedSecret = SecretManager.XorEncrypt(secretInput, obfuscationKey);
            }

            GUILayout.Label("🔄 Obfuscated Secret:", EditorStyles.label);
            EditorGUILayout.SelectableLabel(obfuscatedSecret, EditorStyles.textField, GUILayout.Height(20));

            if (GUILayout.Button(new GUIContent("Save Secret", "Save the obfuscated secret to StreamingAssets.")))
            {
                SaveSecret(obfuscatedSecret);
            }

            GUILayout.Space(10);
        }

        /// <summary>
        /// UI for decrypting the stored secret.
        /// </summary>
        private void DrawDecryptionSection()
        {
            GUILayout.Label("🔓 Decryption Test", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Load and decrypt the stored secret to verify its integrity.", MessageType.Info);

            if (GUILayout.Button(new GUIContent("Load & Decrypt Stored Secret", "Retrieve and decrypt the stored secret.")))
            {
                decryptedSecret = LoadAndDecryptSecret();
            }

            GUILayout.Label("📜 Decrypted Secret:", EditorStyles.label);
            EditorGUILayout.SelectableLabel(decryptedSecret, EditorStyles.textField, GUILayout.Height(20));
        }
        
        /// <summary>
        /// UI for updating the secret and key from StreamingAssets to the latest version.
        /// </summary>
        private void DrawUpdateSection()
        {
            GUILayout.Label("🔄 Update Secret & Key", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click this button if you've changed the secret or key and want the game to update it.", MessageType.Info);

            if (GUILayout.Button("Force Update Secret & Key"))
            {
                SecretManager.UpdateSecretFiles();
                Debug.Log("✅ Secret & Key updated from StreamingAssets.");
            }
        }

        /// <summary>
        /// UI for testing SHA-256 hashing with the stored secret.
        /// </summary>
        private void DrawHashingTestSection()
        {
            GUILayout.Label("🧪 SHA-256 Hashing Test", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Enter a string below to compute its SHA-256 hash using the stored secret.", MessageType.Info);

            testHashInput = EditorGUILayout.TextField(new GUIContent("🔡 Test Input", "Enter a string to hash."), testHashInput);

            if (GUILayout.Button(new GUIContent("Compute Hash", "Generate a SHA-256 hash using the secret.")))
            {
                string secret = SecretManager.GetSecret();
                testHashOutput = string.IsNullOrEmpty(secret) ? "Error: No Secret Found!" : SHA256Hasher.Hash(testHashInput, secret);
            }

            GUILayout.Label("📝 SHA-256 Hash:", EditorStyles.label);
            EditorGUILayout.SelectableLabel(testHashOutput, EditorStyles.textField, GUILayout.Height(40));
        }

        private void SaveObfuscationKey()
        {
            EnsureDirectoryExists(KeyPath);
            File.WriteAllText(KeyPath, obfuscationKey);
            AssetDatabase.Refresh();
            Debug.Log($"✅ Obfuscation Key saved successfully to {KeyPath}");
        }

        private void SaveSecret(string obfuscatedText)
        {
            EnsureDirectoryExists(SecretPath);
            File.WriteAllText(SecretPath, obfuscatedText);
            AssetDatabase.Refresh();
            Debug.Log($"✅ Secret saved successfully to {SecretPath}");
            SecretManager.ClearSecretCache();
        }

        private string LoadAndDecryptSecret()
        {
            if (!File.Exists(SecretPath) || !File.Exists(KeyPath))
            {
                Debug.LogError("❌ Secret file or key file not found!");
                return "Error: File missing!";
            }

            var storedKey = File.ReadAllText(KeyPath).Trim();
            var storedSecret = File.ReadAllText(SecretPath).Trim();
            return SecretManager.XorDecrypt(storedSecret, storedKey);
        }

        private void EnsureDirectoryExists(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directoryPath))
            {
                Debug.LogError("❌ Invalid directory path detected!");
                return;
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
    }
}
#endif
