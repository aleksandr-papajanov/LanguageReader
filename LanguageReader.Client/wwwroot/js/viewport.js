window.viewportService = (() => {
    let dotNetRef = null;
    let frame = 0;
    let timeout = 0;
    let lastSignature = "";

    function getSnapshot() {
        const visualViewport = window.visualViewport;
        const width = Math.round(visualViewport?.width ?? window.innerWidth);
        const height = Math.round(visualViewport?.height ?? window.innerHeight);

        return {
            width,
            height,
            layoutWidth: Math.round(window.innerWidth),
            layoutHeight: Math.round(window.innerHeight),
            devicePixelRatio: window.devicePixelRatio || 1,
            isTouch: window.matchMedia?.("(pointer: coarse)")?.matches
                || navigator.maxTouchPoints > 0
        };
    }

    function signatureFor(snapshot) {
        return [
            snapshot.width,
            snapshot.height,
            snapshot.layoutWidth,
            snapshot.layoutHeight,
            snapshot.devicePixelRatio,
            snapshot.isTouch
        ].join(":");
    }

    function scheduleNotify() {
        window.clearTimeout(timeout);
        window.cancelAnimationFrame(frame);

        timeout = window.setTimeout(() => {
            frame = window.requestAnimationFrame(() => {
                if (!dotNetRef) {
                    return;
                }

                const snapshot = getSnapshot();
                const signature = signatureFor(snapshot);
                if (signature === lastSignature) {
                    return;
                }

                lastSignature = signature;
                dotNetRef.invokeMethodAsync("OnViewportChanged", snapshot);
            });
        }, 80);
    }

    function addListeners() {
        window.addEventListener("resize", scheduleNotify, { passive: true });
        window.addEventListener("orientationchange", scheduleNotify, { passive: true });
        window.addEventListener("pageshow", scheduleNotify, { passive: true });
        window.visualViewport?.addEventListener("resize", scheduleNotify, { passive: true });
        window.visualViewport?.addEventListener("scroll", scheduleNotify, { passive: true });
    }

    function removeListeners() {
        window.removeEventListener("resize", scheduleNotify);
        window.removeEventListener("orientationchange", scheduleNotify);
        window.removeEventListener("pageshow", scheduleNotify);
        window.visualViewport?.removeEventListener("resize", scheduleNotify);
        window.visualViewport?.removeEventListener("scroll", scheduleNotify);
    }

    function initialize(ref) {
        dispose();

        dotNetRef = ref;
        const snapshot = getSnapshot();
        lastSignature = signatureFor(snapshot);
        addListeners();

        return snapshot;
    }

    function dispose() {
        removeListeners();
        window.clearTimeout(timeout);
        window.cancelAnimationFrame(frame);

        dotNetRef = null;
        frame = 0;
        timeout = 0;
        lastSignature = "";
    }

    return {
        initialize,
        getSnapshot,
        dispose
    };
})();
