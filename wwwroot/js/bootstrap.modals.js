//window.bsModal = {
//    show: (selector) => {
//        const el = document.querySelector(selector);
//        if (!el) return;
//        const m = bootstrap.Modal.getOrCreateInstance(el);
//        m.show();
//    },
//    hide: (selector) => {
//        const el = document.querySelector(selector);
//        if (!el) return;
//        const m = bootstrap.Modal.getOrCreateInstance(el);
//        m.hide();
//    }
//};

window.bsModal = window.bsModal || {};

/* Mostrar modal */
window.bsModal.show = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return;
    el.removeAttribute("inert");
    bootstrap.Modal.getOrCreateInstance(el).show();
};


/* Ocultar modal normal */
window.bsModal.hide = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return;

    const inst = bootstrap.Modal.getOrCreateInstance(el);
    inst.hide();
};

/* 🔥 Limpieza segura usando inert */
//window.bsModal.cleanup = () => {
//    try {
//        // 1) Sacar foco del modal (evita warning y estados zombis)
//        const active = document.activeElement;
//        if (active && active !== document.body && typeof active.blur === "function") active.blur();

//        // 2) Remover backdrops colgados
//        document.querySelectorAll(".modal-backdrop, .offcanvas-backdrop").forEach(b => b.remove());

//        // 3) Reset body/html
//        document.body.classList.remove("modal-open");
//        document.body.style.removeProperty("padding-right");
//        document.body.style.removeProperty("overflow");
//        document.documentElement.style.removeProperty("overflow");
//        document.body.style.pointerEvents = "auto";
//    } catch { }
//};

//window.bsModal.cleanupHard = () => {
//    window.bsModal.cleanup();
//    setTimeout(window.bsModal.cleanup, 200);
//    setTimeout(window.bsModal.cleanup, 1000);
//};

//window.bsModal.ensureBackdropIfModalOpen = () => {
//    const anyOpen = document.querySelectorAll(".modal.show").length > 0;
//    if (!anyOpen) return;

//    // Asegurar body bloqueado como modal
//    document.body.classList.add("modal-open");
//    document.body.style.overflow = "hidden";

//    // Si ya hay backdrop, listo
//    const hasBackdrop = document.querySelectorAll(".modal-backdrop").length > 0;
//    if (hasBackdrop) return;

//    // Crear backdrop (igual al de Bootstrap)
//    const bd = document.createElement("div");
//    bd.className = "modal-backdrop fade show";
//    document.body.appendChild(bd);
//};

//window.bsModal.cleanup = () => {
//    try {

//        // Sacar foco activo (evita warning aria-hidden)
//        const active = document.activeElement;
//        if (active && active !== document.body && typeof active.blur === "function") active.blur();

//        const anyOpen = document.querySelectorAll(".modal.show").length > 0;

//        if (!anyOpen) {
//            // ✅ Caso normal: no hay modales → limpiar todo
//            document.querySelectorAll(".modal-backdrop, .offcanvas-backdrop").forEach(b => b.remove());
//            document.body.classList.remove("modal-open");
//            document.body.style.removeProperty("padding-right");
//            document.body.style.removeProperty("overflow");
//            document.documentElement.style.removeProperty("overflow");
//        } else {
//            // ✅ Hay modales abiertos → NO desbloquear fondo; reponer backdrop si falta
//            window.bsModal.ensureBackdropIfModalOpen();
//        }

//        document.body.style.pointerEvents = "auto";
//    } catch { }
//};

//window.bsModal.cleanupHard = () => {
//    window.bsModal.cleanup();
//    setTimeout(window.bsModal.cleanup, 200);
//    setTimeout(window.bsModal.cleanup, 1000);
//};

window.bsModal.cleanup = () => window.bsModal.resync();

window.bsModal.cleanupHard = () => {
    window.bsModal.resync();
    setTimeout(window.bsModal.resync, 200);
    setTimeout(window.bsModal.resync, 1000);
};







//window.addEventListener("pageshow", () => window.bsModal.cleanupHard());
//window.addEventListener("focus", () => window.bsModal.cleanupHard());
//document.addEventListener("visibilitychange", () => {
//    if (!document.hidden) window.bsModal.cleanupHard();
//});

window.addEventListener("pageshow", () => window.bsModal.cleanup());
document.addEventListener("visibilitychange", () => {
    if (!document.hidden) window.bsModal.cleanup();
});




window.hideReconnectOverlay = () => {
    const ov = document.getElementById("reconnectOverlay");
    if (ov) ov.style.display = "none";
};


window.bsModal.wireInert = (selector) => {
    const el = document.querySelector(selector);
    if (!el) return;

    // Arranca inert si está cerrado
    if (!el.classList.contains("show")) el.setAttribute("inert", "");

    el.addEventListener("shown.bs.modal", () => {
        el.removeAttribute("inert");
    });

    el.addEventListener("hidden.bs.modal", () => {
        el.setAttribute("inert", "");
    });
};


window.bsModal._wired = window.bsModal._wired || false;
window.bsModal._openCount = window.bsModal._openCount || 0;

//window.bsModal._ensureBodyBackdrop = () => {
//    const count = window.bsModal._openCount;

//    if (count > 0) {
//        document.body.classList.add("modal-open");
//        document.body.style.overflow = "hidden";

//        // si falta backdrop, crear
//        if (document.querySelectorAll(".modal-backdrop").length === 0) {
//            const bd = document.createElement("div");
//            bd.className = "modal-backdrop fade show";
//            document.body.appendChild(bd);
//        }
//        return;
//    }

//    // count == 0 -> limpiar
//    document.querySelectorAll(".modal-backdrop, .offcanvas-backdrop").forEach(b => b.remove());
//    document.body.classList.remove("modal-open");
//    document.body.style.removeProperty("padding-right");
//    document.body.style.removeProperty("overflow");
//    document.documentElement.style.removeProperty("overflow");
//    document.body.style.pointerEvents = "auto";
//};

window.bsModal._ensureBodyBackdrop = () => {
    // contar reales abiertos (mejor fuente)
    const open = document.querySelectorAll(".modal.show").length;

    if (open > 0) {
        // Si hay modales abiertos, NO crees backdrops.
        // A lo sumo asegurá overflow, pero yo ni eso tocaría:
        document.body.classList.add("modal-open");
        document.body.style.overflow = "hidden";
        return;
    }

    // open == 0 => limpiar basura huérfana
    document.querySelectorAll(".modal-backdrop, .offcanvas-backdrop").forEach(b => b.remove());
    document.body.classList.remove("modal-open");
    document.body.style.removeProperty("padding-right");
    document.body.style.removeProperty("overflow");
    document.documentElement.style.removeProperty("overflow");
    document.body.style.pointerEvents = "auto";
};


window.bsModal.resync = () => {
    // blur foco (aria-hidden warning)
    const active = document.activeElement;
    if (active && active !== document.body && typeof active.blur === "function") active.blur();

    // recalcular reales abiertos
    window.bsModal._openCount = document.querySelectorAll(".modal.show").length;
    window.bsModal._ensureBodyBackdrop();
};

//window.bsModal.wireModalManager = () => {
//    if (window.bsModal._wired) return;
//    window.bsModal._wired = true;

//    document.addEventListener("shown.bs.modal", () => {
//        window.bsModal._openCount++;
//        window.bsModal._ensureBodyBackdrop();
//    });

//    document.addEventListener("hidden.bs.modal", () => {
//        window.bsModal._openCount = Math.max(0, window.bsModal._openCount - 1);
//        setTimeout(() => window.bsModal._ensureBodyBackdrop(), 0);
//    });

//    // volver de visor externo / cambio de foco / cache de página
//    const run = () => window.bsModal.resync();
//    window.addEventListener("pageshow", run);
//    window.addEventListener("focus", run);
//    document.addEventListener("visibilitychange", () => { if (!document.hidden) run(); });
//};

window.bsModal.wireModalManager = () => {
    if (window.bsModal._wired) return;
    window.bsModal._wired = true;

    document.addEventListener("shown.bs.modal", () => window.bsModal.resync());
    document.addEventListener("hidden.bs.modal", () => setTimeout(() => window.bsModal.resync(), 0));

    // estos están bien, pero OJO con lo agresivo:
    const run = () => window.bsModal.resync();
    window.addEventListener("pageshow", run);
    window.addEventListener("focus", run);
    document.addEventListener("visibilitychange", () => { if (!document.hidden) run(); });
};




document.addEventListener("DOMContentLoaded", () => {
    window.bsModal.wireModalManager();
});
