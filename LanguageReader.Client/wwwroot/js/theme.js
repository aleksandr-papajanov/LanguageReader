(function () {
  const storageKey = "languageReader.theme";
  const legacyWarmTintKey = "languageReader.warmReadingTint";
  const themes = new Set(["light", "warm", "dark"]);

  function normalize(theme) {
    const value = String(theme || "").toLowerCase();
    return themes.has(value) ? value : "light";
  }

  function getStorageItem(key) {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  function setStorageItem(key, value) {
    try {
      localStorage.setItem(key, value);
    } catch {
      // Theme persistence is nice to have; applying the current theme still works.
    }
  }

  function removeStorageItem(key) {
    try {
      localStorage.removeItem(key);
    } catch {
      // Ignore storage failures in restricted browser modes.
    }
  }

  function getStoredTheme() {
    const storedTheme = getStorageItem(storageKey);
    if (themes.has(storedTheme)) {
      return storedTheme;
    }

    const legacyWarmTint = getStorageItem(legacyWarmTintKey);
    return legacyWarmTint === "true" ? "warm" : "light";
  }

  function setMetaThemeColor(theme) {
    const meta = document.querySelector('meta[name="theme-color"]');
    if (!meta) {
      return;
    }

    const color = theme === "dark"
      ? "#15120f"
      : theme === "warm"
        ? "#f5e6aa"
        : "#f7f4ed";

    meta.setAttribute("content", color);
  }

  function apply(theme) {
    const normalized = normalize(theme);
    document.documentElement.dataset.theme = normalized;
    document.documentElement.style.colorScheme = normalized === "dark" ? "dark" : "light";
    setMetaThemeColor(normalized);
    return normalized;
  }

  window.languageReaderTheme = {
    initialize() {
      const theme = apply(getStoredTheme());
      setStorageItem(storageKey, theme);
      removeStorageItem(legacyWarmTintKey);
      return theme;
    },
    set(theme) {
      const normalized = apply(theme);
      setStorageItem(storageKey, normalized);
      return normalized;
    },
    get() {
      return normalize(document.documentElement.dataset.theme || getStoredTheme());
    }
  };

  apply(getStoredTheme());
})();
