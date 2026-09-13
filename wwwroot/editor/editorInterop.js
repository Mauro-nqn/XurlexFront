// /wwwroot/js/editorInterop.js

// --- helpers ---
function waitFor(cond, timeout = 5000, interval = 50) {
    return new Promise((resolve, reject) => {
        const start = Date.now();
        const t = setInterval(() => {
            if (cond()) { clearInterval(t); resolve(); }
            else if (Date.now() - start > timeout) {
                clearInterval(t); reject(new Error("waitFor timeout"));
            }
        }, interval);
    });
}

async function loadCkeditor(url = "/editor/ckeditor/ckeditor.js") {
    if (window.CKEDITOR) return;
    await new Promise((resolve, reject) => {
        const s = document.createElement("script");
        s.src = url;
        s.onload = resolve;
        s.onerror = () => reject(new Error("CKEditor load failed"));
        document.head.appendChild(s);
    });
    await waitFor(() => !!window.CKEDITOR, 4000);
}

// --- API ---
export async function initMiniEditor(editorId, base64Content) {
    await loadCkeditor(); // asegura CKEDITOR

    const el = document.getElementById(editorId);
    if (!el) {
        console.warn(`⚠️ No se encontró el textarea con ID '${editorId}'`);
        return;
    }

    // destruir si existe
    const inst = window.CKEDITOR.instances?.[editorId];
    if (inst) inst.destroy(true);

    const editor = window.CKEDITOR.replace(editorId, {
        language: "es",
        versionCheck: false,
        removePlugins: "exportpdf,image",
        height: 220,
        contentsCss: "/editor/ckeditor/contents.css",
        toolbar: [
            { name: "basicstyles", items: ["Bold", "Italic", "Underline", "RemoveFormat"] },
            { name: "paragraph", items: ["NumberedList", "BulletedList", "-", "Outdent", "Indent"] },
            { name: "align", items: ["JustifyLeft", "JustifyCenter", "JustifyRight", "JustifyBlock"] },
            { name: "links", items: ["Link", "Unlink"] },
            { name: "styles", items: ["Format"] },
            { name: "tools", items: ["Maximize"] }
        ]
    });

    editor.on("instanceReady", () => {
        if (base64Content) {
            try {
                const decodedHtml = atob(base64Content);
                editor.setData(decodedHtml);
            } catch (e) { console.error("❌ Error atob base64:", e); }
        }
    });
}

export function getEditorContent(editorId) {
    const inst = window.CKEDITOR?.instances?.[editorId];
    return inst ? inst.getData() : "";
}

export function destroyEditor(editorId) {
    const inst = window.CKEDITOR?.instances?.[editorId];
    if (inst) inst.destroy(true);
}

export function isReady(editorId) {
    return !!window.CKEDITOR?.instances?.[editorId];
}


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


//window.bindEditorDirty = (editorId, dotnetRef) => {
//    if (typeof CKEDITOR === "undefined") return;

//    const editor = CKEDITOR.instances[editorId];
//    if (!editor) {
//        console.warn("bindEditorDirty: editor no encontrado:", editorId);
//        return;
//    }

//    // CKEditor 4: eventos recomendados
//    const fire = () => {
//        try { dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }
//    };

//    editor.on('change', fire);
//    editor.on('key', fire);    // más inmediato
//    editor.on('paste', fire);  // por si pega texto
//};



window.__editorDirtyPaused = window.__editorDirtyPaused || {};

window.pauseEditorDirty = (editorId = "editor1") => {
    window.__editorDirtyPaused[editorId] = true;
};

window.resumeEditorDirty = (editorId = "editor1") => {
    window.__editorDirtyPaused[editorId] = false;
};




//window.bindEditorDirty = (editorId, dotnetRef, draftKey, tituloKey) => {
//    if (typeof CKEDITOR === "undefined") return;

//    const editor = CKEDITOR.instances[editorId];
//    if (!editor) return;

//    let t = null;

//    const snapshot = () => {
//        try {
//            const html = editor.getData() || "";
//            localStorage.setItem(draftKey, html);
//            // el título lo guardamos desde C# cuando cambia o al guardar
//        } catch { }
//    };

//    const fire = () => {
//        try { dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }
//        clearTimeout(t);
//        t = setTimeout(snapshot, 1500);
//    };

//    editor.on('change', fire);
//    editor.on('key', fire);
//    editor.on('paste', fire);

//    document.addEventListener("visibilitychange", () => {
//        if (document.visibilityState === "hidden") snapshot();
//    });

//    window.addEventListener("beforeunload", () => snapshot());
//};


//window.bindEditorDirty = (editorId, dotnetRef, draftKey, tituloKey) => {
//    if (typeof CKEDITOR === "undefined") return;
//    const editor = CKEDITOR.instances[editorId];
//    if (!editor) return;

//    let t = null;

//    const snapshot = () => {
//        try {
//            const html = editor.getData() || "";
//            localStorage.setItem(draftKey, html);
//        } catch { }
//    };

//    const fire = () => {
//        // ✅ si está en pausa, NO marcar cambios ni guardar borrador
//        if (window.__editorDirtyPaused && window.__editorDirtyPaused[editorId]) return;

//        try { dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }

//        clearTimeout(t);
//        t = setTimeout(snapshot, 1500);
//    };

//    editor.on("change", fire);
//    editor.on("key", fire);
//    editor.on("paste", fire);

//    document.addEventListener("visibilitychange", () => {
//        if (document.visibilityState === "hidden") snapshot();
//    });

//    window.addEventListener("beforeunload", () => snapshot());
//};


// Estado por editorId
window.__editorDirtyState = window.__editorDirtyState || {};

window.unbindEditorDirty = (editorId) => {
    try {
        const st = window.__editorDirtyState[editorId];
        if (!st) return;

        // timeout
        try { clearTimeout(st.t); } catch { }

        // CKEditor events
        try {
            const editor = (typeof CKEDITOR !== "undefined") ? CKEDITOR.instances[editorId] : null;
            if (editor && st.fire) {
                editor.removeListener("change", st.fire);
                editor.removeListener("key", st.fire);
                editor.removeListener("paste", st.fire);
            }
        } catch { }

        // DOM events
        try { document.removeEventListener("visibilitychange", st.onVisibility); } catch { }
        try { window.removeEventListener("beforeunload", st.onBeforeUnload); } catch { }

        delete window.__editorDirtyState[editorId];
    } catch { }
};

window.bindEditorDirty = (editorId, dotnetRef, draftKey, tituloKey) => {
    if (typeof CKEDITOR === "undefined") return;
    const editor = CKEDITOR.instances[editorId];
    if (!editor) return;

    // ✅ si ya estaba bindeado, desbindea antes
    window.unbindEditorDirty(editorId);

    let t = null;

    //const snapshot = () => {
    //    try {
    //        const html = editor.getData() || "";
    //        localStorage.setItem(draftKey, html);

    //        // (si guardás título desde JS, acá)
    //        // localStorage.setItem(tituloKey, ...);
    //    } catch { }
    //};

    let lastSaved = null;

    const snapshot = () => {
        try {
            const html = editor.getData() || "";
            if (!html.trim()) return;
            if (lastSaved !== null && html === lastSaved) return; // ✅ no guardar si no cambió
            lastSaved = html;
            localStorage.setItem(draftKey, html);
        } catch { }
    };


    const fire = () => {
        // ✅ si está en pausa, NO marcar cambios ni guardar borrador
        if (window.__editorDirtyPaused && window.__editorDirtyPaused[editorId]) return;

        try { dotnetRef.invokeMethodAsync("EditorChanged"); } catch { }

        try { clearTimeout(t); } catch { }
        t = setTimeout(snapshot, 1500);
    };

    // CKEditor events
    editor.on("change", fire);
    editor.on("key", fire);
    editor.on("paste", fire);

    // DOM events (guardamos refs para poder remover)
    const onVisibility = () => {
        if (document.visibilityState === "hidden") snapshot();
    };
    const onBeforeUnload = () => snapshot();

    document.addEventListener("visibilitychange", onVisibility);
    window.addEventListener("beforeunload", onBeforeUnload);

    // ✅ guardar refs para unbind
    window.__editorDirtyState[editorId] = {
        fire,
        snapshot,
        t,
        onVisibility,
        onBeforeUnload
    };

    // 🔁 mantener actualizado el timeout id en el state
    // (porque t cambia)
    const st = window.__editorDirtyState[editorId];
    Object.defineProperty(st, "t", {
        get: () => t,
        set: (v) => { t = v; }
    });
};








// Setear contenido HTML directo (NO base64)
window.setEditorContent = (editorId = "editor1", html) => {
    if (typeof CKEDITOR === "undefined") return;

    const editor = CKEDITOR.instances[editorId];
    if (editor) {
        editor.setData(html || "");
    }
};
