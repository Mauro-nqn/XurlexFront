window.ttsIOS = (() => {

    function getToken() {
        try { return localStorage.getItem("token") || ""; } catch { return ""; }
    }

    function normalizeBaseUrl(raw) {
        let s = String(raw || "").trim();
        s = s.replace(/^"+|"+$/g, "").replace(/^'+|'+$/g, "");
        s = s.replace(/\/+$/, "");
        if (s && !/^https?:\/\//i.test(s)) s = "http://" + s;
        return s;
    }

    function speakOnce(text, lang = "es-AR") {
        return new Promise((resolve) => {
            try {
                if (!window.speechSynthesis) return resolve(false);
                const u = new SpeechSynthesisUtterance(String(text || ""));
                u.lang = lang;
                u.volume = 1;
                u.rate = 1;
                u.pitch = 1;
                u.onend = () => resolve(true);
                u.onerror = () => resolve(false);
                window.speechSynthesis.speak(u);
            } catch {
                resolve(false);
            }
        });
    }

    async function fetchJsonOrText(url, opts) {
        const res = await fetch(url, opts);
        const text = await res.text();
        if (!res.ok) {
            throw new Error(`HTTP ${res.status} ${res.statusText} | ${url} | body: ${text.slice(0, 200)}`);
        }
        const ct = (res.headers.get("content-type") || "");
        if (ct.includes("application/json")) {
            try { return JSON.parse(text); } catch { return text; }
        }
        return text;
    }

    async function readMovimiento(baseUrl, token, movId) {
        try {
            if (!window.speechSynthesis) { alert("Sin SpeechSynthesis"); return false; }

            // “unlock” en el click (ideal que esto lo llames desde un handler de UI)
            try { window.speechSynthesis.cancel(); } catch { }
            const pre = new SpeechSynthesisUtterance("Cargando movimiento");
            pre.lang = "es-ES";
            pre.volume = 1;
            window.speechSynthesis.speak(pre);

            const api = normalizeBaseUrl(baseUrl);
            const tkn = String(token || "").trim() || getToken();

            if (!api || !tkn) { alert("Falta baseUrl o token"); return false; }

            // 1) Movimiento TTS
            const movUrl = new URL(`/api/movimientos/${movId}/tts`, api).toString();
            const movData = await fetchJsonOrText(movUrl, { headers: { Authorization: `Bearer ${tkn}` } });

            const textoMov = (typeof movData === "string")
                ? movData.trim()
                : (movData.texto ?? movData.Texto ?? movData.text ?? movData.Text ?? "");

            if (!textoMov) { alert("Movimiento sin texto"); return false; }
            await speakOnce(textoMov, "es-ES");

            // 2) Adjuntos (opcional)
            const adjUrl = new URL(`/api/archivomovimiento/por-movimiento/${movId}`, api).toString();
            let adj;
            try {
                adj = await fetchJsonOrText(adjUrl, { headers: { Authorization: `Bearer ${tkn}` } });
            } catch {
                return true; // si falla adjuntos, igual ya leímos movimiento
            }

            if (!Array.isArray(adj) || adj.length === 0) return true;

            const ok = confirm(`Este movimiento tiene ${adj.length} adjunto(s). ¿Querés que lea el adjunto?`);
            if (!ok) return true;

            const archId = adj[0].id ?? adj[0].Id;
            if (!archId) return true;

            // 3) Archivo TTS
            const archUrl = new URL(`/api/archivos-movimiento/${archId}/tts`, api).toString();
            const arch = await fetchJsonOrText(archUrl, { headers: { Authorization: `Bearer ${tkn}` } });

            if (arch?.ok === false || arch?.Ok === false) return true;

            const textoArch = arch?.texto ?? arch?.Texto ?? arch?.text ?? arch?.Text ?? "";
            if (textoArch) await speakOnce(textoArch, "es-ES");

            return true;
        } catch (e) {
            alert("Error iOS TTS: " + (e?.message || e));
            return false;
        }
    }

    function stop() {
        try { window.speechSynthesis.cancel(); } catch { }
    }

    // API pública
    window.tts_readMovimientoIOS = (baseUrl, token, movId) => readMovimiento(baseUrl, token, movId);

    return { readMovimiento, stop };

})();
