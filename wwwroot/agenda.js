//window.renderAgendaCalendar = (eventos) => {
//    var calendarEl = document.getElementById('agendaCalendar');
//    if (!calendarEl) {
//        console.error("No se encontró el elemento #agendaCalendar");
//        return;
//    }

//    // Destruir calendario previo si existe
//    if (calendarEl._fullCalendar) {
//        calendarEl._fullCalendar.destroy();
//    }

//    var calendar = new FullCalendar.Calendar(calendarEl, {
//        locale: 'es',
//        initialView: 'dayGridMonth',
//        headerToolbar: {
//            left: 'prev,next today',
//            center: 'title',
//            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
//        },
//        events: eventos.map(e => ({
//            title: `${e.tipoAgendamientoNombre} - ${e.personaNombre}`,
//            start: e.fechaAgendada,
//            end: e.fechaAgendada, // si no tenés hora de fin
//            color: obtenerColorEstado(e.estadoAgenda)
//        })),
//        eventClick: function (info) {
//            alert(`Evento: ${info.event.title}`);
//        }
//    });

//    calendar.render();
//    calendarEl._fullCalendar = calendar;
//};

//function obtenerColorEstado(estado) {
//    switch (estado) {
//        case 0: return 'orange';   // Pendiente
//        case 1: return 'green';    // Realizado
//        case 2: return 'red';      // Cancelado
//        default: return 'gray';
//    }
//}

//window.renderAgendaCalendar = (eventos) => {
//    var calendarEl = document.getElementById('agendaCalendar');
//    if (!calendarEl) {
//        console.error("No se encontró el elemento #agendaCalendar");
//        return;
//    }

//    // Destruir previo
//    if (calendarEl._fullCalendar) {
//        calendarEl._fullCalendar.destroy();
//        calendarEl._fullCalendar = null;
//    }

//    var calendar = new FullCalendar.Calendar(calendarEl, {
//        locale: 'es',
//        initialView: 'dayGridMonth',
//        height: '100%',         // <- clave
//        expandRows: true,       // <- ayuda a no “aplastarse”
//        handleWindowResize: true,
//        headerToolbar: {
//            left: 'prev,next today',
//            center: 'title',
//            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
//        },
//        events: eventos.map(e => ({
//            title: `${e.tipoAgendamientoNombre} - ${e.personaNombre}`,
//            start: e.fechaAgendada,
//            end: e.fechaAgendada,
//            color: obtenerColorEstado(e.estadoAgenda)
//        })),
//        eventClick: function (info) {
//            alert(`Evento: ${info.event.title}`);
//        }
//    });

//    calendar.render();
//    calendarEl._fullCalendar = calendar;
//    window._agendaCalendar = calendar; // referencia global para updateSize
//};

//window.updateAgendaCalendarSize = () => {
//    if (window._agendaCalendar && typeof window._agendaCalendar.updateSize === 'function') {
//        window._agendaCalendar.updateSize();
//    }
//};

//function obtenerColorEstado(estado) {
//    switch (estado) {
//        case 0: return 'orange';
//        case 1: return 'green';
//        case 2: return 'red';
//        default: return 'gray';
//    }
//}


function obtenerColorEstado(estado) {
    switch (estado) {
        case 1: return '#dc3545'; // Pendiente - rojo (Bootstrap danger)
        case 2: return '#198754'; // Realizado - verde (success)
        case 3: return '#6c757d'; // Cancelado - gris (secondary)
        case 4: return '#fd7e14'; // Reprogramado - naranja (warning)
        default: return '#343a40'; // Desconocido - dark
    }
}

//window.renderAgendaCalendar = (eventos) => {
//    const el = document.getElementById('agendaCalendar');
//    if (!el) return;

//    // destruir previo
//    if (el._fullCalendar) { el._fullCalendar.destroy(); el._fullCalendar = null; }

//    const calendar = new FullCalendar.Calendar(el, {
//        locale: 'es',
//        // v5 main.min.js ya trae dayGrid/timeGrid/list/interaction integrados
//        initialView: 'dayGridMonth',
//        headerToolbar: {
//            left: 'prev,next today',
//            center: 'title',
//            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
//        },
//        height: '100%',        // clave para ocupar el contenedor
//        expandRows: true,
//        handleWindowResize: true,
//        slotMinTime: '07:00:00',
//        slotMaxTime: '21:00:00',

//        events: eventos.map(e => {
//            // si viene solo fecha sin hora, forzá una hora para las vistas por día/semana
//            const hasTime = typeof e.fechaAgendada === 'string' && e.fechaAgendada.includes('T');
//            const start = hasTime
//                ? e.fechaAgendada
//                : (e.fechaAgendada ? (e.fechaAgendada + 'T09:00:00') : null);

//            return {
//                title: `${e.tipoAgendamientoNombre ?? ''}${e.personaNombre ? ' - ' + e.personaNombre : ''}${e.Observaciones ?? ''}`,
//                start,
//                allDay: !hasTime,
//                color: obtenerColorEstado(e.estadoAgenda)
//            };
//        })
//    });

//    calendar.render();
//    el._fullCalendar = calendar;
//    window._agendaCalendar = calendar; // para updateSize() en shown.bs.modal
//};









function nombreEstado(num) {
    switch (num) {
        case 1: return 'pendiente';
        case 2: return 'realizado';
        case 3: return 'cancelado';
        case 4: return 'reprogramado';
        default: return 'desconocido';
    }
}

function setModalHeaderEstadoFromEvent(evt) {
    const header = document.getElementById('modalEventoHeader');
    if (!header) return;
    const num = evt?.extendedProps?.estadoAgenda;
    header.setAttribute('data-estado', nombreEstado(num));
}





















window.renderAgendaCalendar = (eventos, dotnetHelper) => {
    const el = document.getElementById('agendaCalendar');
    if (!el) return;

    if (el._fullCalendar) { el._fullCalendar.destroy(); el._fullCalendar = null; }

    const calendar = new FullCalendar.Calendar(el, {
        locale: 'es',
        initialView: 'dayGridMonth',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
        },
        

        // ✴ Muy importante para grilla prolija
        //height: '100%',         // usa todo el alto del wrapper
        //contentHeight: 'auto',  // deja que reparta las filas
        //expandRows: true,       // filas se expanden para llenar el alto
        fixedWeekCount: true,   // siempre 6 filas

        // 
        dayMaxEvents: 2,        // hasta 3 eventos visibles
        dayMaxEventRows: false, // que no limite por filas, solo por cantidad
        moreLinkClick: 'popover',



        handleWindowResize: true,
        slotMinTime: '07:00:00',
        slotMaxTime: '21:00:00',

  

      

        //  Evita arrastrar, redimensionar o seleccionar
        editable: false,
        eventStartEditable: false,
        eventDurationEditable: false,
        selectable: false,

        //events: eventos.map(e => {
        //    const hasTime = typeof e.fechaAgendada === 'string' && e.fechaAgendada.includes('T');
        //    const start = hasTime ? e.fechaAgendada : (e.fechaAgendada ? (e.fechaAgendada + 'T09:00:00') : null);
        //    return {
        //        id: e.id,
        //        title: `${e.tipoAgendamientoNombre ?? ''}${e.personaNombre ? ' - ' + e.personaNombre : ''}${e.observaciones ? ' - ' + e.observaciones : ''}`,
        //        start,
        //        allDay: !hasTime,
        //        color: obtenerColorEstado(e.estadoAgenda),
        //        extendedProps: {
        //            estadoAgenda: e.estadoAgenda,               // 👈 acá viaja el enum (1..4)
        //            descripcion: e.observaciones ?? ''          // opcional
        //        }
        //    };
        //}),

        events: eventos.map(e => {
            const hasTime = typeof e.fechaAgendada === 'string' && e.fechaAgendada.includes('T');
            const start = hasTime ? e.fechaAgendada : (e.fechaAgendada ? (e.fechaAgendada + 'T09:00:00') : null);

            const tituloBase = e.tipoAgendamientoNombre ?? '';
            const persona = e.personaNombre ? ' - ' + e.personaNombre : '';

            return {
                id: e.id,
                title: `${tituloBase}${persona}`.trim(), // 👈 sin observaciones
                start,
                allDay: !hasTime,
                color: obtenerColorEstado(e.estadoAgenda),
                extendedProps: {
                    estadoAgenda: e.estadoAgenda,
                    descripcion: e.observaciones ?? ''  // 👈 sólo acá
                }
            };
        }),


        dateClick: (info) => {
            console.log('dateClick', info.dateStr);
            dotnetHelper?.invokeMethodAsync('OnDiaClick', info.dateStr).catch(console.error);
        },

        eventClick: (info) => {
            setModalHeaderEstadoFromEvent(info.event); 
            const id = info.event.id ? parseInt(info.event.id, 10) : null;
            if (id) {
                dotnetHelper?.invokeMethodAsync('OnEventoClick', id).catch(console.error);
            }
        },

        eventDidMount: (info) => {
            info.el.style.cursor = 'pointer';
            // por si algún hijo pisa el cursor
            info.el.querySelectorAll('*').forEach(n => n.style.cursor = 'pointer');
        }

    });

    calendar.render();
    el._fullCalendar = calendar;
    window._agendaCalendar = calendar;
};







// Fuerza a dayGrid a recalcular el grid: saltamos a otra vista y volvemos
window.forceCalendarRelayout = () => {
    const cal = window._agendaCalendar;
    if (!cal) return;
    const current = cal.view.type;
    cal.changeView('timeGridDay');
    cal.updateSize && cal.updateSize();
    cal.changeView(current);
    cal.updateSize && cal.updateSize();
};


window.addEventListener('orientationchange', () => {
    setTimeout(() => {
        if (window.forceCalendarRelayout) window.forceCalendarRelayout();
    }, 200);
});




 /*Hacer que un modal sea arrastrable*/
//window.makeModalDraggable = (modalId) => {
//    const modal = document.getElementById(modalId);
//    if (!modal) return;

//    const dialog = modal.querySelector(".modal-dialog");
//    const header = modal.querySelector(".modal-header");

//    if (!dialog || !header) return;

//    let isDragging = false;
//    let startX, startY, origX, origY;

//    header.style.cursor = "move";
//    header.addEventListener("mousedown", (e) => {
//        isDragging = true;
//        startX = e.clientX;
//        startY = e.clientY;

//        // posición actual
//        const rect = dialog.getBoundingClientRect();
//        origX = rect.left;
//        origY = rect.top;

//        document.addEventListener("mousemove", onMouseMove);
//        document.addEventListener("mouseup", onMouseUp);
//    });

//    const onMouseMove = (e) => {
//        if (!isDragging) return;
//        const dx = e.clientX - startX;
//        const dy = e.clientY - startY;
//        dialog.style.transform = `translate(${dx}px, ${dy}px)`;
//        dialog.style.margin = "0"; // evitar que Bootstrap lo centre de nuevo
//        dialog.style.position = "absolute";
//        dialog.style.left = `${origX}px`;
//        dialog.style.top = `${origY}px`;
//    };

//    const onMouseUp = () => {
//        isDragging = false;
//        document.removeEventListener("mousemove", onMouseMove);
//        document.removeEventListener("mouseup", onMouseUp);
//    };
//};




// Arrastrar solo desde el header y limitar al viewport
window.makeModalDraggable = (modalId, padding = 8) => {
    const modal = document.getElementById(modalId);
    if (!modal) return;

    const dialog = modal.querySelector(".modal-dialog");
    const header = modal.querySelector(".modal-header");
    if (!dialog || !header) return;

    let isDragging = false;
    let startX = 0, startY = 0, origLeft = 0, origTop = 0;

    // Fijar posición y anular centrado cuando se empiece a arrastrar
    const ensureFixedPosition = () => {
        const rect = dialog.getBoundingClientRect();
        dialog.style.position = "fixed"; // relativo al viewport (mejor que absolute)
        dialog.style.margin = "0";       // evita recentrado de Bootstrap
        dialog.style.left = rect.left + "px";
        dialog.style.top = rect.top + "px";
        dialog.style.transform = "none";
    };

    const clampToViewport = (left, top) => {
        const w = dialog.offsetWidth;
        const h = dialog.offsetHeight;
        const maxLeft = window.innerWidth - w - padding;
        const maxTop = window.innerHeight - h - padding;
        return {
            left: Math.min(Math.max(left, padding), Math.max(padding, maxLeft)),
            top: Math.min(Math.max(top, padding), Math.max(padding, maxTop))
        };
    };

    const onMouseMove = (e) => {
        if (!isDragging) return;
        const dx = e.clientX - startX;
        const dy = e.clientY - startY;

        const { left, top } = clampToViewport(origLeft + dx, origTop + dy);
        dialog.style.left = left + "px";
        dialog.style.top = top + "px";
    };

    const onMouseUp = () => {
        if (!isDragging) return;
        isDragging = false;
        document.removeEventListener("mousemove", onMouseMove);
        document.removeEventListener("mouseup", onMouseUp);
        document.body.classList.remove("no-select");
    };

    header.style.cursor = "move";
    header.addEventListener("mousedown", (e) => {
        // No iniciar drag si clickeás el botón cerrar u otro control del header
        if (e.target.closest(".btn,button,[data-bs-dismiss]")) return;

        ensureFixedPosition();

        const rect = dialog.getBoundingClientRect();
        isDragging = true;
        startX = e.clientX;
        startY = e.clientY;
        origLeft = rect.left;
        origTop = rect.top;

        document.body.classList.add("no-select");
        document.addEventListener("mousemove", onMouseMove);
        document.addEventListener("mouseup", onMouseUp);

        e.preventDefault();
    });

    // Mantener dentro al redimensionar la ventana
    window.addEventListener("resize", () => {
        if (!dialog.style.position) return;
        const rect = dialog.getBoundingClientRect();
        const { left, top } = clampToViewport(rect.left, rect.top);
        dialog.style.left = left + "px";
        dialog.style.top = top + "px";
    });
};






