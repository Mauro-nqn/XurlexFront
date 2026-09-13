// wwwroot/js/googleLink.js
(function () {
    if (!window.googleLink) window.googleLink = {};
    let handler = null;

    window.googleLink.startListening = function (dotnetRef, allowed) {
        window.googleLink.stopListening();

        const allow = allowed === "*" ? "*" : (Array.isArray(allowed) ? allowed : [allowed]);

        handler = function (ev) {
            if (!ev || !ev.data) return;

            // El origin del MENSAJE es el BACKEND (5000)
            if (allow !== "*" && !allow.includes(ev.origin)) {
                console.warn('postMessage ignorado por origin', ev.origin, 'permitidos', allow);
                return;
            }

            console.log('postMessage recibido', ev.origin, ev.data);

            if (ev.data.type === "google-linked") {
                // 1) Aviso a Blazor para refrescar estado
                dotnetRef.invokeMethodAsync("OnGoogleLinked", ev.data.userId)
                    .then(() => {
                        // 2) Envío ACK al popup (callback) para que se cierre
                        try {
                            if (ev.source && typeof ev.source.postMessage === 'function') {
                                ev.source.postMessage({ type: 'google-linked-ack' }, ev.origin);
                            }
                        } catch (e) { console.error(e); }
                    });
            }
        };

        window.addEventListener("message", handler);
    };

    window.googleLink.stopListening = function () {
        if (handler) window.removeEventListener("message", handler);
        handler = null;
    };
})();
