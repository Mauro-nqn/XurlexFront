//window.winInterop = (function () {
//    let dotnet;
//    function onMove(e) { dotnet?.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY); }
//    function onUp() { dotnet?.invokeMethodAsync('OnMouseUp'); }
//    return {
//        registerMouse: function (dotnetRef) {
//            dotnet = dotnetRef;
//            window.addEventListener('mousemove', onMove);
//            window.addEventListener('mouseup', onUp);
//        },
//        getWindowSize: function () {
//            return [window.innerWidth, window.innerHeight];
//        }
//    };
//})();

// wwwroot/js/floatingWindows.js






//(function () {
//    let active = null;   // ventana activa (recibe mousemove/up)
//    let attached = false;

//    function onMove(e) { if (active) active.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY); }
//    function onUp() { if (active) { active.invokeMethodAsync('OnMouseUp'); active = null; } }

//    window.winInterop = {
//        init: function () {
//            if (attached) return;
//            attached = true;
//            window.addEventListener('mousemove', onMove);
//            window.addEventListener('mouseup', onUp);
//        },
//        setActive: function (dotnetRef) { active = dotnetRef; },
//        getWindowSize: function () { return [window.innerWidth, window.innerHeight]; }
//    };
//})();








/*VERSION FUNCIONAL */


//(function () {
//    let activeWindows = new Map(); // Usamos un mapa para almacenar las referencias por su GUID
//    let attached = false;

//    // Funciones de movimiento para el mouse
//    function onMove(e) {
//        // Itera sobre todas las referencias activas y llama a su método OnMouseMove
//        activeWindows.forEach(dotnetRef => {
//            dotnetRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY);
//        });
//    }

//    // Funciones de movimiento para el toque (touch)

//    //function onTouchMove(e) {
//    //    // Prevén el comportamiento por defecto (ej. scroll de la página)
//    //    e.preventDefault();
//    //    e.stopPropagation(); // ¡Nuevo! Detiene la propagación del evento
//    //    if (e.touches.length > 0) {
//    //        activeWindows.forEach(dotnetRef => {
//    //            dotnetRef.invokeMethodAsync('OnMouseMove', e.touches[0].clientX, e.touches[0].clientY);
//    //        });
//    //    }
//    //}

//    function onUp(e) {
//        // Llama a OnMouseUp en todas las referencias activas
//        activeWindows.forEach(dotnetRef => {
//            dotnetRef.invokeMethodAsync('OnMouseUp');
//        });
//        // Limpiamos el mapa cuando se suelta el mouse para evitar efectos secundarios
//        activeWindows.clear();
//    }


//    //function onTouchEnd(e) {
//    //    activeWindows.forEach(dotnetRef => {
//    //        dotnetRef.invokeMethodAsync('OnMouseUp');
//    //    });
//    //    activeWindows.clear();
//    //}

//    window.winInterop = {
//        init: function () {
//            if (attached) return;
//            attached = true;

//            //Eventos para mouse
//            window.addEventListener('mousemove', onMove);
//            window.addEventListener('mouseup', onUp);

//            // Eventos para Touch
//            //window.addEventListener('touchmove', onTouchMove, { passive: false });
//            //window.addEventListener('touchend', onTouchEnd);
//        },
//        // setActive ahora recibe el ID y la referencia del objeto DotNet
//        setActive: function (id, dotnetRef) {
//            // Limpia el mapa de referencias activas antes de agregar la nueva.
//            // Esto asegura que solo una ventana se esté arrastrando o redimensionando a la vez.
//            activeWindows.clear();
//            activeWindows.set(id, dotnetRef);
//        },
//        // Añadimos un método para remover la referencia de una ventana cerrada
//        removeActive: function (id) {
//            activeWindows.delete(id);
//        },
//        getWindowSize: function () {
//            return [window.innerWidth, window.innerHeight];
//        }
//    };
//})();



// winInterop.js – STUB para convivir con Interact.js
// wwwroot/js/winInterop.js
(function () {
    let activeRef = null;

    function onMove(e) {
        if (!activeRef) return;
        try { activeRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY); } catch { }
    }
    function onUp() {
        if (!activeRef) return;
        try { activeRef.invokeMethodAsync('OnMouseUp'); } catch { }
        window.removeEventListener('mousemove', onMove, { capture: false });
        window.removeEventListener('mouseup', onUp, { capture: false });
        activeRef = null;
    }

    window.winInterop = {
        init: function () { }, // no-op

        // usado solo por StartResize (mouse)
        setActive: function (_id, dotnetRef) {
            if (activeRef) onUp();
            activeRef = dotnetRef;
            window.addEventListener('mousemove', onMove, { passive: true });
            window.addEventListener('mouseup', onUp, { passive: true });
        },

        removeActive: function () { onUp(); },

        getWindowSize: function () {
            return [window.innerWidth, window.innerHeight];
        }
    };
})();











