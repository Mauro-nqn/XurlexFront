window.voiceConfirm = (() => {
    let rec = null;

    function supported() {
        return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
    }

    function stop() {
        try { rec?.abort(); } catch { }
        rec = null;
    }

    // Escucha SI/NO y resuelve true/false/null (null = timeout o no entendido)
    function askYesNo(lang = "es-AR", timeoutMs = 8000) {
        return new Promise((resolve) => {
            if (!supported()) return resolve(null);

            stop();

            const SR = window.SpeechRecognition || window.webkitSpeechRecognition;
            rec = new SR();
            rec.lang = lang;
            rec.interimResults = false;
            rec.continuous = false;
            rec.maxAlternatives = 3;

            let started = false;
            let done = false;

            const finish = (val) => {
                if (done) return;
                done = true;
                stop();
                resolve(val);
            };

            const timer = setTimeout(() => finish(null), timeoutMs);

            rec.onstart = () => { started = true; };

            rec.onresult = (e) => {
                clearTimeout(timer);

                const text = (e.results?.[0]?.[0]?.transcript || "").toLowerCase().trim();
                const t = text.replace(/[.,;:!?]/g, "").replace(/\s+/g, " ").trim();

                if (t.includes("sí") || t.includes("si") || t.includes("dale") || t.includes("ok")) return finish(true);
                if (t.includes("no") || t.includes("negativo") || t.includes("cancel")) return finish(false);

                finish(null);
            };

            rec.onerror = () => {
                clearTimeout(timer);
                finish(null);
            };

            rec.onend = () => {
                clearTimeout(timer);
                // si se cerró sin arrancar o sin resultado, null
                finish(null);
            };

            try { rec.start(); } catch { finish(null); }
        });
    }


    return { supported, askYesNo, stop };
})();
