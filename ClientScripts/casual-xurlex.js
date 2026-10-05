import { renderAsync } from "@casualoffice/docs";
import "@casualoffice/docs/styles.css";
import es from "./es.json";

let editorHandle = null;
let currentBuffer = null;

console.log("=== XURLEX CASUAL JS VERSION 2026-09-19-1909 ===");


// let dotNetReference = null;



let editorApi = null;

let dotNetRef = null;

let programmaticSave = false;



function arrayBufferToBase64(buffer) {

    const bytes = new Uint8Array(buffer);
    let binary = "";

    const chunkSize = 0x8000;

    for (let i = 0; i < bytes.length; i += chunkSize) {

        const chunk = bytes.subarray(
            i,
            Math.min(i + chunkSize, bytes.length)
        );

        binary += String.fromCharCode.apply(
            null,
            chunk
        );
    }

    return btoa(binary);
}



// ==========================================================
// DESTRUIR EDITOR ANTERIOR
// ==========================================================

function destroyCurrentEditor() {

    if (editorApi) {
        try {
            editorApi.destroy?.();
        } catch (e) {
            console.warn(e);
        }
    }

    editorApi = null;
    currentBuffer = null;
}


//Funcion agrandar editor pantallas grande
function getInitialZoom() {

    const width = window.innerWidth;

    if (width >= 1800)
        return 1.5;

    if (width >= 1500)
        return 1.25;

    if (width >= 1200)
        return 1.1;

    return 1.0;
}


// ==========================================================
// CREAR EDITOR
// ==========================================================



// ==========================================================
// CREAR EDITOR
// ==========================================================

// async function createEditor(root, documentData) {

//     destroyCurrentEditor();

//     root.innerHTML = "";

//     console.log("[Casual] iniciando renderAsync");

//     renderAsync(
//         documentData,
//         root,
//         {
//             documentMode: "editing",
//             i18n: es,

//             onSave: function (buffer) {
//                 console.log("[Casual] onSave recibido:", buffer);

//                 currentBuffer = buffer;
//             }
//         }
//     ).catch(function (error) {

//         console.error(
//             "[Casual] error en renderAsync:",
//             error
//         );
//     });

//     console.log("[Casual] editor lanzado");
// }


async function createEditor(root, documentData) {

    destroyCurrentEditor();

    root.innerHTML = "";

    renderAsync(
        documentData,
        root,
        {
            documentMode: "editing",
            i18n: es,

            // onReady: function (api) {
            //     console.log("[Casual] API lista:", api);

            //     editorApi = api;

            //     // Solo para diagnóstico temporal
            //     window.casualDebugApi = api;
            // },

            onReady: function (api) {

                console.log(
                    "[Casual] API lista:",
                    api
                );

                editorApi = api;

                // Solo para diagnóstico temporal
                window.casualDebugApi = api;

                // Zoom inicial según tamaño de pantalla
                const zoom = getInitialZoom();

                console.log(
                    "[Casual] zoom inicial:",
                    zoom
                );

                editorApi.setZoom(zoom);
            },


            onSave: function (buffer) {

                console.log(
                    "[Casual] onSave recibido:",
                    buffer,
                    "programmaticSave:",
                    programmaticSave
                );

                // Casual puede ejecutar onSave internamente.
                // Acá solamente conservamos el DOCX generado.
                currentBuffer = buffer;
            },
        }
    ).catch(function (error) {
        console.error(
            "[Casual] error en renderAsync:",
            error
        );
    });
}


// ==========================================================
// API PÚBLICA PARA BLAZOR
// ==========================================================

window.casualXurlex = {

    // ------------------------------------------------------
    // ABRIR DESDE URL
    // Lo dejamos disponible para pruebas
    // ------------------------------------------------------

    open: async function (elementId, documentUrl) {

        const root = document.getElementById(elementId);

        if (!root)
            throw new Error(
                `No se encontró el elemento '${elementId}'`
            );

        const response = await fetch(documentUrl);

        if (!response.ok)
            throw new Error(
                `No se pudo descargar DOCX. HTTP ${response.status}`
            );

        const blob = await response.blob();

        await createEditor(
            root,
            blob
        );
    },


    // ------------------------------------------------------
    // ABRIR DESDE BYTE[] DE BLAZOR
    // ------------------------------------------------------

    openBytes: async function (
        elementId,
        bytes,
        dotNetReference
    ) {

        console.log("[Casual] >>> ENTRANDO openBytes");

        dotNetRef = dotNetReference;

        console.log(
            "[Casual] referencia Blazor recibida:",
            dotNetRef
        );

        const root = document.getElementById(elementId);

        if (!root)
            throw new Error(
                `No se encontró el elemento '${elementId}'`
            );

        if (!bytes)
            throw new Error(
                "No se recibió contenido del documento."
            );

        const uint8Array = new Uint8Array(bytes);

        if (uint8Array.length === 0)
            throw new Error(
                "El documento recibido está vacío."
            );

        if (
            uint8Array.length < 4 ||
            uint8Array[0] !== 0x50 ||
            uint8Array[1] !== 0x4B ||
            uint8Array[2] !== 0x03 ||
            uint8Array[3] !== 0x04
        ) {
            throw new Error(
                "El contenido recibido no parece ser un DOCX válido."
            );
        }

        console.log("[Casual] >>> LLAMANDO createEditor");

        await createEditor(
            root,
            uint8Array
        );

        console.log("[Casual] >>> createEditor TERMINÓ");
    },





    clearAutosave: async function () {

        return new Promise((resolve, reject) => {

            const request = indexedDB.open("casual-docs");

            request.onerror = () => {
                reject(request.error);
            };

            request.onsuccess = () => {

                const db = request.result;

                try {
                    const tx = db.transaction(
                        "autosave",
                        "readwrite"
                    );

                    const store = tx.objectStore("autosave");

                    store.delete("current");

                    tx.oncomplete = () => {
                        console.log(
                            "[Casual] autosave eliminado después del guardado."
                        );

                        db.close();
                        resolve();
                    };

                    tx.onerror = () => {
                        db.close();
                        reject(tx.error);
                    };
                }
                catch (error) {
                    db.close();
                    reject(error);
                }
            };
        });
    },

    

    // ------------------------------------------------------
    // GUARDAR
    // ------------------------------------------------------

    // save: async function () {

    //     if (!editorHandle)
    //         throw new Error(
    //             "Casual Editor no está inicializado."
    //         );

    //     currentBuffer = null;

    //     // Casual genera el DOCX actualizado.
    //     await editorHandle.save();

    //     if (!currentBuffer)
    //         throw new Error(
    //             "Casual no devolvió el documento guardado."
    //         );

    //     const bytes = new Uint8Array(currentBuffer);

    //     // Convertir DOCX a Base64 para enviarlo
    //     // de forma segura a Blazor.
    //     let binary = "";

    //     const chunkSize = 0x8000;

    //     for (let i = 0; i < bytes.length; i += chunkSize) {

    //         const chunk = bytes.subarray(
    //             i,
    //             Math.min(i + chunkSize, bytes.length)
    //         );

    //         binary += String.fromCharCode.apply(
    //             null,
    //             chunk
    //         );
    //     }

    //     return btoa(binary);
    // },


    // save: async function () {

    //     if (!editorApi)
    //         throw new Error(
    //             "Casual Editor todavía no está listo."
    //         );

    //     console.log("[Casual] solicitando DOCX actual...");

    //     const result = await editorApi.save();

    //     console.log(
    //         "[Casual] editorApi.save devolvió:",
    //         result
    //     );

    //     let buffer = result ?? currentBuffer;

    //     if (!buffer)
    //         throw new Error(
    //             "Casual no devolvió el documento."
    //         );

    //     // Puede devolver ArrayBuffer o una vista tipada.
    //     let bytes;

    //     if (buffer instanceof ArrayBuffer) {
    //         bytes = new Uint8Array(buffer);
    //     }
    //     else if (ArrayBuffer.isView(buffer)) {
    //         bytes = new Uint8Array(
    //             buffer.buffer,
    //             buffer.byteOffset,
    //             buffer.byteLength
    //         );
    //     }
    //     else {
    //         throw new Error(
    //             "Casual devolvió un formato de documento inesperado."
    //         );
    //     }

    //     let binary = "";
    //     const chunkSize = 0x8000;

    //     for (let i = 0; i < bytes.length; i += chunkSize) {

    //         const chunk = bytes.subarray(
    //             i,
    //             Math.min(i + chunkSize, bytes.length)
    //         );

    //         binary += String.fromCharCode.apply(
    //             null,
    //             chunk
    //         );
    //     }

    //     return btoa(binary);
    // },

    save: async function () {

        if (!editorApi)
            throw new Error(
                "Casual Editor todavía no está listo."
            );

        console.log("[Casual] solicitando DOCX actual...");

        let result;

        programmaticSave = true;

        try {
            result = await editorApi.save();
        }
        finally {
            programmaticSave = false;
        }

        console.log(
            "[Casual] editorApi.save devolvió:",
            result
        );

        let buffer = result ?? currentBuffer;

        if (!buffer)
            throw new Error(
                "Casual no devolvió el documento."
            );

        let bytes;

        if (buffer instanceof ArrayBuffer) {
            bytes = new Uint8Array(buffer);
        }
        else if (ArrayBuffer.isView(buffer)) {
            bytes = new Uint8Array(
                buffer.buffer,
                buffer.byteOffset,
                buffer.byteLength
            );
        }
        else {
            throw new Error(
                "Casual devolvió un formato de documento inesperado."
            );
        }

        let binary = "";
        const chunkSize = 0x8000;

        for (let i = 0; i < bytes.length; i += chunkSize) {

            const chunk = bytes.subarray(
                i,
                Math.min(
                    i + chunkSize,
                    bytes.length
                )
            );

            binary += String.fromCharCode.apply(
                null,
                chunk
            );
        }

        return btoa(binary);
    },


    // ------------------------------------------------------
    // DESTRUIR
    // ------------------------------------------------------

    destroy: function () {

        destroyCurrentEditor();
    }




};