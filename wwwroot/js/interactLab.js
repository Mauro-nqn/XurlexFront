// wwwroot/js/interact.win.js
(function () {
    // HUD simple
    let hud, hudOn = false;
    function hudLog(m) {
        if (!hudOn) return;
        if (!hud) {
            hud = document.createElement('div');
            hud.style.cssText = 'position:fixed;right:8px;bottom:8px;z-index:2147483647;background:rgba(0,0,0,.75);color:#0f0;padding:8px;border-radius:8px;font:12px monospace;pointer-events:none;max-width:65vw;white-space:pre-wrap';
            document.body.appendChild(hud);
        }
        const ts = Math.floor(performance.now());
        const lines = ((hud.textContent || '') + '\n' + ts + ': ' + m).split('\n').slice(-12);
        hud.textContent = lines.join('\n');
    }
    window.interactWinHUD = { enable(v) { hudOn = !!v; if (!v && hud?.parentNode) { hud.parentNode.removeChild(hud); hud = null; } } };

    // utils
    function px(n) { return Number.isFinite(n) ? n : 0; }
    function parsePx(v) { const n = parseFloat(v); return Number.isFinite(n) ? n : 0; }
    function clamp(left, top, el) {
        const vw = window.innerWidth, vh = window.innerHeight;
        const w = el.offsetWidth || 300, h = el.offsetHeight || 200;
        const margin = 8, titleH = 44;
        const maxLeft = Math.max(margin, vw - w - margin);
        const maxTop = Math.max(margin, vh - titleH - margin);
        return {
            left: Math.min(Math.max(margin, left), maxLeft),
            top: Math.min(Math.max(margin, top), maxTop)
        };
    }

    // Compuerta anti-ghost: solo DOWNs y CLICK/CONTEXTMENU
    let ghostUntil = 0;
    function armGhost(ms) { ghostUntil = Math.max(ghostUntil, performance.now() + ms); hudLog('Ghost ARM ' + ms + 'ms'); }
    ['click', 'mousedown', 'pointerdown', 'contextmenu'].forEach(t => {
        window.addEventListener(t, e => {
            if (performance.now() < ghostUntil) {
                e.preventDefault(); e.stopPropagation(); e.stopImmediatePropagation?.();
                hudLog(t + ' cancelado (ghost)');
            }
        }, true);
    });

    // API
    window.interactWin = {
        attach(rootEl, handleEl) {
            if (!window.interact || !rootEl || !handleEl) return;

            // estilos base
            const cs = getComputedStyle(rootEl);
            if (cs.position !== 'absolute') { rootEl.style.position = 'absolute'; }
            handleEl.style.touchAction = 'none';

            let moved = false;
            const GHOST_MS = 900;
            const MIN_W = 300, MIN_H = 180;

            // --- DRAG (sólo si NO está maximizada/minimizada)
            window.interact(handleEl).draggable({
                allowFrom: handleEl,
                ignoreFrom: '.win-actions, .lab-content *',
                inertia: false,
                listeners: {
                    start(ev) {
                        if (rootEl.classList.contains('maximized') || rootEl.classList.contains('minimized')) {
                            ev.interaction.stop(); // no arrastrar en esos estados
                            return;
                        }
                        moved = false;
                        rootEl.classList.add('is-any-dragging');
                        hudLog(`start @ ${ev.clientX},${ev.clientY}`);
                    },
                    move(ev) {
                        const dx = ev.dx || 0, dy = ev.dy || 0;
                        if (dx !== 0 || dy !== 0) moved = true;
                        rootEl.style.left = (parsePx(rootEl.style.left) + dx) + 'px';
                        rootEl.style.top = (parsePx(rootEl.style.top) + dy) + 'px';
                    },
                    end() {
                        rootEl.classList.remove('is-any-dragging');
                        if (moved) {
                            const cl = clamp(parsePx(rootEl.style.left), parsePx(rootEl.style.top), rootEl);
                            rootEl.style.left = cl.left + 'px'; rootEl.style.top = cl.top + 'px';
                            armGhost(GHOST_MS);
                            hudLog(`end drag -> clamp (${cl.left},${cl.top})`);
                        } else {
                            hudLog('end (tap)');
                        }
                    }
                },
                modifiers: [
                    window.interact.modifiers.restrictRect({
                        restriction: { top: 8, left: 8, bottom: window.innerHeight - 44 - 8, right: window.innerWidth - 8 },
                        endOnly: false
                    })
                ]
            });

            // --- RESIZE (borde derecho e inferior)
            window.interact(rootEl).resizable({
                edges: { left: false, top: false, right: true, bottom: true },
                inertia: false,
                listeners: {
                    move(ev) {
                        if (rootEl.classList.contains('maximized') || rootEl.classList.contains('minimized')) return;

                        let w = px(ev.rect.width), h = px(ev.rect.height);
                        // clamp a viewport y mínimos
                        const left = parsePx(rootEl.style.left), top = parsePx(rootEl.style.top);
                        const maxW = Math.max(MIN_W, window.innerWidth - 8 - left);
                        const maxH = Math.max(MIN_H, window.innerHeight - 8 - top);
                        w = Math.min(Math.max(MIN_W, w), maxW);
                        h = Math.min(Math.max(MIN_H, h), maxH);
                        rootEl.style.width = w + 'px';
                        rootEl.style.height = h + 'px';
                    },
                    end() {
                        armGhost(600);
                        hudLog('end resize');
                    }
                }
            });

            // --- Controles de ventana
            function saveRect() {
                const r = rootEl.getBoundingClientRect();
                rootEl.dataset.prevLeft = rootEl.style.left || (r.left + 'px');
                rootEl.dataset.prevTop = rootEl.style.top || (r.top + 'px');
                rootEl.dataset.prevWidth = rootEl.style.width || (r.width + 'px');
                rootEl.dataset.prevHeight = rootEl.style.height || (r.height + 'px');
            }
            function restoreRect() {
                const L = rootEl.dataset.prevLeft, T = rootEl.dataset.prevTop, W = rootEl.dataset.prevWidth, H = rootEl.dataset.prevHeight;
                if (L) rootEl.style.left = L;
                if (T) rootEl.style.top = T;
                if (W) rootEl.style.width = W;
                if (H) rootEl.style.height = H;
            }

            this.minimize = function (el) {
                const e = el || rootEl;
                if (!e.classList.contains('minimized')) {
                    // guardar height para restaurar
                    const h = e.style.height || e.getBoundingClientRect().height + 'px';
                    e.dataset.prevHeight = h;
                    e.classList.add('minimized');
                    e.style.height = '44px';
                    armGhost(500);
                    hudLog('minimize');
                } else {
                    e.classList.remove('minimized');
                    const ph = e.dataset.prevHeight;
                    if (ph) e.style.height = ph;
                    armGhost(300);
                    hudLog('restore from minimized');
                }
            };

            this.toggleMaximize = function (el) {
                const e = el || rootEl;
                if (!e.classList.contains('maximized')) {
                    saveRect();
                    e.classList.add('maximized');
                    e.style.left = '8px'; e.style.top = '8px';
                    e.style.width = (window.innerWidth - 16) + 'px';
                    e.style.height = (window.innerHeight - 16) + 'px';
                    armGhost(500);
                    hudLog('maximize');
                } else {
                    e.classList.remove('maximized');
                    restoreRect();
                    armGhost(300);
                    hudLog('restore from maximized');
                }
            };

            this.close = function (el) {
                const e = el || rootEl;
                e.style.display = 'none';
                armGhost(200);
                hudLog('close');
            };
            this.open = function (el) {
                const e = el || rootEl;
                e.style.display = 'block';
                armGhost(200);
                hudLog('open');
            };
        },

        // Métodos estáticos accesibles desde Blazor:
        minimize: function (rootEl) { this._call(rootEl, 'minimize'); },
        toggleMaximize: function (rootEl) { this._call(rootEl, 'toggleMaximize'); },
        close: function (rootEl) { this._call(rootEl, 'close'); },
        open: function (rootEl) { this._call(rootEl, 'open'); },

        // Helper interno
        _call: function (rootEl, method) {
            // Busca la instancia attach más reciente en window.interactWin (simple)
            // Como attach define métodos en 'this', volvemos a llamarlo vía el prototipo actual:
            const api = this;
            if (typeof api[method] === 'function') {
                // si el método está “bound” a la instancia, úsalo
                try { api[method](rootEl); return; } catch { }
            }
            // fallback: si se perdió el binding, re-atachear rápido
            try {
                const handleEl = rootEl?.querySelector?.('.lab-title');
                if (handleEl) this.attach(rootEl, handleEl);
            } catch { }
            // y reintentar
            try {
                if (typeof api[method] === 'function') api[method](rootEl);
            } catch { }
        }
    };

    // Wrapper seguro (lo usás desde Razor)
    window.interactWinAttachSafe = function (rootEl, handleEl) {
        (function wait() {
            if (window.interact && window.interactWin && typeof window.interactWin.attach === 'function') {
                try { window.interactWin.attach(rootEl, handleEl); } catch (e) { }
            } else { setTimeout(wait, 80); }
        })();
    };
})();

