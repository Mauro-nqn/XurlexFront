
// wwwroot/js/tts.js
window.tts = (() => {
    let voicesReady = false;

    let _unlocked = false;

    async function unlock(lang = "es-AR") {
        try {
            if (!window.speechSynthesis) return false;
            if (_unlocked) return true;

            await ensureVoicesLoaded();

            // iOS: a veces necesita un utterance audible (muy bajo) para "enganchar"
            const u = new SpeechSynthesisUtterance("Listo");
            u.lang = lang;
            u.volume = 0.01; // casi inaudible, pero no 0
            u.rate = 1;

            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(u);

            _unlocked = true;
            return true;
        } catch {
            return false;
        }
    }





    function supported() {
        return !!window.speechSynthesis;
    }

    function ensureVoicesLoaded() {
        return new Promise((resolve) => {
            if (!supported()) return resolve(false);

            const voices = window.speechSynthesis.getVoices();
            if (voices && voices.length > 0) {
                voicesReady = true;
                return resolve(true);
            }

            // Esperar evento (en Android a veces tarda)
            window.speechSynthesis.onvoiceschanged = () => {
                voicesReady = true;
                resolve(true);
            };

            // Trigger
            window.speechSynthesis.getVoices();
        });
    }

    

    function normalizeForSpeech(text) {
        if (!text) return "";

        let t = String(text);

        // HTML / saltos
        t = t.replace(/&nbsp;/g, " ");
        t = t.replace(/\r\n|\r|\n/g, ". ");
        t = t.replace(/<[^>]*>/g, " ");

        // 1) Convertir tokens tipo "/RPI" o "/MEV" => "RPI" (sin decir barra)
        //    Solo cuando el slash está pegado a una palabra corta en mayúsculas o letras/números
        t = t.replace(/(^|\s)\/([A-ZÁÉÍÓÚÑ0-9]{2,10})(?=\s|[.,;:!?]|$)/g, "$1$2");

        // 2) Casos jurídicos típicos: c/ Perez, s/ Daños
        t = t.replace(/\bc\s*\/\s*/gi, " contra ");
        t = t.replace(/\bs\s*\/\s*/gi, " sobre ");

        // 3) Paréntesis con texto: si es corto y en mayúsculas, leer como "nota: ..."
        //    Ej: (NO SISCOM) -> "nota: no siscom"
        t = t.replace(/\(([A-ZÁÉÍÓÚÑ0-9 ]{2,30})\)/g, (m, inner) => {
            const cleaned = inner.trim().toLowerCase();
            return ` nota: ${cleaned}. `;
        });

        // 4) Quitar paréntesis/brackets restantes pero mantener separación
        t = t.replace(/[\(\)\[\]\{\}]/g, " ");

        // 5) Slashes restantes => pausa
        t = t.replace(/\s*\/\s*/g, ", ");

        // 6) Guiones / rayas
        t = t.replace(/[—–]/g, ", ");
        t = t.replace(/\s-\s/g, ", ");
        t = t.replace(/-{2,}/g, ". ");
        t = t.replace(/(\d)\-(\d)/g, "$1 $2");

        // 7) Símbolos conflictivos
        t = t.replace(/[§¶°ºª•…]/g, " ");
        t = t.replace(/[=<>]/g, " ");
        t = t.replace(/[#@*_~^`]/g, " ");

        // 8) Puntuación repetida
        t = t.replace(/([,.!?])\1+/g, "$1");
        t = t.replace(/[:;]/g, ". ");

        // 9) Espacios
        t = t.replace(/\s+/g, " ").trim();

        const skip = new Set(["NO", "SI", "DE", "LA", "EL", "DEL", "AL", "OFICIO", "CEDULA"]);
        t = t.replace(/\b([A-Z]{2,6})\b/g, (m, s) => skip.has(s) ? s : s.split("").join(" "));


        return t;
    }




    function splitIntoChunks(text, maxLen = 220) {
        // Separa por frases, pero mantiene tamaño razonable
        const parts = text
            .split(/([.!?]\s+)/)   // mantiene el separador
            .reduce((acc, cur) => {
                if (!cur) return acc;
                if (acc.length === 0) return [cur];
                // si el separador viene solo, se lo pegamos al último
                if (/^[.!?]\s+$/.test(cur)) {
                    acc[acc.length - 1] += cur;
                    return acc;
                }
                acc.push(cur);
                return acc;
            }, []);

        const chunks = [];
        let buf = "";

        for (const p of parts) {
            const piece = p.trim();
            if (!piece) continue;

            if ((buf + " " + piece).trim().length <= maxLen) {
                buf = (buf ? (buf + " " + piece) : piece);
            } else {
                if (buf) chunks.push(buf.trim());
                buf = piece;
            }
        }
        if (buf) chunks.push(buf.trim());

        return chunks;
    }

   

    let _queue = [];
    let _i = 0;
    let _timer = null;
    let _paused = false;
    let _resolve = null;
    let _lang = "es-AR";
    let _options = {};
    let _voice = null;

    // bandera: si se detuvo voluntariamente (stop), no lo tratamos como error
    let _stoppedByUser = false;

    function clearTimer() {
        if (_timer) { clearTimeout(_timer); _timer = null; }
    }

    function stop() {
        if (!window.speechSynthesis) return;
        _stoppedByUser = true;

        clearTimer();
        _paused = false;
        _queue = [];
        _i = 0;

        try { window.speechSynthesis.cancel(); } catch { }

        // resolver como true para no mostrar error
        const r = _resolve;
        _resolve = null;
        if (r) r(true);
    }


    function stopInternal() {
        if (!window.speechSynthesis) return;

        clearTimer();
        _paused = false;
        _queue = [];
        _i = 0;

        try { window.speechSynthesis.cancel(); } catch { }

        // NO resolver acá
        _resolve = null;
    }


 
    function pause() { stop(); }   // en móvil pausa = stop
    function resume() { /* no-op */ return; }


    function speakNext() {
        if (_paused) return;

        if (_i >= _queue.length) {
            const r = _resolve;
            _resolve = null;
            _queue = [];
            _i = 0;
            if (r) r(true);
            return;
        }

        const u = new SpeechSynthesisUtterance(_queue[_i]);
        u.lang = _lang;
        u.rate = _options.rate ?? 1;
        u.pitch = _options.pitch ?? 1;
        u.volume = _options.volume ?? 1;
        if (_voice) u.voice = _voice;

        u.onend = () => {
            _i++;
            clearTimer();
            _timer = setTimeout(() => speakNext(), _options.gapMs ?? 120);
        };

        u.onerror = () => {
            const r = _resolve;
            _resolve = null;

            // Si el usuario frenó, no es error
            if (_stoppedByUser) { if (r) r(true); return; }

            if (r) r(false);
        };

        window.speechSynthesis.speak(u);
    }

    //async function speakQueued(text, lang = "es-AR", options = {}) {
    //    if (!window.speechSynthesis) return false;

    //    // iOS: si no desbloqueaste, intentar unlock rápido (sin await)
    //    if (!_unlocked) {
    //        try {
    //            const pre = new SpeechSynthesisUtterance(" ");
    //            pre.lang = lang;
    //            pre.volume = 1;       // NO 0
    //            window.speechSynthesis.speak(pre);
    //            _unlocked = true;
    //        } catch { }
    //    }

    //    _stoppedByUser = false;
    //    stopInternal();

    //    // ⚠️ En iOS, esto puede demorar; pero ya “arrancaste” speak arriba.
    //    await ensureVoicesLoaded();



    //    //if (!window.speechSynthesis) return false;

    //    //// Reset estado
    //    //_stoppedByUser = false;
    //    //stopInternal();          // ✅ limpia sin marcar stop del usuario
    //    //await ensureVoicesLoaded();


    //    const normalized = normalizeForSpeech(text);
    //    if (!normalized) return false;

    //    _lang = lang;
    //    _options = options;

    //    _queue = splitIntoChunks(normalized, options.chunkLen ?? 220);
    //    _i = 0;
    //    _paused = false;

    //    const voices = getVoices();
    //    _voice = options.voiceName ? voices.find(v => v.name === options.voiceName) : null;

    //    return new Promise((resolve) => {
    //        _resolve = resolve;
    //        speakNext();
    //    });
    //}

    async function speakQueued(text, lang = "es-AR", options = {}) {
        if (!window.speechSynthesis) return false;

        _stoppedByUser = false;

        // 1) Limpiar ANTES (esto hace cancel)
        stopInternal();

        // 2) iOS: unlock (SIN cancelar después)
        if (!_unlocked) {
            try {
                const pre = new SpeechSynthesisUtterance("Listo");
                pre.lang = lang;
                pre.volume = 1;
                pre.rate = 1;
                pre.pitch = 1;

                window.speechSynthesis.speak(pre);
                _unlocked = true;

                // iOS: pequeño respiro para que arranque el motor
                await new Promise(r => setTimeout(r, 80));
            } catch { }
        }

        // 3) Cargar voces (ok que tenga await, ya estás “enganchado”)
        await ensureVoicesLoaded();

        const normalized = normalizeForSpeech(text);
        if (!normalized) return false;

        _lang = lang;
        _options = options;

        _queue = splitIntoChunks(normalized, options.chunkLen ?? 220);
        _i = 0;
        _paused = false;

        // ⚠️ iOS: NO fuerces voiceName por defecto
        const voices = getVoices();
        _voice = options.voiceName ? voices.find(v => v.name === options.voiceName) : null;

        return new Promise((resolve) => {
            _resolve = resolve;
            speakNext();
        });
    }





    function getVoices() {
        if (!supported()) return [];
        return window.speechSynthesis.getVoices() || [];
    }

    async function listVoices() {
        await ensureVoicesLoaded();
        return getVoices().map(v => ({ name: v.name, lang: v.lang, def: v.default }));
    }

    

    async function speak(text, lang = "es-AR", options = {}) {
        if (!supported()) return false;

        stop();
        await ensureVoicesLoaded();

        const normalized = normalizeForSpeech(text);
        if (!normalized) return false;

        const u = new SpeechSynthesisUtterance(normalized);
        u.lang = lang;
        u.rate = options.rate ?? 1;
        u.pitch = options.pitch ?? 1;
        u.volume = options.volume ?? 1;

        const voices = getVoices();

        // Forzar voz exacta si viene
        if (options.voiceName) {
            const exact = voices.find(v => v.name === options.voiceName);
            if (exact) u.voice = exact;
        } else {
            // Fallback: primera voz en español (Android puede no tener "femenina")
            //const es = voices.filter(v => (v.lang || "").toLowerCase().startsWith("es"));
            //if (es.length) u.voice = es[0];
        }

        window.speechSynthesis.speak(u);
        return true;
    }

    function iosUnlock(lang = "es-AR") {
        try {
            const u = new SpeechSynthesisUtterance("Audio listo");
            u.lang = lang;
            u.volume = 1;      // NO 0
            u.rate = 1;
            u.pitch = 1;

            window.speechSynthesis.cancel();
            window.speechSynthesis.speak(u);
            _unlocked = true;
            return true;
        } catch {
            return false;
        }
    }


    function testIOS() {
        try {
            if (!window.speechSynthesis) return false;

            // clave iOS: speak directo, sin awaits, y sin cancel inmediato después
            const u = new SpeechSynthesisUtterance(
                "Probando lectura en iPhone. Si escuchás esto, funciona."
            );
            u.lang = "es-AR";
            u.volume = 1;
            u.rate = 1;
            u.pitch = 1;

            window.speechSynthesis.cancel(); // limpiar anteriores
            window.speechSynthesis.speak(u);
            _unlocked = true;
            return true;
        } catch (e) {
            return false;
        }
    }


    return { unlock, supported, listVoices, speak, speakQueued, stop, pause, resume, normalizeForSpeech, iosUnlock, testIOS };
})();






window.tts_readMovimientoIOS = async (baseUrl, token, movId) => {
    try {
        if (!window.speechSynthesis) return { ok: false, error: "SpeechSynthesis no soportado" };

        // 1) Arranque inmediato dentro del click (clave iOS)
        window.speechSynthesis.cancel();
        const pre = new SpeechSynthesisUtterance("Leyendo movimiento");
        pre.lang = "es-ES";
        pre.volume = 1;
        window.speechSynthesis.speak(pre);

        // 2) Fetch al TTS del movimiento
        const movResp = await fetch(`${baseUrl}/api/movimientos/${movId}/tts`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!movResp.ok) return { ok: false, error: "No se pudo obtener el movimiento" };
        const mov = await movResp.json();

        // 3) Leer texto del movimiento (usá tu motor ya existente)
        await window.tts.speakQueued(mov.texto || mov.Texto || "", "es-ES", { chunkLen: 220, gapMs: 140, volume: 1 });

        // 4) Buscar adjuntos
        const adjResp = await fetch(`${baseUrl}/api/archivomovimiento/por-movimiento/${movId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!adjResp.ok) return { ok: true }; // movimiento leído igual
        const adj = await adjResp.json();
        if (!adj || adj.length === 0) return { ok: true };

        // 5) Preguntar (acá podés usar confirm normal)
        const desea = window.confirm(`Este movimiento tiene ${adj.length} adjunto(s). ¿Querés que lea el adjunto?`);
        if (!desea) return { ok: true };

        // 6) Leer primer adjunto
        const primero = adj[0];
        const archId = primero.id ?? primero.Id;
        const archResp = await fetch(`${baseUrl}/api/archivos-movimiento/${archId}/tts`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (!archResp.ok) return { ok: true, warn: "No se pudo leer el adjunto" };
        const arch = await archResp.json();

        if (!arch.ok && arch.Ok === false) {
            return { ok: true, warn: arch.error || arch.Error || "No se pudo leer el adjunto" };
        }

        await window.tts.speakQueued(arch.texto || arch.Texto || "", "es-ES", { chunkLen: 220, gapMs: 140, volume: 1 });
        return { ok: true };

    } catch (e) {
        return { ok: false, error: String(e?.message || e) };
    }
};


window.xurlexDevice = {
    isIOS: () => {
        const ua = navigator.userAgent || "";
        const iOS = /iPad|iPhone|iPod/i.test(ua);
        const iPadOS = navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1;
        return iOS || iPadOS;
    }
};
