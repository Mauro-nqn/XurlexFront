window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);

    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();

    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
};


window.pickFile = async (allowedExtension) => {
    return new Promise((resolve, reject) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = allowedExtension || "*/*";
        input.onchange = () => {
            const file = input.files[0];
            if (!file) {
                reject("No se seleccionó ningún archivo.");
                return;
            }

            const reader = new FileReader();
            reader.onload = () => {
                resolve({
                    fileName: file.name,
                    content: reader.result // ✅ Contenido completo en texto (incluye BEGIN/END CERTIFICATE)
                });
            };
            reader.onerror = (err) => reject(err);
            reader.readAsText(file); // 👈 Leemos como texto plano, NO como Base64
        };
        input.click();
    });
};

//window.presupuesto = {
//    getPrintHtml: function () {
//        var el = document.getElementById('print-root');
//        return el ? el.outerHTML : '';
//    }
//};




