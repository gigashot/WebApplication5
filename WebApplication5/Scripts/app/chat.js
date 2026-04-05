// EncryptApp - Chat Module
var Chat = (function () {
    var currentUser = null;
    var privateKey = null;
    var friends = [];
    var activeFriendId = null;
    var activeFriend = null;
    var currentPage = 1;
    var pageSize = 50;
    var typingTimeout = null;
    var lastTypingSent = 0;

    function init() {
        currentUser = Utils.requireAuth();
        if (!currentUser) return;

        document.getElementById('navUsername').textContent = currentUser.username;
        document.getElementById('logoutBtn').addEventListener('click', function () {
            Auth.logout();
        });

        // Load private key from IndexedDB
        KeyStore.getPrivateKey(currentUser.userId)
            .then(function (key) {
                privateKey = key;
                if (!key) {
                    Utils.showToast('No encryption key found. Messages cannot be decrypted.', 'error');
                }
            })
            .then(loadFriends)
            .then(initSignalR)
            .then(setupInputHandlers);
    }

    function loadFriends() {
        return API.get('/friends')
            .then(function (result) {
                if (result.success) {
                    friends = result.data || [];
                    renderFriendsList();
                }
            });
    }

    function renderFriendsList() {
        var container = document.getElementById('friendsList');
        if (friends.length === 0) {
            container.innerHTML = '<div class="empty-state"><p>No friends yet. Add some friends to start chatting!</p></div>';
            return;
        }

        var html = '';
        for (var i = 0; i < friends.length; i++) {
            var f = friends[i];
            var initial = f.username.charAt(0).toUpperCase();
            var activeClass = f.userId === activeFriendId ? ' active' : '';
            var statusClass = f.isOnline ? 'status-online' : 'status-offline';

            html += '<div class="chat-friend-item' + activeClass + '" data-userid="' + f.userId + '">' +
                '<div class="chat-friend-avatar">' + Utils.escapeHtml(initial) + '</div>' +
                '<div class="chat-friend-info">' +
                '<div class="chat-friend-name"><span class="status-dot ' + statusClass + '"></span>' +
                Utils.escapeHtml(f.username) + '</div>' +
                '<div class="chat-friend-status">' + (f.isOnline ? 'Online' : 'Offline') + '</div>' +
                '</div></div>';
        }
        container.innerHTML = html;

        // Attach click handlers
        var items = container.querySelectorAll('.chat-friend-item');
        for (var j = 0; j < items.length; j++) {
            items[j].addEventListener('click', function () {
                var userId = parseInt(this.getAttribute('data-userid'));
                openConversation(userId);
            });
        }
    }

    function openConversation(friendId) {
        activeFriendId = friendId;
        activeFriend = null;
        for (var i = 0; i < friends.length; i++) {
            if (friends[i].userId === friendId) {
                activeFriend = friends[i];
                break;
            }
        }
        if (!activeFriend) return;

        // Update UI
        document.getElementById('chatEmpty').classList.add('hidden');
        var chatActive = document.getElementById('chatActive');
        chatActive.classList.remove('hidden');
        chatActive.style.display = 'flex';

        document.getElementById('chatAvatar').textContent = activeFriend.username.charAt(0).toUpperCase();
        document.getElementById('chatHeaderName').textContent = activeFriend.username;
        document.getElementById('chatHeaderStatus').textContent = activeFriend.isOnline ? 'Online' : 'Offline';
        document.getElementById('chatTyping').textContent = '';

        // Highlight in sidebar
        renderFriendsList();

        // Clear and load messages
        currentPage = 1;
        document.getElementById('chatMessages').innerHTML = '<div class="chat-load-more hidden" id="loadMore"><button id="loadMoreBtn">Load older messages</button></div>';

        loadMessages(true);
    }

    function loadMessages(scrollToBottom) {
        API.get('/messages/' + activeFriendId + '?page=' + currentPage + '&pageSize=' + pageSize)
            .then(function (result) {
                if (result.success && result.data) {
                    var messages = result.data;
                    if (messages.length === pageSize) {
                        var loadMore = document.getElementById('loadMore');
                        if (loadMore) {
                            loadMore.classList.remove('hidden');
                            document.getElementById('loadMoreBtn').onclick = function () {
                                currentPage++;
                                loadMessages(false);
                            };
                        }
                    }

                    // Decrypt and render messages (newest first from server, reverse for display)
                    renderMessages(messages.reverse(), scrollToBottom);
                }
            });
    }

    function renderMessages(messages, scrollToBottom) {
        var container = document.getElementById('chatMessages');
        var fragment = document.createDocumentFragment();

        var promises = messages.map(function (msg) {
            return decryptAndRender(msg);
        });

        Promise.all(promises).then(function (elements) {
            for (var i = 0; i < elements.length; i++) {
                if (elements[i]) fragment.appendChild(elements[i]);
            }
            container.appendChild(fragment);

            if (scrollToBottom) {
                container.scrollTop = container.scrollHeight;
            }

            // Mark unread messages as read
            messages.forEach(function (msg) {
                if (msg.receiverId === currentUser.userId && !msg.read) {
                    SignalRClient.markRead(msg.messageId);
                }
            });
        });
    }

    function decryptAndRender(msg) {
        var isSent = msg.senderId === currentUser.userId;

        if (!privateKey) {
            return Promise.resolve(createMessageElement('[Encrypted - no key available]', msg, isSent));
        }

        return Crypto.decryptMessage(msg.encryptedContent, privateKey)
            .then(function (plaintext) {
                return createMessageElement(plaintext, msg, isSent);
            })
            .catch(function () {
                return createMessageElement('[Unable to decrypt]', msg, isSent);
            });
    }

    function createMessageElement(text, msg, isSent) {
        var div = document.createElement('div');
        div.className = 'chat-message ' + (isSent ? 'chat-message-sent' : 'chat-message-received');
        div.setAttribute('data-msgid', msg.messageId);

        var statusHtml = '';
        if (isSent) {
            if (msg.read) statusHtml = '<span class="msg-status msg-status-read"></span>';
            else if (msg.delivered) statusHtml = '<span class="msg-status msg-status-delivered"></span>';
            else statusHtml = '<span class="msg-status msg-status-sent"></span>';
        }

        div.innerHTML = '<div class="chat-message-text">' + Utils.escapeHtml(text) + '</div>' +
            '<div class="chat-message-time">' + Utils.formatDate(msg.sentAt) + ' ' + statusHtml + '</div>';

        return div;
    }

    function appendMessage(text, msg, isSent) {
        var container = document.getElementById('chatMessages');
        var el = createMessageElement(text, msg, isSent);
        container.appendChild(el);
        container.scrollTop = container.scrollHeight;
    }

    function initSignalR() {
        return SignalRClient.init(currentUser.userId, {
            onMessage: handleIncomingMessage,
            onMessageSent: handleMessageSent,
            onDelivered: handleDelivered,
            onRead: handleRead,
            onUserOnline: handleUserOnline,
            onUserOffline: handleUserOffline,
            onTyping: handleTyping,
            onNewRequest: function () {
                Utils.showToast('New friend request received!');
            }
        });
    }

    function handleIncomingMessage(dto) {
        // If this message is from the active conversation, decrypt and display
        if (dto.senderId === activeFriendId) {
            if (privateKey) {
                Crypto.decryptMessage(dto.encryptedContent, privateKey)
                    .then(function (plaintext) {
                        appendMessage(plaintext, dto, false);
                        SignalRClient.markRead(dto.messageId);
                    })
                    .catch(function () {
                        appendMessage('[Unable to decrypt]', dto, false);
                    });
            } else {
                appendMessage('[Encrypted - no key]', dto, false);
            }
        } else {
            // Show notification for other conversations
            var senderName = 'Someone';
            for (var i = 0; i < friends.length; i++) {
                if (friends[i].userId === dto.senderId) {
                    senderName = friends[i].username;
                    break;
                }
            }
            Utils.showToast('New message from ' + senderName);
        }
    }

    function handleMessageSent(dto) {
        // Update the temp message with real data if needed
    }

    function handleDelivered(messageId) {
        updateMessageStatus(messageId, 'delivered');
    }

    function handleRead(messageId) {
        updateMessageStatus(messageId, 'read');
    }

    function updateMessageStatus(messageId, status) {
        var el = document.querySelector('[data-msgid="' + messageId + '"] .msg-status');
        if (el) {
            el.className = 'msg-status msg-status-' + status;
        }
    }

    function handleUserOnline(userId) {
        for (var i = 0; i < friends.length; i++) {
            if (friends[i].userId === userId) {
                friends[i].isOnline = true;
                break;
            }
        }
        renderFriendsList();
        if (userId === activeFriendId) {
            document.getElementById('chatHeaderStatus').textContent = 'Online';
        }
    }

    function handleUserOffline(userId) {
        for (var i = 0; i < friends.length; i++) {
            if (friends[i].userId === userId) {
                friends[i].isOnline = false;
                break;
            }
        }
        renderFriendsList();
        if (userId === activeFriendId) {
            document.getElementById('chatHeaderStatus').textContent = 'Offline';
        }
    }

    function handleTyping(fromUserId) {
        if (fromUserId === activeFriendId) {
            var el = document.getElementById('chatTyping');
            el.textContent = 'typing...';
            clearTimeout(typingTimeout);
            typingTimeout = setTimeout(function () {
                el.textContent = '';
            }, 3000);
        }
    }

    function setupInputHandlers() {
        var input = document.getElementById('messageInput');
        var sendBtn = document.getElementById('sendBtn');

        // Send on Enter (Shift+Enter for newline)
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });

        // Typing indicator (debounced)
        input.addEventListener('input', function () {
            if (activeFriendId && Date.now() - lastTypingSent > 2000) {
                lastTypingSent = Date.now();
                SignalRClient.sendTyping(activeFriendId);
            }

            // Auto-resize textarea
            this.style.height = 'auto';
            this.style.height = Math.min(this.scrollHeight, 100) + 'px';
        });

        sendBtn.addEventListener('click', sendMessage);
    }

    function sendMessage() {
        var input = document.getElementById('messageInput');
        var text = input.value.trim();
        if (!text || !activeFriend) return;

        input.value = '';
        input.style.height = 'auto';

        // Encrypt for recipient AND sender (so sender can read their own messages later)
        Promise.all([
            Crypto.encryptMessage(text, activeFriend.publicKey),
            Crypto.encryptMessage(text, currentUser.publicKey)
        ])
            .then(function (results) {
                var encryptedForRecipient = results[0];
                var encryptedForSender = results[1];

                // Show message locally immediately
                var tempMsg = {
                    messageId: 'temp_' + Date.now(),
                    senderId: currentUser.userId,
                    receiverId: activeFriendId,
                    encryptedContent: encryptedForSender,
                    sentAt: new Date().toISOString(),
                    delivered: false,
                    read: false
                };
                appendMessage(text, tempMsg, true);

                // Send both encrypted copies via SignalR
                return SignalRClient.sendMessage(activeFriendId, encryptedForRecipient, encryptedForSender);
            })
            .catch(function (err) {
                Utils.showToast('Failed to send message: ' + err, 'error');
            });
    }

    return {
        init: init
    };
})();
