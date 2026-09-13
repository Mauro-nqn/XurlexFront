// /wwwroot/js/popovers.js
export function initPopovers() {
    // Requiere bootstrap.bundle (con Popover) cargado globalmente
    if (!window.bootstrap || !window.bootstrap.Popover) return;

    document.querySelectorAll('[data-bs-toggle="popover"]').forEach(el => {
        const existing = window.bootstrap.Popover.getInstance(el);
        if (existing) existing.dispose();
        new window.bootstrap.Popover(el); // trigger por defecto: 'hover focus'
    });
}

export function initTooltips() {
    if (!window.bootstrap || !window.bootstrap.Tooltip) return;

    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        const existing = window.bootstrap.Tooltip.getInstance(el);
        if (existing) existing.dispose();
        new window.bootstrap.Tooltip(el);
    });
}
