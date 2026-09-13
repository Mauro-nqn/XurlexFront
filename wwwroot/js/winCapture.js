// winCapture.js: drag con Pointer Events (mouse + touch), sin polyfills.
// winCapture.js: drag con Pointer Events (mouse + touch), sin polyfills.
// 👇 util: viewport que respeta zoom/teclado en móviles
function viewBox() {
    const vv = window.visualViewport;
    return {
        left: vv ? vv.pageLeft : window.scrollX,
        top: vv ? vv.pageTop : window.scrollY,
        width: vv ? vv.width : window.innerWidth,
        height: vv ? vv.height : window.innerHeight
    };
}

// clamp contra el viewport visible (con scroll)
function clampToViewbox(left, top, el) {
    const { left: vx, top: vy, width: vw, height: vh } = viewBox();
    const w = el.offsetWidth || 300, h = el.offsetHeight || 200;
    const margin = 8, titleH = 44;

    const minL = vx + margin;
    const maxL = vx + vw - w - margin;
    const minT = vy + margin;
    const maxT = vy + vh - titleH - margin;

    return {
        left: Math.min(Math.max(minL, left), maxL),
        top: Math.min(Math.max(minT, top), maxT)
    };
}

// clamp contra el offsetParent que scrollea
function clampToOffsetParent(left, top, el) {
    const p = el.offsetParent || document.documentElement;
    const w = el.offsetWidth || 300, h = el.offsetHeight || 200;
    const margin = 8, titleH = 44;

    // scroll del contenedor (o del documento)
    const sL = (p === document.documentElement || p === document.body) ? window.scrollX : p.scrollLeft;
    const sT = (p === document.documentElement || p === document.body) ? window.scrollY : p.scrollTop;

    const minL = sL + margin;
    const maxL = sL + (p.clientWidth || viewBox().width) - w - margin;
    const minT = sT + margin;
    const maxT = sT + (p.clientHeight || viewBox().height) - titleH - margin;

    return {
        left: Math.min(Math.max(minL, left), maxL),
        top: Math.min(Math.max(minT, top), maxT)
    };
}

// elige automáticamente según dónde vive la ventana
function clampSmart(left, top, el) {
    const p = el.offsetParent;
    if (!p || p === document.body || p === document.documentElement)
        return clampToViewbox(left, top, el);
    return clampToOffsetParent(left, top, el);
}


(function () {
    // --- HUD opcional (debug sin consola) ---
    const HUD_MAX = 12;
    let hud, hudEnabled = false;
    function hudLog(msg) {
        if (!hudEnabled) return;
        if (!hud) {
            hud = document.createElement('div');
            hud.style.cssText = `
                position:fixed; right:8px; bottom:8px; z-index:2147483647;
                max-width:60vw; font:12px/1.3 monospace; 
                background:rgba(0,0,0,.75); color:#0f0; padding:8px; border-radius:8px;
                pointer-events:none; white-space:pre-wrap;
            `;
            document.body.appendChild(hud);
        }
        const ts = Math.floor(performance.now());
        const lines = (hud.textContent || '').split('\n').filter(Boolean);
        lines.push(`${ts}: ${msg}`);
        while (lines.length > HUD_MAX) lines.shift();
        hud.textContent = lines.join('\n');
    }
    window.winCaptureHUD = {
        enable(v) { hudEnabled = !!v; if (!v && hud && hud.parentNode) hud.parentNode.removeChild(hud), hud = null; }
    };
    //  logger accesible desde Blazor
    window.winCaptureLog = function (msg) { try { hudLog(String(msg)); } catch {} };

    // --- Utils
    function parsePx(v) { const n = parseFloat(v); return Number.isFinite(n) ? n : 0; }
    function clampToViewport(left, top, rootEl) {
        const vw = window.innerWidth, vh = window.innerHeight;
        const w = rootEl.offsetWidth || 300, h = rootEl.offsetHeight || 200;
        const margin = 8, titleH = 44;
        const maxLeft = Math.max(margin, vw - w - margin);
        const maxTop = Math.max(margin, vh - titleH - margin);
        return {
            left: Math.min(Math.max(margin, left), maxLeft),
            top: Math.min(Math.max(margin, top), maxTop)
        };
    }

    // --- ESCUDO (glass pane) durante drag y post-drop ---
    let shieldEl = null, shieldTimer = null;
    function blockAll(e){ e.preventDefault(); e.stopPropagation(); if (e.stopImmediatePropagation) e.stopImmediatePropagation(); }
    function ensureShield(){
        if (shieldEl) return shieldEl;
        const el = document.createElement('div');
        el.id = 'win-drag-shield';
        el.style.cssText = 'position:fixed;inset:0;z-index:2147483646;background:transparent;cursor:grabbing;user-select:none;-webkit-user-select:none;';
        ['click','mousedown','mouseup','pointerdown','pointerup','touchstart','touchend','contextmenu']
            .forEach(t => el.addEventListener(t, blockAll, true));
        shieldEl = el;
        return el;
    }
    function showShield(){ const el = ensureShield(); if (!el.isConnected) document.body.appendChild(el); hudLog('shield ON'); }
    function hideShield(){ if (shieldTimer){ clearTimeout(shieldTimer); shieldTimer=null; } if (shieldEl && shieldEl.parentNode){ shieldEl.parentNode.removeChild(shieldEl); hudLog('shield OFF'); } }
    function stopShieldAfter(ms){ if (shieldTimer) clearTimeout(shieldTimer); shieldTimer = setTimeout(hideShield, ms); }
    window.winCaptureShield = { start: showShield, stopAfter: stopShieldAfter };

    // --- Compuerta global anti-ghost ---
    let lastGhostUntilTs = 0;
    function armGhost(ms) {
        const now = performance.now();
        lastGhostUntilTs = Math.max(lastGhostUntilTs, now + ms);
        hudLog(`Ghost ARM ${ms}ms`);
    }
    function ghostGuard(e){
        if (performance.now() < lastGhostUntilTs) {
            blockAll(e);
            hudLog(`${e.type} cancelado (ghost)`);
            return false;
        }
    }
    // Captura amplia (up + down + click + contextmenu)
    ['click','mouseup','pointerup','touchend','mousedown','pointerdown','contextmenu'].forEach(t => {
        window.addEventListener(t, ghostGuard, true);
    });
    // API pública
    window.winCaptureArmGhost = armGhost;
    window.winCaptureIsGhostActive = function(){ return performance.now() < lastGhostUntilTs; };

    // --- Drag por Pointer Events ---
    window.winCapture = {
        attach: function (rootEl, handleEl, dotnetRef) {
            if (!rootEl || !handleEl) return;

            let startX = 0, startY = 0, startLeft = 0, startTop = 0;
            let dragging = false, moved = false;
            const DRAG_THRESHOLD = 8;        // px
            const GHOST_MS = 100;           // subimos a 1s para Android tercos

            function onDown(ev) {
                // si tocás sobre los botones, no empezar drag
                if (ev.target && ev.target.closest && ev.target.closest('.win-actions')) return;

                ev.preventDefault();
                ev.stopPropagation();

                dragging = true; moved = false;

                try { handleEl.setPointerCapture(ev.pointerId); } catch {}
                startX = ev.clientX ?? 0;
                startY = ev.clientY ?? 0;
                startLeft = parsePx(rootEl.style.left);
                startTop = parsePx(rootEl.style.top);

                // Activa escudo durante el drag
                showShield();

                rootEl.classList.add('is-any-dragging');
                hudLog(`down at ${startX},${startY}`);
            }

            function onMove(ev) {
                if (!dragging) return;
                const dx = (ev.clientX ?? 0) - startX;
                const dy = (ev.clientY ?? 0) - startY;

                if (!moved && (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD)) {
                    moved = true;
                    hudLog('drag threshold superado');
                }
                if (!moved) return;

                rootEl.style.left = (startLeft + dx) + 'px';
                rootEl.style.top  = (startTop  + dy) + 'px';
            }

            function end(ev) {

                if (!dragging) return;
                dragging = false;
                rootEl.classList.remove('is-any-dragging');
                try { handleEl.releasePointerCapture(ev.pointerId); } catch {}

                if (moved) {
                    const cl = clampToViewport(parsePx(rootEl.style.left), parsePx(rootEl.style.top), rootEl);
                    rootEl.style.left = cl.left + 'px';
                    rootEl.style.top  = cl.top  + 'px';

                    try { dotnetRef.invokeMethodAsync('CommitMove', cl.left, cl.top); } catch {}

                    // Bloqueo y escudo por un rato para tragar cualquier compat-event
                    armGhost(GHOST_MS);
                    stopShieldAfter(GHOST_MS);

                    try { dotnetRef.invokeMethodAsync('SuppressClicks', GHOST_MS); } catch {}
                    hudLog(`end drag -> clamp (${cl.left},${cl.top})`);
                } else {
                    hideShield();
                    hudLog('end (tap)');
                }
            }

            // Listeners (no passive donde usamos preventDefault)
            handleEl.addEventListener('pointerdown', onDown, { passive: false });
            handleEl.addEventListener('pointermove', onMove,   { passive: false });
            handleEl.addEventListener('pointerup',   end,      { passive: true  });
            handleEl.addEventListener('pointercancel', end,    { passive: true  });
            // 🔧 algunos navegadores solo emiten lostpointercapture
            handleEl.addEventListener('lostpointercapture', end, { passive: true });

            // Si cae un click sobre el handle en ventana ghost, cancelalo
            handleEl.addEventListener('click', function (e) {
                if (performance.now() < lastGhostUntilTs) { blockAll(e); hudLog('click handle cancelado (ghost)'); }
            }, true);
        }
    };
})();






