; (() => {
    function platform() {
        const ua = navigator.userAgent || '';
        const isAndroid = /Android/i.test(ua);
        const isWindows = /Windows/i.test(ua);

        const isTouch =
            ('ontouchstart' in window) ||
            (navigator.maxTouchPoints > 0) ||
            window.matchMedia('(pointer: coarse)').matches;

        // ancho efectivo del viewport visible (mejor que innerWidth solo)
        const vv = window.visualViewport;
        const effectiveWidth = Math.floor(
            Math.min(
                (vv && vv.width) || window.innerWidth || document.documentElement.clientWidth || 0,
                (screen && screen.width) || Number.POSITIVE_INFINITY
            )
        );

        // subí el corte a 1280 para tablets/landscape
        const isSmall = effectiveWidth <= 1280;

        return { isAndroid, isWindows, isTouch, isSmall, effectiveWidth };
    }

    function get() {
        const p = platform();
        // Política: en Android SIEMPRE preferir página completa.
        // Si no es Android, preferir página completa si es touch + “pantalla chica”.
        const preferSinglePage = p.isAndroid || (p.isTouch && p.isSmall);
        const os = p.isAndroid ? 'Android' : (p.isWindows ? 'Windows' : '');

        // útil para diagnosticar desde consola
        try { console.log('[deviceInfo]', { ...p, os, preferSinglePage }); } catch { }

        return { ...p, os, preferSinglePage };
    }

    window.deviceInfo = { get };
})();
