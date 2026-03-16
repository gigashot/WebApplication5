// EncryptApp - Authentication Module
var Auth = (function () {

    function showAlert(elementId, message, type) {
        var el = document.getElementById(elementId);
        el.className = 'alert alert-' + (type || 'error');
        el.textContent = message;
        el.classList.remove('hidden');
    }

    function hideAlert(elementId) {
        document.getElementById(elementId).classList.add('hidden');
    }

    function initRegister() {
        var form = document.getElementById('registerForm');
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            hideAlert('alert');

            var username = document.getElementById('username').value.trim();
            var password = document.getElementById('password').value;
            var confirmPassword = document.getElementById('confirmPassword').value;

            if (password !== confirmPassword) {
                showAlert('alert', 'Passwords do not match.');
                return;
            }

            if (password.length < 6) {
                showAlert('alert', 'Password must be at least 6 characters.');
                return;
            }

            var btn = document.getElementById('registerBtn');
            btn.disabled = true;
            btn.textContent = 'Creating account...';
            document.getElementById('progress').classList.add('active');

            // Generate encryption key pair
            Crypto.generateKeyPair()
                .then(function (keyPair) {
                    document.getElementById('progress').textContent = 'Saving encryption keys...';

                    // Export public key as base64 SPKI
                    return Crypto.exportPublicKey(keyPair.publicKey)
                        .then(function (publicKeyBase64) {
                            // Store private key in IndexedDB (keyed by username since we don't have userId yet)
                            return KeyStore.savePrivateKey(username, keyPair.privateKey)
                                .then(function () {
                                    return publicKeyBase64;
                                });
                        });
                })
                .then(function (publicKeyBase64) {
                    document.getElementById('progress').textContent = 'Registering account...';

                    // Register with server
                    return API.post('/auth/register', {
                        username: username,
                        password: password,
                        publicKey: publicKeyBase64
                    });
                })
                .then(function (result) {
                    if (result.success) {
                        showAlert('alert', 'Account created! Redirecting to login...', 'success');
                        setTimeout(function () {
                            window.location.href = '/Views/login.html';
                        }, 1500);
                    } else {
                        showAlert('alert', result.message || 'Registration failed.');
                        btn.disabled = false;
                        btn.textContent = 'Create Account';
                        document.getElementById('progress').classList.remove('active');
                    }
                })
                .catch(function (err) {
                    showAlert('alert', 'An error occurred: ' + err.message);
                    btn.disabled = false;
                    btn.textContent = 'Create Account';
                    document.getElementById('progress').classList.remove('active');
                });
        });
    }

    function initLogin() {
        // Check if already authenticated
        var user = Utils.getCurrentUser();
        if (user) {
            window.location.href = '/Views/dashboard.html';
            return;
        }

        var form = document.getElementById('loginForm');
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            hideAlert('alert');

            var username = document.getElementById('username').value.trim();
            var password = document.getElementById('password').value;

            var btn = document.getElementById('loginBtn');
            btn.disabled = true;
            btn.textContent = 'Signing in...';

            API.post('/auth/login', { username: username, password: password })
                .then(function (result) {
                    if (result.success) {
                        var userData = result.data;

                        // Check if we have the private key in IndexedDB
                        // Try both by userId and by username (registration stores by username)
                        return KeyStore.hasPrivateKey(userData.userId)
                            .then(function (hasById) {
                                if (hasById) return true;
                                // Check by username (in case registered on this browser)
                                return KeyStore.hasPrivateKey(username)
                                    .then(function (hasByName) {
                                        if (hasByName) {
                                            // Migrate key from username to userId
                                            return KeyStore.getPrivateKey(username)
                                                .then(function (key) {
                                                    return KeyStore.savePrivateKey(userData.userId, key);
                                                })
                                                .then(function () { return true; });
                                        }
                                        return false;
                                    });
                            })
                            .then(function (hasKey) {
                                // Store user data in session storage
                                Utils.setCurrentUser(userData);

                                if (!hasKey) {
                                    document.getElementById('keyWarning').classList.remove('hidden');
                                }

                                window.location.href = '/Views/dashboard.html';
                            });
                    } else {
                        showAlert('alert', result.message || 'Invalid credentials.');
                        btn.disabled = false;
                        btn.textContent = 'Sign In';
                    }
                })
                .catch(function (err) {
                    if (err.message !== 'Not authenticated') {
                        showAlert('alert', 'An error occurred. Please try again.');
                    }
                    btn.disabled = false;
                    btn.textContent = 'Sign In';
                });
        });
    }

    function logout() {
        API.post('/auth/logout')
            .then(function () {
                Utils.clearCurrentUser();
                window.location.href = '/Views/login.html';
            })
            .catch(function () {
                Utils.clearCurrentUser();
                window.location.href = '/Views/login.html';
            });
    }

    return {
        initRegister: initRegister,
        initLogin: initLogin,
        logout: logout
    };
})();
