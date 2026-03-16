// EncryptApp - Shared Utilities
var Utils = (function () {

    function escapeHtml(str) {
        if (!str) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(str));
        return div.innerHTML;
    }

    function formatDate(isoString) {
        var date = new Date(isoString);
        var now = new Date();
        var diff = now - date;

        // Today: show time only
        if (date.toDateString() === now.toDateString()) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        // Yesterday
        var yesterday = new Date(now);
        yesterday.setDate(yesterday.getDate() - 1);
        if (date.toDateString() === yesterday.toDateString()) {
            return 'Yesterday ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        // This year
        if (date.getFullYear() === now.getFullYear()) {
            return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) +
                ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

        // Older
        return date.toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' });
    }

    function formatDateShort(isoString) {
        var date = new Date(isoString);
        var now = new Date();
        if (date.toDateString() === now.toDateString()) {
            return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' });
    }

    function debounce(fn, ms) {
        var timer;
        return function () {
            var context = this;
            var args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () {
                fn.apply(context, args);
            }, ms);
        };
    }

    // Toast notifications
    var toastContainer = null;

    function showToast(message, type) {
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container';
            document.body.appendChild(toastContainer);
        }

        var toast = document.createElement('div');
        toast.className = 'toast';
        if (type === 'error') toast.style.background = '#ef4444';
        if (type === 'success') toast.style.background = '#22c55e';
        toast.textContent = message;
        toastContainer.appendChild(toast);

        setTimeout(function () {
            toast.style.opacity = '0';
            toast.style.transition = 'opacity 0.3s';
            setTimeout(function () {
                if (toast.parentNode) toast.parentNode.removeChild(toast);
            }, 300);
        }, 3000);
    }

    // Get current user from session storage (set on login)
    function getCurrentUser() {
        var data = sessionStorage.getItem('encryptapp_user');
        return data ? JSON.parse(data) : null;
    }

    function setCurrentUser(user) {
        sessionStorage.setItem('encryptapp_user', JSON.stringify(user));
    }

    function clearCurrentUser() {
        sessionStorage.removeItem('encryptapp_user');
    }

    function requireAuth() {
        var user = getCurrentUser();
        if (!user) {
            window.location.href = '/Views/login.html';
            return null;
        }
        return user;
    }

    return {
        escapeHtml: escapeHtml,
        formatDate: formatDate,
        formatDateShort: formatDateShort,
        debounce: debounce,
        showToast: showToast,
        getCurrentUser: getCurrentUser,
        setCurrentUser: setCurrentUser,
        clearCurrentUser: clearCurrentUser,
        requireAuth: requireAuth
    };
})();
