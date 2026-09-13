//window.mostrarModalAgenda = () => {
//    const modal = document.getElementById("modalAgenda");
//    if (modal) {
//        // Cambiar el estilo de fondo
//        modal.querySelector('.modal-content').style.backgroundColor = '#f4f4f4';
//        modal.querySelector('.modal-header').style.backgroundColor = '#007bff';

//        // Cambiar el tamaño del modal
//        const modalDialog = modal.querySelector('.modal-dialog');
//        modalDialog.style.maxWidth = '60%';
//        modalDialog.style.width = 'auto';
//        modalDialog.style.height = '80vh';
//        modalDialog.style.margin = '30px auto';

//        // Retraso de 100ms para asegurar que el navegador procese los estilos
//        setTimeout(() => {
//            const bootstrapModal = new bootstrap.Modal(modal);
//            bootstrapModal.show();
//        }, 400);  // Este pequeño retraso puede ayudar a que el modal se ajuste correctamente.
//    } else {
//        console.error("❌ No se encontró el modal con id 'modalAgenda'");
//    }
//};



//window.mostrarModalAgenda = () => {
//    const modal = document.getElementById("modalAgenda");
//    if (!modal) {
//        console.error("❌ No se encontró el modal con id 'modalAgenda'");
//        return;
//    }

//    // Opcional: colores
//    // modal.querySelector('.modal-content').style.backgroundColor = '#f4f4f4';
//    // modal.querySelector('.modal-header').style.backgroundColor = '#007bff';

//    // NO achiques el modal acá; dejamos que el CSS lo maneje

//    // Hook para cuando el modal ya está visible
//    modal.addEventListener('shown.bs.modal', function onShown() {
//        // actualizar tamaño para que FullCalendar se expanda
//        setTimeout(() => { window.updateAgendaCalendarSize && window.updateAgendaCalendarSize(); }, 0);
//        // quitar el listener si no querés duplicarlo
//        modal.removeEventListener('shown.bs.modal', onShown);
//    });

//    const bootstrapModal = new bootstrap.Modal(modal, { backdrop: 'static' });
//    bootstrapModal.show();
//};




//window.mostrarModalAgenda = () => {
//    const modal = document.getElementById("modalAgenda");
//    if (!modal) return;

//    const onShown = () => {
//        // Reintenta varias veces hasta que el modal termine el fade y el layout esté estable
//        window.ensureAgendaCalendarSized(40, 50); // ~2 segundos máx de reintentos

//        // por si acaso, un resize global (equivale a abrir DevTools)
//        setTimeout(() => window.dispatchEvent(new Event('resize')), 0);

//        modal.removeEventListener('shown.bs.modal', onShown);
//    };
//    modal.addEventListener('shown.bs.modal', onShown);

//    const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
//    bsModal.show();
//};

//window.mostrarModalAgenda = () => {
//    const modal = document.getElementById("modalAgenda");
//    if (!modal) return;

//    const onShown = () => {
//        // Reintenta varias veces hasta que el modal termine el fade y el layout esté estable
//        window.ensureAgendaCalendarSized(40, 50); // ~2 segundos máx de reintentos

//        // Por si acaso, un resize global (equivale a abrir DevTools)
//        setTimeout(() => {
//            window.dispatchEvent(new Event('resize'));
//            // 🧩 Relayout extra para forzar recalculo del grid del mes
//            if (window.forceCalendarRelayout) {
//                window.forceCalendarRelayout();
//            }
//        }, 200); // pequeño delay para que el DOM esté listo

//        modal.removeEventListener('shown.bs.modal', onShown, { once: true });
//    };
//    modal.addEventListener('shown.bs.modal', onShown);

//    const bsModal = new bootstrap.Modal(modal, { backdrop: 'static' });
//    bsModal.show();
//};


//window.mostrarModalAgenda = () => {
//    const modalId = "modalAgenda";
//    const modal = document.getElementById(modalId);
//    if (!modal) return;

//    modal.addEventListener('shown.bs.modal', () => {
//        window.ensureAgendaCalendarSized(40, 50);

//        setTimeout(() => {
//            window.dispatchEvent(new Event('resize'));
//            if (window.forceCalendarRelayout) {
//                window.forceCalendarRelayout();
//            }
//        }, 200);

//        window.makeModalDraggable('modalAgenda'); // ✅ habilitar arrastre
//    }, { once: true });

//    new bootstrap.Modal(modal, { backdrop: 'static' }).show();
//};


window.mostrarModalAgenda = () => {
    const modal = document.getElementById("modalAgenda");
    if (!modal || !window.bootstrap?.Modal) return;

    // ✅ 1) Asegurar que el modal viva bajo <body> (fuera del árbol que Blazor re-renderiza)
    if (modal.parentElement !== document.body) {
        document.body.appendChild(modal);
    }

    // ✅ 2) Hook 1 sola vez (sin once)
    if (!modal.dataset.agendaHooked) {
        modal.dataset.agendaHooked = "1";

        modal.addEventListener("shown.bs.modal", () => {
            window.ensureAgendaCalendarSized?.(40, 50);
            setTimeout(() => {
                window.dispatchEvent(new Event("resize"));
                window.forceCalendarRelayout?.();
            }, 200);
            window.makeModalDraggable?.("modalAgenda");
        });

        // 🔥 si por un re-render se “rompe” el backdrop después de abrir, lo repara
        modal.addEventListener("shown.bs.modal", () => {
            window.fixBackdropIfMissing?.("modalAgenda");
        });
    }

    const instance = bootstrap.Modal.getOrCreateInstance(modal, {
        backdrop: "static",
        keyboard: false,
        focus: true
    });

    instance.show();

    // ✅ 3) Reparación inmediata (por carrera)
    setTimeout(() => window.fixBackdropIfMissing?.("modalAgenda"), 60);
    setTimeout(() => window.fixBackdropIfMissing?.("modalAgenda"), 180);
};

window.fixBackdropIfMissing = (modalId) => {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    const isShown = modal.classList.contains("show");
    if (!isShown) return;

    const hasBackdrop = !!document.querySelector(".modal-backdrop");
    const bodyOk = document.body.classList.contains("modal-open");

    if (hasBackdrop && bodyOk) return;

    // ✅ fuerza estado correcto
    document.body.classList.add("modal-open");

    // ✅ crea backdrop si falta
    if (!hasBackdrop) {
        const bd = document.createElement("div");
        bd.className = "modal-backdrop fade show";
        document.body.appendChild(bd);
    }
};









// Reintenta hasta que #agendaCalendar tenga layout real y luego fuerza recalculo
window.ensureAgendaCalendarSized = (retries = 40, intervalMs = 50) => {
    const el = document.getElementById('agendaCalendar');
    const cal = el && el._fullCalendar;
    if (!el || !cal) return; // aún no existe o no está renderizado

    const hasLayout = el.offsetWidth > 0 && el.offsetHeight > 0;
    if (hasLayout) {
        cal.updateSize && cal.updateSize();
        // segundo empujón por si el fade de Bootstrap sigue
        requestAnimationFrame(() => cal.updateSize && cal.updateSize());
        return;
    }

    if (retries <= 0) return;
    setTimeout(() => window.ensureAgendaCalendarSized(retries - 1, intervalMs), intervalMs);
};











window.mostrarModalEventosDia = () => {
    const modal = document.getElementById("modalEventosDia");
    if (modal) {
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
    } else {
        console.error("❌ No se encontró el modal de eventos del día");
    }
};


window.cerrarModalAgenda = () => {
    const modal = document.getElementById("modalAgenda");
    if (!modal) return;
    (bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal)).hide();
};


window.setEventoModal = (titulo, fecha, descripcionHtml) => {
    document.getElementById('eventoTitulo').innerText = titulo || '';
    document.getElementById('eventoFecha').innerText = fecha || '';
    document.getElementById('eventoDescripcion').innerHTML = descripcionHtml || '';
};

//window.mostrarModalEventosDia = () => {
//    const modal = document.getElementById("modalEventosDia");
//    if (modal) new bootstrap.Modal(modal).show();
//};


