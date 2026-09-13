function initCKEditor() {
    const target = document.querySelector('#editor1');
    if (!target) {
        console.error("❌ No se encontró #editor1");
        return;
    }

    ClassicEditor.create(target)
        .then(editor => {
            console.log("✅ Editor cargado correctamente");
        })
        .catch(error => {
            console.error("❌ Error al inicializar el editor:", error);
        });
}
