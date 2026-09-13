// ✅ Mostrar notificaciones con Toastr
window.mostrarToast = (mensaje, tipo) => {
    if (typeof $ === "undefined") {
        console.error("❌ jQuery no está cargado. Toastr no funcionará.");
        alert(mensaje);
        return;
    }
    if (typeof toastr === "undefined") {
        console.error("❌ Toastr no está cargado.");
        alert(mensaje);
        return;
    }



    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        /*"positionClass": "toast-top-center",*/
        "positionClass": "toast-center-center",
        "timeOut": "4000",
        "extendedTimeOut": "2000",
        "preventDuplicates": true
    };

    switch (tipo) {
        case "success":
            toastr.success(mensaje);
            break;
        case "info":
            toastr.info(mensaje);
            break;
        case "warning":
            toastr.warning(mensaje);
            break;
        case "error":
            toastr.error(mensaje);
            break;
        default:
            toastr.info(mensaje); // fallback
            break;
    }
};

// ✅ Confirmación estilo modal (true/false)
window.mostrarConfirmacion = async (mensaje) => {
    return new Promise((resolve) => {
        resolve(window.confirm(mensaje));
    });
};

// ✅ Prompt simple (texto o null)
window.mostrarPrompt = async (mensaje, valorDefecto = "") => {
    return new Promise((resolve) => {
        const result = window.prompt(mensaje, valorDefecto);
        resolve(result);
    });
};

// ✅ Opción de guardar (actualizar/nuevo)
window.mostrarOpcionGuardar = async () => {
    return new Promise((resolve) => {
        const opcion = window.prompt(
            "¿Qué desea hacer?\n- Escriba 'actualizar' para actualizar\n- Escriba 'nuevo' para guardar como nuevo\n(Cancelar para salir)",
            ""
        );
        if (!opcion) resolve("cancelar");
        else resolve(opcion.toLowerCase());
    });
};






// ✅ Scroll al inicio de la página
window.scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
};



window.presupuesto = window.presupuesto || {};

window.presupuesto.getPrintHtml = function () {
    var el = document.getElementById('print-root');
    if (!el) {
        console.warn('presupuesto.getPrintHtml: #print-root no encontrado');
        return null;
    }
    return el.outerHTML;
};



// --- Protección cambios sin guardar ---

// Si ya existe, lo reutiliza; si no, lo crea
window.setUnsavedChanges = function (value) {
    // Creamos el objeto si no existe
    window.unsavedChanges = window.unsavedChanges || { enabled: false };
    window.unsavedChanges.enabled = !!value;
};

// Diálogo estándar del navegador al cerrar/recargar
window.addEventListener('beforeunload', function (e) {
    if (!window.unsavedChanges || !window.unsavedChanges.enabled) return;

    e.preventDefault();
    e.returnValue = '';
});
