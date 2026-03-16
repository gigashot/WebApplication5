// EncryptApp - Web Crypto API (RSA-OAEP + AES-GCM Hybrid Encryption)
var Crypto = (function () {

    // === Helper functions ===

    function arrayBufferToBase64(buffer) {
        var bytes = new Uint8Array(buffer);
        var binary = '';
        for (var i = 0; i < bytes.length; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary);
    }

    function base64ToArrayBuffer(base64) {
        var binary = atob(base64);
        var bytes = new Uint8Array(binary.length);
        for (var i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes.buffer;
    }

    // === Key Generation ===

    function generateKeyPair() {
        return window.crypto.subtle.generateKey(
            {
                name: 'RSA-OAEP',
                modulusLength: 2048,
                publicExponent: new Uint8Array([1, 0, 1]),
                hash: 'SHA-256'
            },
            true, // extractable (needed to export the public key)
            ['wrapKey', 'unwrapKey']
        );
    }

    function exportPublicKey(publicKey) {
        return window.crypto.subtle.exportKey('spki', publicKey)
            .then(function (exported) {
                return arrayBufferToBase64(exported);
            });
    }

    function importPublicKey(base64Spki) {
        var binaryDer = base64ToArrayBuffer(base64Spki);
        return window.crypto.subtle.importKey(
            'spki',
            binaryDer,
            { name: 'RSA-OAEP', hash: 'SHA-256' },
            false,
            ['wrapKey']
        );
    }

    function importPrivateKey(base64Pkcs8) {
        var binaryDer = base64ToArrayBuffer(base64Pkcs8);
        return window.crypto.subtle.importKey(
            'pkcs8',
            binaryDer,
            { name: 'RSA-OAEP', hash: 'SHA-256' },
            false,
            ['unwrapKey']
        );
    }

    // === Hybrid Encryption ===

    function encryptMessage(plaintext, recipientPublicKeyBase64) {
        var aesKey, iv, ciphertext, wrappedKey;

        // 1. Generate ephemeral AES-GCM 256-bit key
        return window.crypto.subtle.generateKey(
            { name: 'AES-GCM', length: 256 },
            true, // extractable so we can wrap it
            ['encrypt', 'decrypt']
        ).then(function (key) {
            aesKey = key;

            // 2. Encrypt plaintext with AES-GCM
            iv = window.crypto.getRandomValues(new Uint8Array(12));
            var encoded = new TextEncoder().encode(plaintext);
            return window.crypto.subtle.encrypt(
                { name: 'AES-GCM', iv: iv },
                aesKey,
                encoded
            );
        }).then(function (encrypted) {
            ciphertext = encrypted;

            // 3. Import recipient's RSA public key
            return importPublicKey(recipientPublicKeyBase64);
        }).then(function (recipientPubKey) {
            // 4. Wrap (encrypt) the AES key with recipient's RSA-OAEP public key
            return window.crypto.subtle.wrapKey(
                'raw',
                aesKey,
                recipientPubKey,
                { name: 'RSA-OAEP' }
            );
        }).then(function (wrapped) {
            wrappedKey = wrapped;

            // 5. Package everything as JSON (all base64-encoded)
            return JSON.stringify({
                wrappedKey: arrayBufferToBase64(wrappedKey),
                iv: arrayBufferToBase64(iv),
                ciphertext: arrayBufferToBase64(ciphertext)
            });
        });
    }

    function decryptMessage(encryptedPayload, privateKey) {
        var payload = JSON.parse(encryptedPayload);

        var wrappedKeyBuf = base64ToArrayBuffer(payload.wrappedKey);
        var ivBuf = base64ToArrayBuffer(payload.iv);
        var ciphertextBuf = base64ToArrayBuffer(payload.ciphertext);

        // 1. Unwrap the AES key using our RSA-OAEP private key
        return window.crypto.subtle.unwrapKey(
            'raw',
            wrappedKeyBuf,
            privateKey,
            { name: 'RSA-OAEP' },
            { name: 'AES-GCM', length: 256 },
            false,
            ['decrypt']
        ).then(function (aesKey) {
            // 2. Decrypt the ciphertext with AES-GCM
            return window.crypto.subtle.decrypt(
                { name: 'AES-GCM', iv: new Uint8Array(ivBuf) },
                aesKey,
                ciphertextBuf
            );
        }).then(function (decrypted) {
            // 3. Decode to string
            return new TextDecoder().decode(decrypted);
        });
    }

    return {
        generateKeyPair: generateKeyPair,
        exportPublicKey: exportPublicKey,
        importPublicKey: importPublicKey,
        importPrivateKey: importPrivateKey,
        encryptMessage: encryptMessage,
        decryptMessage: decryptMessage,
        arrayBufferToBase64: arrayBufferToBase64,
        base64ToArrayBuffer: base64ToArrayBuffer
    };
})();
