// Inicializar editor
window.initEditor = (editorId = "editor1", base64Content) => {
    if (typeof CKEDITOR === "undefined") {
        console.error("❌ CKEditor no se cargó.");
        return;
    }


    // Destruir instancia previa si existe
    if (CKEDITOR.instances[editorId]) {
        CKEDITOR.instances[editorId].destroy(true);
    }

    const textarea = document.getElementById(editorId);
    if (!textarea) {
        console.warn(`⚠️ No se encontró el textarea con ID '${editorId}'`);
        return;
    }

    const editor = CKEDITOR.replace(editorId, {
        language: 'es',
        removePlugins: 'exportpdf,image, save',  // sin imágenes en el modal        
        versionCheck: false,
        height: 400,// más bajo que tu editor “full”        
        contentsCss: '/editor/ckeditor/contents.css',
        toolbar: [
            { name: 'document', items: ['Source', 'Preview', 'Print'] },
            { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'Undo', 'Redo'] },
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'RemoveFormat'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', 'JustifyLeft', 'JustifyCenter', 'JustifyRight'] },
            { name: 'insert', items: ['Image', 'Table', 'HorizontalRule', 'SpecialChar'] },
            { name: 'align', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
            { name: 'links', items: ['Link', 'Unlink'] },
            { name: 'styles', items: ['Format'] }, // títulos párrafo/h4/h5
            { name: 'colors', items: ['TextColor', 'BGColor'] },
            { name: 'tools', items: ['Maximize'] },



        ],
        // Podés agregar 'autogrow' si lo tenés disponible:
        // extraPlugins: 'autogrow',
        // autoGrow_minHeight: 180,
        // autoGrow_maxHeight: 360,
        // autoGrow_onStartup: true
    });

    //  Hook de cambios: cada tecla / cambio marca dirty + dispara autosave (si está bind)
    editor.on('change', function () {
        try {
            if (window.__editorOnChange && window.__editorOnChange[editorId]) {
                window.__editorOnChange[editorId]();
            }
        } catch (e) {
            console.warn("No se pudo ejecutar __editorOnChange:", e);
        }
    });



    editor.on('instanceReady', () => {
        console.log("✅ CKEditor listo.");
        //if (base64Content) {
        //    const decodedHtml = decodeURIComponent(escape(window.atob(base64Content)));
        //    editor.setData(decodedHtml);
        //    console.log("📥 Contenido inicial cargado en CKEditor.");
        //}
        if (base64Content) {
            try {
                const decodedHtml = atob(base64Content); // ✅ más seguro y directo
                editor.setData(decodedHtml);
                console.log("📥 Contenido inicial cargado en CKEditor.");
            } catch (e) {
                console.error("❌ Error al decodificar base64:", e);
            }
        }
    });
};

// Setear contenido (desde base64)
//window.setEditorContentFromBase64 = (editorId = "editor1", base64Content) => {
//    if (CKEDITOR.instances[editorId]) {
//        const decodedHtml = decodeURIComponent(escape(window.atob(base64Content)));
//        CKEDITOR.instances[editorId].setData(decodedHtml);
//    }
//};

window.setEditorContentFromBase64 = (editorId = "editor1", base64Content) => {
    if (CKEDITOR.instances[editorId]) {
        const decodedHtml = atob(base64Content); // ✅ más simple y seguro
        CKEDITOR.instances[editorId].setData(decodedHtml);
    }
};


// Obtener contenido como HTML puro
window.getEditorContent = (editorId = "editor1") => {
    if (CKEDITOR.instances[editorId]) {
        return CKEDITOR.instances[editorId].getData();
    }
    return "";
};

// Destruir editor
window.destroyEditor = (editorId = "editor1") => {
    if (CKEDITOR.instances[editorId]) {
        CKEDITOR.instances[editorId].destroy(true);
        console.log("🗑️ CKEditor destruido correctamente.");
    }
};

 /*Modal: elegir opción de guardado*/
window.mostrarOpcionGuardar = async () => {
    return new Promise((resolve) => {
        const modal = document.createElement('div');
        modal.classList.add('custom-modal');
        modal.innerHTML = `
            <div class="custom-modal-content">
                <h5>¿Qué desea hacer?</h5>
                <button id="actualizarBtn">Actualizar escrito</button>
                <button id="nuevoBtn">Guardar como nuevo</button>
                <button id="cancelarBtn">Cancelar</button>
            </div>
        `;
        document.body.appendChild(modal);

        modal.querySelector('#actualizarBtn').onclick = () => { modal.remove(); resolve("actualizar"); };
        modal.querySelector('#nuevoBtn').onclick = () => { modal.remove(); resolve("nuevo"); };
        modal.querySelector('#cancelarBtn').onclick = () => { modal.remove(); resolve("cancelar"); };
    });
};

// Toast centrado en pantalla
window.mostrarToast = (mensaje, tipo) => {
    const toast = document.createElement("div");
    toast.className = `toast-message ${tipo}`;
    toast.innerText = mensaje;
    document.body.appendChild(toast);

    setTimeout(() => toast.classList.add("show"), 10);
    setTimeout(() => {
        toast.classList.remove("show");
        setTimeout(() => toast.remove(), 500);
    }, 3000);
};





//Version mini para editor mini en presupuesto

// Editor minimal para modal (dos instancias separadas)
window.initMiniEditor = (editorId, base64Content) => {
    if (typeof CKEDITOR === "undefined") {
        console.error("❌ CKEditor no se cargó.");
        return;
    }
    if (!editorId) editorId = "editor1";

    // Destruir si ya existe
    if (CKEDITOR.instances[editorId]) {
        CKEDITOR.instances[editorId].destroy(true);
    }

    const el = document.getElementById(editorId);
    if (!el) {
        console.warn(`⚠️ No se encontró el textarea con ID '${editorId}'`);
        return;
    }

    const editor = CKEDITOR.replace(editorId, {
        language: 'es',
        removePlugins: 'exportpdf,image',   // sin imágenes en el modal
        height: 220,                        // más bajo que tu editor “full”
        contentsCss: '/editor/ckeditor/contents.css',
        toolbar: [
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline', 'RemoveFormat'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent'] },
            { name: 'align', items: ['JustifyLeft', 'JustifyCenter', 'JustifyRight', 'JustifyBlock'] },
            { name: 'links', items: ['Link', 'Unlink'] },
            { name: 'styles', items: ['Format'] }, // títulos párrafo/h4/h5
            { name: 'tools', items: ['Maximize'] }
        ],
        // Podés agregar 'autogrow' si lo tenés disponible:
        // extraPlugins: 'autogrow',
        // autoGrow_minHeight: 180,
        // autoGrow_maxHeight: 360,
        // autoGrow_onStartup: true
    });

    editor.on('instanceReady', () => {
        if (base64Content) {
            try {
                const decodedHtml = atob(base64Content);
                editor.setData(decodedHtml);
            } catch (e) {
                console.error("❌ Error al decodificar base64:", e);
            }
        }
    });
};

// Obtener contenido como HTML
window.getEditorContent = (editorId) => {
    if (CKEDITOR.instances[editorId]) {
        return CKEDITOR.instances[editorId].getData();
    }
    return "";
};

// Destruir instancia
window.destroyEditor = (editorId) => {
    if (CKEDITOR.instances[editorId]) {
        CKEDITOR.instances[editorId].destroy(true);
    }
};



window.setEditorContentFromText = function (editorId, text) {
    // Si estás usando CKEditor 4
    if (window.CKEDITOR && CKEDITOR.instances[editorId]) {
        CKEDITOR.instances[editorId].setData(text.replace(/\r\n|\n/g, "<br>"));
        return;
    }

    // Si usás CKEditor 5 y guardaste la instancia en window._editors[editorId]
    if (window._editors && window._editors[editorId]) {
        window._editors[editorId].setData(text);
        return;
    }

    console.warn("No se encontró instancia de editor para", editorId);
};



window.editorDirtyConfirm = async function () {
    // Podés reemplazar por un modal propio si querés.
    // Acá hacemos una confirmación en 2 pasos simple.
    const r = confirm("Tenés cambios sin guardar.\n\nAceptar = Guardar y salir\nCancelar = Ver más opciones");
    if (r) return "save";

    const r2 = confirm("¿Querés salir SIN guardar?\nAceptar = Salir sin guardar\nCancelar = Quedarme");
    return r2 ? "discard" : "stay";
};

//window.editorDraft = {
//    get: (key) => localStorage.getItem(key) || "",
//    set: (key, html) => localStorage.setItem(key, html),
//    clear: (key) => localStorage.removeItem(key),

//    bindAutosave: (editorId, key, dotnetRef) => {
//        // Cada cuánto guardar (ms)
//        const interval = 2000;

//        let t = null;

//        async function snapshot() {
//            try {
//                const html = window.getEditorContent(editorId);
//                if (html && html.trim().length > 0) {
//                    localStorage.setItem(key, html);
//                }
//            } catch { }
//        }

//        // Hook cambio editor: marca dirty + programa autosave (debounce)
//        window.__editorOnChange = window.__editorOnChange || {};
//        window.__editorOnChange[editorId] = async () => {
//            try { await dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }
//            clearTimeout(t);
//            t = setTimeout(snapshot, interval);
//        };

//        // 1) Guardar cuando el usuario vuelve/oculta (móviles)
//        document.addEventListener("visibilitychange", () => {
//            if (document.visibilityState === "hidden") snapshot();
//        });

//        // 2) Antes de recargar/cerrar pestaña (no siempre dispara en mobile, pero ayuda)
//        window.addEventListener("beforeunload", () => {
//            snapshot();
//        });
//    }
//};


//window.editorDraft = window.editorDraft || {
//    get: (key) => localStorage.getItem(key) || "",
//    set: (key, html) => localStorage.setItem(key, html),
//    clear: (key) => localStorage.removeItem(key),

//    bindAutosave: (editorId, key, dotnetRef) => {
//        const interval = 2000;
//        let t = null;

//        async function snapshot() {
//            try {
//                const html = window.getEditorContent(editorId);
//                if (html && html.trim().length > 0) {
//                    localStorage.setItem(key, html);
//                }
//            } catch { }
//        }

//        window.__editorOnChange = window.__editorOnChange || {};
//        window.__editorOnChange[editorId] = async () => {
//            try { await dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }
//            clearTimeout(t);
//            t = setTimeout(snapshot, interval);
//        };

//        document.addEventListener("visibilitychange", () => {
//            if (document.visibilityState === "hidden") snapshot();
//        });

//        window.addEventListener("beforeunload", () => snapshot());
//    }
//};



//window.editorDirty = window.editorDirty || {};
//window.editorDirty._unbind = window.editorDirty._unbind || (() => { });
//window.unbindEditorDirty = (editorId) => window.editorDirty._unbind?.(editorId);


window.editorDraft = window.editorDraft || {
    get: (key) => localStorage.getItem(key) || "",
    set: (key, html) => localStorage.setItem(key, html),
    clear: (key) => localStorage.removeItem(key),

    _state: {},

    bindAutosave: (editorId, key, dotnetRef) => {
        const interval = 2000;

        // ✅ si ya estaba, unbind primero
        if (window.editorDraft._state[editorId]) {
            window.editorDraft.unbindAutosave(editorId);
        }

        let t = null;

        function snapshot() {
            try {
                const html = window.getEditorContent(editorId);
                if (html && html.trim().length > 0) localStorage.setItem(key, html);
            } catch { }
        }

        window.__editorOnChange = window.__editorOnChange || {};
        window.__editorOnChange[editorId] = async () => {
            try { await dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }
            clearTimeout(t);
            t = setTimeout(snapshot, interval);
        };

        const onVisibility = () => {
            if (document.visibilityState === "hidden") snapshot();
        };
        const onBeforeUnload = () => snapshot();

        document.addEventListener("visibilitychange", onVisibility);
        window.addEventListener("beforeunload", onBeforeUnload);

        window.editorDraft._state[editorId] = { onVisibility, onBeforeUnload, clear: () => clearTimeout(t) };
    },

    unbindAutosave: (editorId) => {
        const st = window.editorDraft._state?.[editorId];
        if (!st) return;

        try { st.clear(); } catch { }
        try { document.removeEventListener("visibilitychange", st.onVisibility); } catch { }
        try { window.removeEventListener("beforeunload", st.onBeforeUnload); } catch { }

        try {
            if (window.__editorOnChange && window.__editorOnChange[editorId]) {
                delete window.__editorOnChange[editorId];
            }
        } catch { }

        delete window.editorDraft._state[editorId];
    }
};


window.editorDirty = window.editorDirty || {};
window.editorDirty._unbind = window.editorDirty._unbind || (() => { });
window.unbindEditorDirty = (editorId) => window.editorDirty._unbind?.(editorId);

