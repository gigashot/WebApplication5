// EncryptApp - IndexedDB Key Storage
var KeyStore = (function () {
    var DB_NAME = 'EncryptAppKeys';
    var STORE_NAME = 'keys';
    var DB_VERSION = 1;

    function openDb() {
        return new Promise(function (resolve, reject) {
            var req = indexedDB.open(DB_NAME, DB_VERSION);
            req.onupgradeneeded = function (e) {
                var db = e.target.result;
                if (!db.objectStoreNames.contains(STORE_NAME)) {
                    db.createObjectStore(STORE_NAME, { keyPath: 'id' });
                }
            };
            req.onsuccess = function () { resolve(req.result); };
            req.onerror = function () { reject(req.error); };
        });
    }

    function savePrivateKey(userId, cryptoKey) {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction(STORE_NAME, 'readwrite');
                var store = tx.objectStore(STORE_NAME);
                store.put({ id: 'privkey_' + userId, key: cryptoKey });
                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            });
        });
    }

    function getPrivateKey(userId) {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction(STORE_NAME, 'readonly');
                var store = tx.objectStore(STORE_NAME);
                var req = store.get('privkey_' + userId);
                req.onsuccess = function () {
                    resolve(req.result ? req.result.key : null);
                };
                req.onerror = function () { reject(req.error); };
            });
        });
    }

    function hasPrivateKey(userId) {
        return getPrivateKey(userId).then(function (key) {
            return key !== null;
        });
    }

    function deletePrivateKey(userId) {
        return openDb().then(function (db) {
            return new Promise(function (resolve, reject) {
                var tx = db.transaction(STORE_NAME, 'readwrite');
                var store = tx.objectStore(STORE_NAME);
                store.delete('privkey_' + userId);
                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            });
        });
    }

    return {
        savePrivateKey: savePrivateKey,
        getPrivateKey: getPrivateKey,
        hasPrivateKey: hasPrivateKey,
        deletePrivateKey: deletePrivateKey
    };
})();
