# LazyEncrypt

 A simple Unity package to obfuscate and store secrets for all major platforms.

### Why I Made This

I often find myself needing **SHA-256 hashing** in my Unity projects to securely sign requests before sending them to a backend server. The challenge is always the same: **where do I store the secret key securely on the client side?**

While mobile platforms like Android and iOS offer **Keychain** and **Keystore** for secure storage, I wanted a solution that:

- **Works across all Unity platforms** (including standalone and WebGL)
- **Is simple to integrate** without complex dependencies
- **Works out of the box** without requiring platform-specific implementations

To achieve this, **LazyEncrypt** stores the secret by **obfuscating it with XOR encryption** before placing it in `StreamingAssets`. This provides a lightweight way to protect secrets from casual reverse engineering.

### 🚨 **Important Disclaimer**
This is **not** the best or most secure way to store secrets in a Unity project. There are **more robust solutions** out there.

However, **LazyEncrypt is designed to be simple, cross-platform, and easy to use out of the box** enough to counter casual hacking.

If you need high security, **consider some other advanced methods**

---

## What's Next?

This project is just the beginning. Some ideas for future improvements include:

- **Expanding encryption features** to support local data encryption for games that don’t use backend servers but still need secure save files.
- **Enhancing security** by implementing additional encryption techniques beyond XOR.
- **Open collaboration** with the community to refine and improve the tool.

We'll see where it goes.

---

### **Setup Process**
1. **Decide on an obfuscation key.** This is used to XOR-encrypt your secret key.
2. **Decide on a secret key.** This is the value that will be used when generating hashes.
3. **Encrypt and store the secret in `StreamingAssets`.** Use the editor tool to do step 1-3.
4. **Use hashing whenever needed.** The stored secret is decrypted and used automatically.
5. **The obfuscation mechanism can also serve as a lightweight security measure** beyond just enabling hashing.

LazyEncrypt provides a simple editor interface to configure these values and generate obfuscated storage files.

---

## How to Install

Currently, there is only one way to install LazyEncrypt:

1. **Download the `.unitypackage` file** from the [GitHub Releases](https://github.com/Vishal-Choudhary-bulu/LazyEncrypt/releases).
2. **Import it into your Unity project** via `Assets → Import Package → Custom Package...`.
3. **You're ready to use it.** The editor tool is available under `Tools → LazyEncrypt → Encryption Manager`.

For now, LazyEncrypt is not available via Unity’s Package Manager, but that might change in the future.

---

## Anything Else?

I’m not a security expert. This project is based on my personal needs, and I encourage **anyone interested in improving it** to fork, modify, and collaborate.

This is **not the most secure solution**, but it’s **a simple and practical one** that works across multiple platforms. If you're dealing with **highly sensitive data**, consider **more advanced security measures** beyond LazyEncrypt.

If you have ideas for making this more robust, feel free to contribute.

---
