const languageReaderMascotActivityObservers = new Map();
let languageReaderMascotActivityObserverId = 0;

window.languageReaderMascotActivity = {
    observe: (dotNetReference) => {
        const id = `mascot-activity-${++languageReaderMascotActivityObserverId}`;
        let lastNotificationAt = 0;

        const notify = () => {
            const now = Date.now();
            if (now - lastNotificationAt < 300) {
                return;
            }

            lastNotificationAt = now;
            dotNetReference.invokeMethodAsync("NotifyMascotActivityAsync");
        };

        document.addEventListener("click", notify, { passive: true, capture: true });
        document.addEventListener("keydown", notify, { passive: true, capture: true });
        document.addEventListener("touchstart", notify, { passive: true, capture: true });
        window.addEventListener("scroll", notify, { passive: true, capture: true });

        languageReaderMascotActivityObservers.set(id, () => {
            document.removeEventListener("click", notify, true);
            document.removeEventListener("keydown", notify, true);
            document.removeEventListener("touchstart", notify, true);
            window.removeEventListener("scroll", notify, true);
        });

        return id;
    },

    unobserve: (id) => {
        const disconnect = languageReaderMascotActivityObservers.get(id);
        if (!disconnect) {
            return;
        }

        disconnect();
        languageReaderMascotActivityObservers.delete(id);
    }
};
