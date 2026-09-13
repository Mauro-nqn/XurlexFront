//window.mic = (() => {
//    async function ensurePermission() {
//        try {
//            if (!navigator.mediaDevices?.getUserMedia) return false;

//            // Pedir audio una vez (warm-up)
//            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

//            // Cortar inmediatamente
//            stream.getTracks().forEach(t => t.stop());
//            return true;
//        } catch {
//            return false;
//        }
//    }

//    return { ensurePermission };
//})();


//window.mic = (() => {
//    async function ensurePermission() {
//        try {
//            if (!navigator.mediaDevices?.getUserMedia) return false;

//            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

//            // iOS: dejalo un toque vivo para que el permiso “asiente”
//            await new Promise(r => setTimeout(r, 400));

//            stream.getTracks().forEach(t => t.stop());
//            return true;
//        } catch (e) {
//            return false;
//        }
//    }

//    return { ensurePermission };
//})();


window.mic = (() => {
    async function ensurePermission() {
        try {
            if (!navigator.mediaDevices?.getUserMedia) return false;

            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            await new Promise(r => setTimeout(r, 400));
            stream.getTracks().forEach(t => t.stop());

            // ✅ persistir OK
            try { localStorage.setItem("mic_ok", "1"); } catch { }
            return true;
        } catch (e) {
            return false;
        }
    }

    function getStored() {
        try { return localStorage.getItem("mic_ok") === "1"; }
        catch { return false; }
    }

    return { ensurePermission, getStored };
})();

