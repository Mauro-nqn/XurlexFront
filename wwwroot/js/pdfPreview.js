//window.pdfPreview = {
//    createObjectUrl: function (bytes) {
//        const blob = new Blob([new Uint8Array(bytes)], { type: "application/pdf" });
//        return URL.createObjectURL(blob);
//    },
//    revokeObjectUrl: function (url) {
//        try { URL.revokeObjectURL(url); } catch { }
//    },
//    openModal: function (modalId) {
//        const el = document.getElementById(modalId);
//        const modal = bootstrap.Modal.getOrCreateInstance(el);
//        modal.show();
//    },
//    closeModal: function (modalId) {
//        const el = document.getElementById(modalId);
//        const modal = bootstrap.Modal.getOrCreateInstance(el);
//        modal.hide();
//    },
//    openNewTab: function (url) {
//        window.open(url, "_blank", "noopener,noreferrer");
//    },
//    printFrame: function (frameId) {
//        const iframe = document.getElementById(frameId);
//        if (!iframe) return;
//        iframe.contentWindow?.focus();
//        iframe.contentWindow?.print();
//    }
//};


window.pdfPreview = {
    createObjectUrl: function (bytes) {
        const blob = new Blob([new Uint8Array(bytes)], { type: "application/pdf" });
        return URL.createObjectURL(blob);
    },
    revokeObjectUrl: function (url) {
        try { URL.revokeObjectURL(url); } catch { }
    },
    openModal: function (modalId) {
        const el = document.getElementById(modalId);
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.show();
    },
    closeModal: function (modalId) {
        const el = document.getElementById(modalId);
        const modal = bootstrap.Modal.getOrCreateInstance(el);
        modal.hide();
    },
    openNewTab: function (url) {
        window.open(url, "_blank", "noopener,noreferrer");
    },
    printFrame: function (frameId) {
        const iframe = document.getElementById(frameId);
        if (!iframe) return;
        iframe.contentWindow?.focus();
        iframe.contentWindow?.print();
    },

    // ✅ NUEVO: descargar bytes como archivo
    downloadBytes: function (bytes, contentType, filename) {
        const blob = new Blob([new Uint8Array(bytes)], { type: contentType || "application/octet-stream" });
        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = filename || "archivo";
        document.body.appendChild(a);
        a.click();
        a.remove();

        setTimeout(() => URL.revokeObjectURL(url), 1000);
    },

    // ✅ NUEVO: abrir cliente de correo (sin adjunto)
    openMailto: function (mailtoUrl) {
        try {
            // ✅ marcar salida intencional (para que beforeunload no cierre sesión)
            if (window.__appNav && window.__appNav.markIntentionalExit) {
                window.__appNav.markIntentionalExit(8000); // 8s
            }

            // (opcional) también tu pausa de inactividad
            window.__inact = window.__inact || {};
            window.__inact.pauseUntil = Date.now() + 2 * 60 * 1000;

            const ov = document.getElementById("inactivity-overlay");
            if (ov) ov.remove();

            window.location.href = mailtoUrl;
        } catch { }
    }




};
