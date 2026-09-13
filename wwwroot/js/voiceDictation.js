//// wwwroot/js/voiceDictation.js
//window.voiceDictation = (() => {
//    let rec = null;
//    let dotnetRef = null;


//    let lastFullFinal = ""; //  AGREGAR ESTO

//    function supported() {
//        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
//    }

//    function stop() {
//        try { if (rec) rec.stop(); } catch { }
//    }

//    function reset() {
//        lastFullFinal = "";
//    }


//    function start(targetId, dotnet, lang = "es-AR") {
//        if (!supported()) return false;

//        // si había uno activo, cerrarlo antes
//        try { if (rec) rec.abort(); } catch { }
//        rec = null;

//        const SR = window.SpeechRecognition || window.webkitSpeechRecognition;
//        rec = new SR();
//        dotnetRef = dotnet;

//        rec.lang = lang;
//        rec.interimResults = true;
//        rec.continuous = true;

//        rec.onstart = () => dotnetRef?.invokeMethodAsync("OnDictationState", true);
//        rec.onend = () => dotnetRef?.invokeMethodAsync("OnDictationState", false);
//        rec.onerror = (e) => dotnetRef?.invokeMethodAsync("OnDictationError", e?.error || "unknown");

//        rec.onresult = (event) => {
//            let newFinal = "";
//            let interim = "";

//            for (let i = event.resultIndex; i < event.results.length; i++) {
//                const res = event.results[i];
//                const txt = (res[0]?.transcript || "").trim();

//                if (res.isFinal) newFinal += " " + txt;
//                else interim += " " + txt;
//            }

//            newFinal = newFinal.replace(/\s+/g, " ").trim();
//            interim = interim.replace(/\s+/g, " ").trim();

//            if (newFinal || interim) {
//                dotnetRef?.invokeMethodAsync("OnDictationChunk", newFinal, interim);
//            }
//        };

//        rec.start();
//        return true;
//    }



//    return { supported, start, stop, reset };
//})();


// wwwroot/js/voiceDictation.js
// wwwroot/js/voiceDictation.js
window.voiceDictation = (() => {
    let rec = null;
    let dotnetRef = null;

    let finals = []; // ✅ transcript final por índice

    function supported() {
        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
    }

    function reset() {
        finals = [];
    }

    function stop() {
        try { if (rec) rec.abort(); } catch { }
    }

    function clean(s) {
        return (s || "")
            .replace(/\u00A0/g, " ")
            .replace(/\s+/g, " ")
            .trim();
    }

    function buildFinal() {
        const parts = [];

        for (let i = 0; i < finals.length; i++) {
            const seg = clean(finals[i]);
            if (!seg) continue;

            if (parts.length === 0) {
                parts.push(seg);
                continue;
            }

            const prev = parts[parts.length - 1];

            // ✅ mismo segmento => ignorar
            if (seg === prev) continue;

            // ✅ el nuevo contiene al anterior (hola -> hola como -> hola como estas)
            if (seg.startsWith(prev + " ") || seg === prev) {
                parts[parts.length - 1] = seg;
                continue;
            }

            // ✅ el anterior contiene al nuevo => ignorar
            if (prev.startsWith(seg + " ") || prev === seg) {
                continue;
            }

            parts.push(seg);
        }

        return clean(parts.join(" "));
    }

    function start(targetId, dotnet, lang = "es-AR") {
        if (!supported()) return false;

        try { if (rec) rec.abort(); } catch { }
        rec = null;

        reset();

        const SR = window.SpeechRecognition || window.webkitSpeechRecognition;
        rec = new SR();
        dotnetRef = dotnet;

        rec.lang = lang;
        rec.interimResults = true;
        rec.continuous = true;

        rec.onstart = () => dotnetRef?.invokeMethodAsync("OnDictationState", true);
        rec.onend = () => dotnetRef?.invokeMethodAsync("OnDictationState", false);
        rec.onerror = (e) => dotnetRef?.invokeMethodAsync("OnDictationError", e?.error || "unknown");

        rec.onresult = (event) => {
            let interim = "";

            // ✅ actualizar buffer por índice
            for (let i = event.resultIndex; i < event.results.length; i++) {
                const res = event.results[i];
                const txt = clean(res[0]?.transcript);

                if (!txt) continue;

                if (res.isFinal) {
                    finals[i] = txt;       // guardar final en su índice
                } else {
                    interim += " " + txt;  // juntar interim
                }
            }

            interim = clean(interim);
            const fullFinal = buildFinal();

            dotnetRef?.invokeMethodAsync("OnDictationChunk", fullFinal, interim);
        };

        rec.start();
        return true;
    }

    return { supported, start, stop, reset };
})();

