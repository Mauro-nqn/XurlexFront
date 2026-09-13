window.initEditor = (base64Content) => {
    if (typeof CKEDITOR === "undefined") {
        console.error("❌ CKEditor no se cargó.");
        return;
    }

    // Si ya existe una instancia, destruirla antes de crear otra
    if (CKEDITOR.instances.editor1) {
        CKEDITOR.instances.editor1.destroy(true);
    }

    const editor = CKEDITOR.replace("editor1", {
        language: 'es',
        versionCheck: false,
        height: 500,
        contentsCss: '/editor/ckeditor/contents.css',
        toolbar: [
            { name: 'document', items: ['Source', 'Save', 'Preview', 'Print'] },
            { name: 'clipboard', items: ['Cut', 'Copy', 'Paste', 'Undo', 'Redo'] },
            { name: 'basicstyles', items: ['Bold', 'Italic', 'Underline'] },
            { name: 'paragraph', items: ['NumberedList', 'BulletedList', 'JustifyLeft', 'JustifyCenter', 'JustifyRight'] },
            { name: 'insert', items: ['Image', 'Table', 'HorizontalRule', 'SpecialChar'] },
            { name: 'styles', items: ['Format', 'Font', 'FontSize'] },
            { name: 'colors', items: ['TextColor', 'BGColor'] },
            { name: 'tools', items: ['Maximize'] }
        ]
    });

    editor.on('instanceReady', () => {
        console.log("✅ CKEditor listo.");
        if (base64Content) {
            const decodedHtml = decodeURIComponent(escape(window.atob(base64Content)));
            editor.setData(decodedHtml);
            console.log("📥 Contenido inicial cargado en CKEditor.");
        }
    });
};

// ✅ Setear contenido desde C#
window.setEditorContentFromBase64 = (base64Content) => {
    if (CKEDITOR.instances.editor1) {
        const decodedHtml = decodeURIComponent(escape(window.atob(base64Content)));
        CKEDITOR.instances.editor1.setData(decodedHtml);
    }
};

// ✅ Obtener contenido para guardar (HTML puro)
window.getEditorContent = () => {
    if (CKEDITOR.instances.editor1) {
        return CKEDITOR.instances.editor1.getData();
    }
    return "";
};

// ✅ Destruir instancia del editor al salir
window.destroyEditor = () => {
    if (CKEDITOR.instances.editor1) {
        CKEDITOR.instances.editor1.destroy(true);
        console.log("🗑️ CKEditor destruido correctamente.");
    }
};





// Mostrar opciones tipo ActionSheet
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

// Mostrar toast centrado en pantalla
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
