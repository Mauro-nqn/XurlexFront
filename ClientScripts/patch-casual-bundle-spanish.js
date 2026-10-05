const fs = require("fs");

const file = "./wwwroot/js/casual-xurlex.bundle.js";

let s = fs.readFileSync(file, "utf8");

const replacements = [
    ['label: "Edit"', 'label: "Editar"'],
    ['label: "View"', 'label: "Ver"'],

    ['label: "Undo"', 'label: "Deshacer"'],
    ['label: "Redo"', 'label: "Rehacer"'],
    ['label: "Cut"', 'label: "Cortar"'],
    ['label: "Copy"', 'label: "Copiar"'],
    ['label: "Paste"', 'label: "Pegar"'],
    ['label: "Paste without formatting"', 'label: "Pegar sin formato"'],
    ['label: "Select all"', 'label: "Seleccionar todo"'],

    ['}Bold`', '}Negrita`'],
    ['}Italic`', '}Cursiva`'],
    ['}Underline`', '}Subrayado`'],
    ['}Strikethrough`', '}Tachado`'],
    ['}Small Caps`', '}Versalitas`'],
    ['}All Caps`', '}Mayúsculas`'],

    ['label: "Clear formatting"', 'label: "Borrar formato"'],

    ['label: "Zoom in"', 'label: "Acercar"'],
    ['label: "Zoom out"', 'label: "Alejar"'],
    ['label: "Reset zoom (100%)"', 'label: "Restablecer zoom (100%)"'],

    ['}Show ruler`', '}Mostrar regla`'],

    ['}Theme: match system`', '}Tema: usar el del sistema`'],
    ['}Theme: light`', '}Tema: claro`'],
    ['}Theme: dark`', '}Tema: oscuro`'],

    ['label: "Export as PDF"', 'label: "Exportar como PDF"'],
    ['label: "Export as ODT"', 'label: "Exportar como ODT"'],
    ['label: "Export as Markdown"', 'label: "Exportar como Markdown"'],
    ['label: "Export as Plain Text"', 'label: "Exportar como texto plano"'],
    ['label: "Properties"', 'label: "Propiedades"']
];

let total = 0;

for (const [from, to] of replacements) {
    const count = s.split(from).length - 1;

    if (count > 0) {
        s = s.split(from).join(to);
        console.log(`${from} -> ${to} (${count})`);
        total += count;
    }
}

fs.writeFileSync(file, s, "utf8");

console.log("");
console.log("======================================");
console.log("Bundle Casual Office parcheado");
console.log(`Reemplazos realizados: ${total}`);
console.log("======================================");