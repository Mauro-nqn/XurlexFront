window.audioUtils = {
    beep: async () => {
        try {
            const AudioCtx = window.AudioContext || window.webkitAudioContext;
            if (!AudioCtx) return false;

            const ctx = new AudioCtx();

            // en móviles suele arrancar "suspended"
            if (ctx.state === "suspended") {
                await ctx.resume();
            }

            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = "sine";
            osc.frequency.value = 880;
            gain.gain.value = 0.25;

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start();
            setTimeout(() => {
                osc.stop();
                ctx.close();
            }, 160);

            return true;
        } catch {
            return false;
        }
    }
};
