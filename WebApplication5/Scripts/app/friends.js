// EncryptApp - Friends Management Module
var Friends = (function () {
    var currentUser = null;

    function init() {
        currentUser = Utils.requireAuth();
        if (!currentUser) return;

        document.getElementById('navUsername').textContent = currentUser.username;
        document.getElementById('logoutBtn').addEventListener('click', function () {
            Auth.logout();
        });

        loadRequests();
        loadFriends();
    }

    function loadRequests() {
        API.get('/friends/requests')
            .then(function (result) {
                var container = document.getElementById('requestsList');
                if (!result.success || !result.data || result.data.length === 0) {
                    container.innerHTML = '<div class="empty-state"><p>No pending requests</p></div>';
                    return;
                }

                var requests = result.data;
                var badge = document.getElementById('requestCount');
                badge.textContent = requests.length;
                badge.classList.remove('hidden');

                var html = '';
                for (var i = 0; i < requests.length; i++) {
                    var r = requests[i];
                    html += '<div class="list-item" id="request-' + r.requestId + '">' +
                        '<div class="list-item-info">' +
                        '<strong>' + Utils.escapeHtml(r.senderUsername) + '</strong>' +
                        '<span style="color: var(--gray-400); font-size: 13px; margin-left: 8px;">' +
                        Utils.formatDateShort(r.createdAt) + '</span>' +
                        '</div>' +
                        '<div class="list-item-actions">' +
                        '<button class="btn btn-success btn-sm" onclick="Friends.acceptRequest(' + r.requestId + ')">Accept</button>' +
                        '<button class="btn btn-danger btn-sm" onclick="Friends.rejectRequest(' + r.requestId + ')">Reject</button>' +
                        '</div></div>';
                }
                container.innerHTML = html;
            });
    }

    function loadFriends() {
        API.get('/friends')
            .then(function (result) {
                var container = document.getElementById('friendsList');
                if (!result.success || !result.data || result.data.length === 0) {
                    container.innerHTML = '<div class="empty-state"><p>No friends yet. Use the search to find people!</p></div>';
                    return;
                }

                var friends = result.data;
                var html = '';
                for (var i = 0; i < friends.length; i++) {
                    var f = friends[i];
                    var statusClass = f.isOnline ? 'status-online' : 'status-offline';
                    var statusText = f.isOnline ? 'Online' : 'Offline';

                    html += '<div class="list-item">' +
                        '<div class="list-item-info">' +
                        '<span class="status-dot ' + statusClass + '"></span>' +
                        '<strong>' + Utils.escapeHtml(f.username) + '</strong>' +
                        '<span style="color: var(--gray-400); font-size: 13px; margin-left: 8px;">' + statusText + '</span>' +
                        '</div>' +
                        '<div class="list-item-actions">' +
                        '<a href="/Views/chat.html?friend=' + f.userId + '" class="btn btn-primary btn-sm">Message</a>' +
                        '</div></div>';
                }
                container.innerHTML = html;
            });
    }

    function acceptRequest(requestId) {
        API.post('/friends/requests/' + requestId + '/accept')
            .then(function (result) {
                if (result.success) {
                    Utils.showToast('Friend request accepted!', 'success');
                    loadRequests();
                    loadFriends();
                } else {
                    Utils.showToast(result.message || 'Failed to accept request', 'error');
                }
            });
    }

    function rejectRequest(requestId) {
        API.post('/friends/requests/' + requestId + '/reject')
            .then(function (result) {
                if (result.success) {
                    var el = document.getElementById('request-' + requestId);
                    if (el) el.remove();
                    Utils.showToast('Request rejected');
                } else {
                    Utils.showToast(result.message || 'Failed to reject request', 'error');
                }
            });
    }

    return {
        init: init,
        acceptRequest: acceptRequest,
        rejectRequest: rejectRequest
    };
})();
