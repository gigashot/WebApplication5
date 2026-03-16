// EncryptApp - Dashboard Module
var Dashboard = (function () {

    function init() {
        var user = Utils.requireAuth();
        if (!user) return;

        document.getElementById('navUsername').textContent = user.username;
        document.getElementById('logoutBtn').addEventListener('click', function () {
            Auth.logout();
        });

        // Load pending friend request count
        loadRequestCount();
    }

    function loadRequestCount() {
        API.get('/friends/requests')
            .then(function (result) {
                if (result.success && result.data && result.data.length > 0) {
                    var badge = document.getElementById('requestBadge');
                    badge.textContent = result.data.length;
                    badge.classList.remove('hidden');
                }
            })
            .catch(function () { /* ignore */ });
    }

    return {
        init: init
    };
})();
