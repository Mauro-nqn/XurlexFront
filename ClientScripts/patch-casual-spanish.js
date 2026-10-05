const fs = require("fs");
const path = require("path");

const distDir = path.resolve(
    __dirname,
    "../node_modules/@casualoffice/docs/dist"
);

// Reemplazos SOLO de textos visibles hardcodeados.
// Evitamos reemplazar palabras internas como "left", "right", "normal", etc.
const replacements = new Map([
    // Editar
    ["Undo", "Deshacer"],
    ["Redo", "Rehacer"],
    ["Cut", "Cortar"],
    ["Copy", "Copiar"],
    ["Paste", "Pegar"],
    ["Paste without formatting", "Pegar sin formato"],
    ["Select all", "Seleccionar todo"],

    // Formato
    ["Bold", "Negrita"],
    ["Italic", "Cursiva"],
    ["Underline", "Subrayado"],
    ["Strikethrough", "Tachado"],
    ["Small Caps", "Versalitas"],
    ["All Caps", "Mayúsculas"],
    ["Clear formatting", "Borrar formato"],

    // Vista
    ["Zoom in", "Acercar"],
    ["Zoom out", "Alejar"],
    ["Reset zoom (100%)", "Restablecer zoom (100%)"],
    ["Show ruler", "Mostrar regla"],
    ["Show non-printing characters", "Mostrar caracteres no imprimibles"],
    ["Show document outline", "Mostrar esquema del documento"],
    ["Theme: match system", "Tema: usar el del sistema"],
    ["Theme: light", "Tema: claro"],
    ["Theme: dark", "Tema: oscuro"],

    // Archivo
    ["Email as attachment...", "Enviar como archivo adjunto..."],
    ["Export as PDF", "Exportar como PDF"],
    ["Export as ODT", "Exportar como ODT"],
    ["Export as Markdown", "Exportar como Markdown"],
    ["Properties", "Propiedades"],

    // Herramientas
    ["Spell check", "Corrección ortográfica"],
    ["Grammar check", "Corrección gramatical"],
    ["Dictionary", "Diccionario"],
    ["Preferences...", "Preferencias..."],
    ["Accessibility...", "Accesibilidad..."],

    // Ayuda
    ["About Casual Editor", "Acerca de Casual Editor"],

    // Párrafos
    ["Line spacing", "Interlineado"],
    ["Add space before paragraph", "Agregar espacio antes del párrafo"],
    ["Remove space before paragraph", "Quitar espacio antes del párrafo"],
    ["Add space after paragraph", "Agregar espacio después del párrafo"],
    ["Remove space after paragraph", "Quitar espacio después del párrafo"],
    ["Custom spacing…", "Espaciado personalizado…"],
    ["Pagination", "Paginación"],
    ["Keep with next", "Mantener con el siguiente"],
    ["Keep lines together", "Mantener líneas juntas"],
    ["Page break before", "Salto de página anterior"],
    ["Prevent single lines", "Evitar líneas aisladas"],

    // Estilos
    ["Normal text", "Texto normal"],
    ["Title", "Título"],
    ["Subtitle", "Subtítulo"],
    ["Heading 1", "Título 1"],
    ["Heading 2", "Título 2"],
    ["Heading 3", "Título 3"],

    // Tablas
    ["Select size", "Seleccionar tamaño"],
    ["Table size selector", "Selector de tamaño de tabla"]
]);

if (!fs.existsSync(distDir)) {
    console.error("No existe:", distDir);
    process.exit(1);
}

const files = fs
    .readdirSync(distDir)
    .filter(file => file.endsWith(".js"));

let modifiedFiles = 0;
let totalReplacements = 0;

for (const file of files) {
    const fullPath = path.join(distDir, file);

    let content = fs.readFileSync(fullPath, "utf8");
    let modified = false;

    for (const [english, spanish] of replacements) {
        // Reemplazamos literales completos entre comillas.
        // Esto reduce muchísimo el riesgo de tocar nombres internos.
        const candidates = [
            [`"${english}"`, `"${spanish}"`],
            [`'${english}'`, `'${spanish}'`]
        ];

        for (const [from, to] of candidates) {
            if (content.includes(from)) {
                const occurrences = content.split(from).length - 1;

                content = content.split(from).join(to);

                totalReplacements += occurrences;
                modified = true;

                console.log(
                    `${file}: "${english}" -> "${spanish}" (${occurrences})`
                );
            }
        }
    }


    if (modified) {
        fs.writeFileSync(fullPath, content, "utf8");
        modifiedFiles++;
    }
}

console.log("");
console.log("======================================");
console.log("Parche español Casual Office terminado");
console.log(`Archivos modificados: ${modifiedFiles}`);
console.log(`Reemplazos realizados: ${totalReplacements}`);
console.log("======================================");