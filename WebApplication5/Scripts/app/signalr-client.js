// EncryptApp - SignalR Client
var SignalRClient = (function () {
    var hub = null;
    var userId = null;
    var connected = false;

    function init(uid, callbacks) {
        userId = uid;
        hub = $.connection.chatHub;

        // Register client-side callback methods
        hub.client.receiveMessage = function (dto) {
            if (callbacks.onMessage) callbacks.onMessage(dto);
        };

        hub.client.messageSent = function (dto) {
            if (callbacks.onMessageSent) callbacks.onMessageSent(dto);
        };

        hub.client.messageDelivered = function (messageId) {
            if (callbacks.onDelivered) callbacks.onDelivered(messageId);
        };

        hub.client.messageRead = function (messageId) {
            if (callbacks.onRead) callbacks.onRead(messageId);
        };

        hub.client.userOnline = function (uid) {
            if (callbacks.onUserOnline) callbacks.onUserOnline(uid);
        };

        hub.client.userOffline = function (uid) {
            if (callbacks.onUserOffline) callbacks.onUserOffline(uid);
        };

        hub.client.typingIndicator = function (fromUserId) {
            if (callbacks.onTyping) callbacks.onTyping(fromUserId);
        };

        hub.client.refreshRequests = function () {
            if (callbacks.onNewRequest) callbacks.onNewRequest();
        };

        hub.client.error = function (message) {
            Utils.showToast(message, 'error');
        };

        // Configure connection
        $.connection.hub.qs = { userId: userId };

        // Connection state change events
        $.connection.hub.reconnecting(function () {
            Utils.showToast('Reconnecting...', 'warning');
        });

        $.connection.hub.reconnected(function () {
            Utils.showToast('Reconnected', 'success');
            hub.server.register(userId);
        });

        $.connection.hub.disconnected(function () {
            connected = false;
            // Auto-reconnect after 5 seconds
            setTimeout(function () {
                if (!connected) {
                    $.connection.hub.start().done(function () {
                        connected = true;
                        hub.server.register(userId);
                    });
                }
            }, 5000);
        });

        // Start connection
        return $.connection.hub.start().done(function () {
            connected = true;
            hub.server.register(userId);
        });
    }

    function sendMessage(toUserId, encryptedPayload) {
        if (!hub || !connected) {
            return $.Deferred().reject('Not connected').promise();
        }
        return hub.server.sendMessage(toUserId, encryptedPayload);
    }

    function sendTyping(toUserId) {
        if (!hub || !connected) return;
        hub.server.sendTypingIndicator(toUserId);
    }

    function markRead(messageId) {
        if (!hub || !connected) return;
        hub.server.markMessageRead(messageId);
    }

    function markDelivered(messageId) {
        if (!hub || !connected) return;
        hub.server.markMessageDelivered(messageId);
    }

    function isConnected() {
        return connected;
    }

    return {
        init: init,
        sendMessage: sendMessage,
        sendTyping: sendTyping,
        markRead: markRead,
        markDelivered: markDelivered,
        isConnected: isConnected
    };
})();
