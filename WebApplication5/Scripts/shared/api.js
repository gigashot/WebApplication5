// EncryptApp - API Client
var API = (function () {
    var baseUrl = '/api';

    function request(method, path, body) {
        var opts = {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin'
        };
        if (body) {
            opts.body = JSON.stringify(body);
        }
        return fetch(baseUrl + path, opts)
            .then(function (res) {
                if (res.status === 401) {
                    Utils.clearCurrentUser();
                    window.location.href = '/Views/login.html';
                    return Promise.reject(new Error('Not authenticated'));
                }
                return res.json();
            });
    }

    return {
        get: function (path) { return request('GET', path); },
        post: function (path, body) { return request('POST', path, body); },
        put: function (path, body) { return request('PUT', path, body); },
        del: function (path) { return request('DELETE', path); }
    };
})();
