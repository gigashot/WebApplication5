// EncryptApp - User Search Module
var Search = (function () {
    var currentUser = null;

    function init() {
        currentUser = Utils.requireAuth();
        if (!currentUser) return;

        document.getElementById('navUsername').textContent = currentUser.username;
        document.getElementById('logoutBtn').addEventListener('click', function () {
            Auth.logout();
        });

        var input = document.getElementById('searchInput');
        input.addEventListener('input', Utils.debounce(function () {
            var query = input.value.trim();
            if (query.length < 1) {
                document.getElementById('searchResults').innerHTML =
                    '<div class="empty-state"><p>Type a username to search</p></div>';
                return;
            }
            performSearch(query);
        }, 300));

        input.focus();
    }

    function performSearch(query) {
        var container = document.getElementById('searchResults');
        container.innerHTML = '<div class="loading">Searching...</div>';

        API.get('/users/search?q=' + encodeURIComponent(query))
            .then(function (result) {
                if (!result.success || !result.data || result.data.length === 0) {
                    container.innerHTML = '<div class="empty-state"><p>No users found</p></div>';
                    return;
                }

                var users = result.data;
                var html = '';
                for (var i = 0; i < users.length; i++) {
                    var u = users[i];
                    html += '<div class="list-item">' +
                        '<div class="list-item-info">' +
                        '<strong>' + Utils.escapeHtml(u.username) + '</strong>' +
                        '</div>' +
                        '<div class="list-item-actions">' +
                        '<button class="btn btn-primary btn-sm" onclick="Search.sendRequest(' + u.userId + ', this)">Add Friend</button>' +
                        '</div></div>';
                }
                container.innerHTML = html;
            });
    }

    function sendRequest(userId, btn) {
        btn.disabled = true;
        btn.textContent = 'Sending...';

        API.post('/friends/request', { receiverId: userId })
            .then(function (result) {
                if (result.success) {
                    btn.textContent = 'Request Sent';
                    btn.className = 'btn btn-secondary btn-sm';
                    Utils.showToast('Friend request sent!', 'success');
                } else {
                    btn.textContent = result.message || 'Failed';
                    btn.disabled = false;
                    Utils.showToast(result.message || 'Failed to send request', 'error');
                }
            })
            .catch(function () {
                btn.textContent = 'Add Friend';
                btn.disabled = false;
            });
    }

    return {
        init: init,
        sendRequest: sendRequest
    };
})();
