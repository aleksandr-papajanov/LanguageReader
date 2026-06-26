// window.languageReaderDebugViewport = true 

window.languageReaderReaderViewport = (() => {
  if ("scrollRestoration" in window.history) {
    window.history.scrollRestoration = "manual";
  }

  function getPixelTolerance() {
    const ratio = window.devicePixelRatio || 1;
    return Math.max(0.5, 1 / Math.max(1, ratio));
  }

  const maxRestoreAttempts = 6;
  const restoreTolerancePx = 1;
  const boundaryTolerancePx = 0;

  const observers = new Map();
  let observerId = 0;
  let overlayRoot = null;

  function observeScroll(root, dotNetReference) {
    if (!root) {
      return "";
    }

    const scrollContainer = getScrollContainer(root);
    const id = `reader-viewport-${++observerId}`;
    let frame = 0;
    let lastSignature = "";

    const notify = () => {
      window.cancelAnimationFrame(frame);
      frame = window.requestAnimationFrame(() => {
        const progress = getProgress(root);
        if (progress.BookmarkBlockIndex === null || progress.ProgressBlockIndex === null) {
          return;
        }

        const signature = `${progress.ProgressBlockIndex}:${progress.BookmarkBlockIndex}`;
        if (signature === lastSignature) {
          return;
        }

        lastSignature = signature;
        dotNetReference.invokeMethodAsync(
          "NotifyVisibleBlockChangedAsync",
          progress.ProgressBlockIndex,
          progress.BookmarkBlockIndex);
      });
    };

    scrollContainer.addEventListener("scroll", notify, { passive: true });
    window.addEventListener("resize", notify, { passive: true });
    notify();

    observers.set(id, {
      disconnect: () => {
        window.cancelAnimationFrame(frame);
        scrollContainer.removeEventListener("scroll", notify);
        window.removeEventListener("resize", notify);
      }
    });

    return id;
  }

  function unobserveScroll(id) {
    const observer = observers.get(id);
    if (!observer) {
      return;
    }

    observer.disconnect();
    observers.delete(id);
  }

  function getProgress(root) {
    const blocks = getTextBlocks(root);
    if (blocks.length === 0) {
      return {
        BookmarkBlockIndex: null,
        ProgressBlockIndex: null
      };
    }

    const viewport = getViewport();
    const bookmarkBlockIndex = getBookmarkBlockIndex(blocks, viewport);
    const progressBlockIndex = getProgressBlockIndex(root, blocks, viewport);
    renderDebugOverlay(root, viewport, {
      bookmarkBlockIndex,
      progressBlockIndex
    });

    log("progress", {
      bookmarkBlockIndex,
      progressBlockIndex,
      viewport
    });

    return {
      BookmarkBlockIndex: bookmarkBlockIndex,
      ProgressBlockIndex: progressBlockIndex
    };
  }

  async function scrollBlockIntoView(root, blockIndex, offset) {
    let didScroll = false;

    for (let attempt = 0; attempt < maxRestoreAttempts; attempt++) {
      const rect = getBlockRect(root, blockIndex, offset);
      if (!rect) {
        log("restore-missing-block", { blockIndex, offset, attempt });
        return didScroll;
      }

      const viewport = getViewport();
      const delta = rect.top - viewport.top;
      renderDebugOverlay(root, viewport, {
        restoreBlockIndex: blockIndex,
        restoreRect: rect,
        restoreDelta: delta
      });

      log("restore-attempt", {
        attempt,
        blockIndex,
        offset,
        rectTop: rect.top,
        rectBottom: rect.bottom,
        viewportTop: viewport.top,
        viewportBottom: viewport.bottom,
        delta,
        scrollTop: getScrollTop(root)
      });

      if (Math.abs(delta) <= restoreTolerancePx) {
        return didScroll;
      }

      scrollBy(root, delta);
      didScroll = true;

      await animationFrame();
      await animationFrame();
    }

    const finalRect = getBlockRect(root, blockIndex, offset);
    if (finalRect) {
      const viewport = getViewport();
      const finalDelta = finalRect.top - viewport.top;
      if (Math.abs(finalDelta) > getPixelTolerance()) {
        scrollBy(root, finalDelta);
        didScroll = true;
        await animationFrame();
        await animationFrame();
      }
    }

    return didScroll;
  }

  function getTextBlocks(root) {
    if (!root) {
      return [];
    }

    return Array.from(root.querySelectorAll("[data-block-index]"));
  }

  function getBookmarkBlockIndex(blocks, viewport) {
    for (const block of blocks) {
      const rect = block.getBoundingClientRect();

      if (rect.bottom <= viewport.top + boundaryTolerancePx || rect.top >= viewport.bottom) {
        continue;
      }

      if (rect.top < viewport.top && rect.bottom > viewport.top) {
        const visibleTailHeight = rect.bottom - viewport.top;
        if (visibleTailHeight <= getBlockLineHeight(block)) {
          continue;
        }
      }

      return numberOrNull(block.dataset.blockIndex);
    }

    return numberOrNull(blocks[0]?.dataset.blockIndex);
  }

  function getBlockLineHeight(block) {
    const style = window.getComputedStyle(block);
    const parsed = Number.parseFloat(style.lineHeight);
    if (Number.isFinite(parsed) && parsed > 0) {
      return parsed;
    }

    const fontSize = Number.parseFloat(style.fontSize);
    return Number.isFinite(fontSize) && fontSize > 0
      ? fontSize * 1.2
      : block.getBoundingClientRect().height;
  }

  function getProgressBlockIndex(root, blocks, viewport) {
    const pageEnd = root.querySelector("[data-reader-page-end]");
    if (pageEnd && isElementEndVisible(pageEnd, viewport.top, viewport.bottom)) {
      return numberOrNull(blocks[blocks.length - 1]?.dataset.blockIndex);
    }

    let lastPartiallyVisible = null;
    let lastBlockWithVisibleEnd = null;

    for (const block of blocks) {
      const rect = block.getBoundingClientRect();
      const blockIndex = numberOrNull(block.dataset.blockIndex);

      if (rect.bottom > viewport.top && rect.top < viewport.bottom) {
        lastPartiallyVisible = blockIndex;
      }

      if (rect.bottom >= viewport.top && rect.bottom <= viewport.bottom) {
        lastBlockWithVisibleEnd = blockIndex;
      }
    }

    return lastBlockWithVisibleEnd
      ?? lastPartiallyVisible
      ?? numberOrNull(blocks[blocks.length - 1]?.dataset.blockIndex);
  }

  function getBlockRect(root, blockIndex, offset) {
    const block = root?.querySelector(`[data-block-index="${blockIndex}"]`);
    if (!block) {
      return null;
    }

    if (offset <= 0) {
      return block.getBoundingClientRect();
    }

    const chunk = Array.from(block.querySelectorAll("[data-original-start][data-original-end]"))
      .find(candidate => {
        const start = Number(candidate.dataset.originalStart);
        const end = Number(candidate.dataset.originalEnd);
        return Number.isFinite(start) && Number.isFinite(end) && start <= offset && end >= offset;
      });

    return (chunk ?? block).getBoundingClientRect();
  }

  function getViewport() {
    const scrollContainer = getActiveScrollContainer();
    const containerRect = scrollContainer.getBoundingClientRect();
    const top = Math.max(containerRect.top, getReaderTopChromeBottom());
    const bottom = Math.min(containerRect.bottom, window.innerHeight - getReaderBottomChromeHeight());

    return {
      top,
      bottom
    };
  }

  function getScrollContainer(root) {
    return root?.closest?.(".app-workspace")
      ?? document.querySelector(".app-workspace--reader")
      ?? document.scrollingElement
      ?? document.documentElement;
  }

  function getActiveScrollContainer() {
    return document.querySelector(".app-workspace--reader.app-workspace--active")
      ?? document.querySelector(".app-workspace--active")
      ?? document.scrollingElement
      ?? document.documentElement;
  }

  function getScrollTop(root) {
    const scrollContainer = getScrollContainer(root);
    return scrollContainer === document.scrollingElement || scrollContainer === document.documentElement
      ? window.scrollY
      : scrollContainer.scrollTop;
  }

  function scrollBy(root, delta) {
    const scrollContainer = getScrollContainer(root);
    if (scrollContainer === document.scrollingElement || scrollContainer === document.documentElement) {
      window.scrollTo({
        top: Math.max(0, window.scrollY + delta),
        behavior: "auto"
      });
      return;
    }

    scrollContainer.scrollTo({
      top: Math.max(0, scrollContainer.scrollTop + delta),
      behavior: "auto"
    });
  }

  function getReaderTopChromeBottom() {
    const chromeBottom = Array.from(document.querySelectorAll("[data-reader-top-chrome]"))
      .map(element => element.getBoundingClientRect())
      .filter(rect => rect.width > 0 && rect.height > 0 && rect.bottom > 0 && rect.top < window.innerHeight)
      .reduce((bottom, rect) => Math.max(bottom, rect.bottom), 0);

    if (chromeBottom > 0) {
      return chromeBottom;
    }

    return getCssLength("--reader-header-offset") + getCssLength("--reader-header-visual-height");
  }

  function getReaderBottomChromeHeight() {
    const bottomChrome = document.querySelector(".app-layout__bottom");
    const rect = bottomChrome?.getBoundingClientRect();

    return rect && rect.width > 0 && rect.height > 0
      ? Math.max(0, window.innerHeight - rect.top)
      : getCssLength("--reader-bottom-offset");
  }

  function isElementEndVisible(element, viewportTop, viewportBottom) {
    const rect = element.getBoundingClientRect();
    return rect.bottom >= viewportTop && rect.bottom <= viewportBottom;
  }

  function getCssLength(variableName, fallback = 0) {
    const value = window.getComputedStyle(document.documentElement).getPropertyValue(variableName);
    if (!value) {
      return fallback;
    }

    const parsed = Number.parseFloat(value);
    return Number.isFinite(parsed) ? parsed : fallback;
  }

  function numberOrNull(value) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  function animationFrame() {
    return new Promise(resolve => window.requestAnimationFrame(resolve));
  }

  function log(eventName, data) {
    return;
  }

  function renderDebugOverlay(root, viewport, state) {
    if (!window.languageReaderDebugViewport) {
      removeDebugOverlay();
      return;
    }

    overlayRoot ??= createDebugOverlay();
    overlayRoot.replaceChildren();

    overlayRoot.appendChild(createViewportBox(viewport));
    overlayRoot.appendChild(createHorizontalLine(viewport.top, "reader-viewport-debug__line--top", "viewport top"));
    overlayRoot.appendChild(createHorizontalLine(viewport.bottom, "reader-viewport-debug__line--bottom", "viewport bottom"));

    if (state.bookmarkBlockIndex !== undefined) {
      appendBlockOverlay(root, state.bookmarkBlockIndex, "reader-viewport-debug__block--bookmark", "bookmark");
    }

    if (state.progressBlockIndex !== undefined) {
      appendBlockOverlay(root, state.progressBlockIndex, "reader-viewport-debug__block--progress", "progress");
    }

    if (state.restoreBlockIndex !== undefined) {
      appendBlockOverlay(root, state.restoreBlockIndex, "reader-viewport-debug__block--restore", "restore target");
    }

    if (state.restoreRect) {
      overlayRoot.appendChild(createRectOverlay(
        state.restoreRect,
        "reader-viewport-debug__rect--restore",
        `restore rect delta ${Math.round(state.restoreDelta ?? 0)}px`));
    }
  }

  function createDebugOverlay() {
    const element = document.createElement("div");
    element.className = "reader-viewport-debug";
    element.setAttribute("aria-hidden", "true");
    document.body.appendChild(element);
    return element;
  }

  function removeDebugOverlay() {
    overlayRoot?.remove();
    overlayRoot = null;
  }

  function createViewportBox(viewport) {
    const element = document.createElement("div");
    element.className = "reader-viewport-debug__viewport";
    element.style.top = `${viewport.top}px`;
    element.style.height = `${Math.max(0, viewport.bottom - viewport.top)}px`;
    element.title = `viewport ${Math.round(viewport.top)}-${Math.round(viewport.bottom)}`;
    return element;
  }

  function createHorizontalLine(top, className, label) {
    const element = document.createElement("div");
    element.className = `reader-viewport-debug__line ${className}`;
    element.style.top = `${top}px`;
    element.dataset.label = label;
    return element;
  }

  function appendBlockOverlay(root, blockIndex, className, label) {
    const block = root?.querySelector(`[data-block-index="${blockIndex}"]`);
    if (!block) {
      return;
    }

    overlayRoot.appendChild(createRectOverlay(
      block.getBoundingClientRect(),
      className,
      `${label} ${blockIndex}`));
  }

  function createRectOverlay(rect, className, label) {
    const element = document.createElement("div");
    element.className = `reader-viewport-debug__rect ${className}`;
    element.style.left = `${rect.left}px`;
    element.style.top = `${rect.top}px`;
    element.style.width = `${rect.width}px`;
    element.style.height = `${rect.height}px`;
    element.dataset.label = label;
    return element;
  }

  return {
    observeScroll,
    unobserveScroll,
    getProgress,
    scrollBlockIntoView
  };
})();
