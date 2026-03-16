// EncryptApp - Profile Module
var Profile = (function () {

    function init() {
        var user = Utils.requireAuth();
        if (!user) return;

        document.getElementById('navUsername').textContent = user.username;
        document.getElementById('logoutBtn').addEventListener('click', function () {
            Auth.logout();
        });

        document.getElementById('profileUsername').textContent = user.username;
        document.getElementById('profileUserId').textContent = '#' + user.userId;
        document.getElementById('profilePublicKey').value = user.publicKey || 'Not available';

        // Check private key status
        KeyStore.hasPrivateKey(user.userId)
            .then(function (hasKey) {
                var statusEl = document.getElementById('keyStatus');
                if (hasKey) {
                    statusEl.innerHTML = '<span style="color: var(--success);">&#10003; Private key found in this browser</span>';
                } else {
                    statusEl.innerHTML = '<span style="color: var(--danger);">&#10007; No private key in this browser - messages cannot be decrypted</span>';
                }
            });
    }

    return {
        init: init
    };
})();
