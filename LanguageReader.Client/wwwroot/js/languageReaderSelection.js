const languageReaderRangeObservers = new Map();
const languageReaderVisibilityObservers = new Map();
const languageReaderScrollObservers = new Map();
let languageReaderRangeObserverId = 0;
let languageReaderVisibilityObserverId = 0;
let languageReaderScrollObserverId = 0;

window.languageReaderSelection = {
    getSelectedText: () => {
        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0) {
            return "";
        }

        return selection.toString();
    },

    getSelectedRange: (root) => {
        if (!root) {
            return null;
        }

        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
            return null;
        }

        const range = selection.getRangeAt(0);
        const startParagraph = getParagraphForNode(root, range.startContainer);
        const endParagraph = getParagraphForNode(root, range.endContainer);
        if (!startParagraph || !endParagraph) {
            return null;
        }

        if (startParagraph !== endParagraph) {
            return null;
        }

        const paragraphIndex = Number(startParagraph.dataset.paragraphIndex);
        const startOffset = getOriginalOffsetFromDomPosition(startParagraph, range.startContainer, range.startOffset);
        const endOffset = getOriginalOffsetFromDomPosition(endParagraph, range.endContainer, range.endOffset);
        if (paragraphIndex < 0 || endOffset <= startOffset) {
            return null;
        }

        return {
            paragraphIndex,
            startOffset,
            endOffset,
            selectedText: getOriginalTextForRange(startParagraph, startOffset, endOffset) || selection.toString()
        };
    },

    clearSelectedText: () => {
        const selection = window.getSelection();
        if (selection) {
            selection.removeAllRanges();
        }
    },

    getTextOffsetAtPoint: (root, clientX, clientY) => {
        if (!root) {
            return null;
        }

        const paragraph = findParagraphAtPoint(root, clientX, clientY);
        if (!paragraph) {
            return null;
        }

        const range = getCaretRangeFromPoint(clientX, clientY);
        let offset = 0;

        if (range && paragraph.contains(range.startContainer)) {
            offset = getOriginalOffsetFromDomPosition(paragraph, range.startContainer, range.startOffset);
        } else {
            offset = getClosestOriginalOffset(paragraph, clientX, clientY);
        }

        return {
            paragraphIndex: Number(paragraph.dataset.paragraphIndex),
            offset: clamp(offset, 0, getParagraphOriginalLength(paragraph))
        };
    },

    measureRanges: (root, ranges) => {
        if (!root || !Array.isArray(ranges)) {
            return [];
        }

        const rootRect = root.getBoundingClientRect();
        const result = [];

        for (const item of ranges) {
            const paragraph = root.querySelector(`[data-paragraph-index="${item.paragraphIndex}"]`);
            const boundary = getRangeBoundary(paragraph, item.startOffset, item.endOffset);
            if (!boundary) {
                continue;
            }

            const range = document.createRange();
            range.setStart(boundary.startNode, boundary.startOffset);
            range.setEnd(boundary.endNode, boundary.endOffset);

            for (const rect of range.getClientRects()) {
                if (rect.width <= 0 || rect.height <= 0) {
                    continue;
                }

                result.push({
                    id: item.id,
                    kind: item.kind,
                    layer: item.layer,
                    paragraphIndex: item.paragraphIndex,
                    startOffset: item.startOffset,
                    endOffset: item.endOffset,
                    displayText: item.displayText || null,
                    left: rect.left - rootRect.left + root.scrollLeft,
                    top: rect.top - rootRect.top + root.scrollTop,
                    width: rect.width,
                    height: rect.height
                });
            }

            range.detach();
        }

        return result;
    },

    scrollParagraphOffsetIntoViewIfNeeded: (root, paragraphIndex, offset) => {
        if (!root) {
            return;
        }

        const rect = getRangeRectForOffset(root, paragraphIndex, offset);
        if (!rect) {
            return;
        }

        scrollRectIntoViewIfNeeded(rect);
    },

    getFirstVisibleParagraphIndex: (root) => {
        if (!root) {
            return null;
        }

        const paragraphs = Array.from(root.querySelectorAll("[data-paragraph-index]"));
        if (paragraphs.length === 0) {
            return null;
        }

        const metrics = getReaderViewportInsets();
        const viewportTop = metrics.top + 12;
        const viewportBottom = window.innerHeight - metrics.bottom - 12;

        for (const paragraph of paragraphs) {
            const rect = paragraph.getBoundingClientRect();
            if (rect.bottom > viewportTop && rect.top < viewportBottom) {
                return Number(paragraph.dataset.paragraphIndex);
            }
        }

        return Number(paragraphs[paragraphs.length - 1].dataset.paragraphIndex);
    },

    observeRangeRoot: (root, dotNetReference) => {
        if (!root || !window.ResizeObserver) {
            return "";
        }

        const id = `reader-range-${++languageReaderRangeObserverId}`;
        let frame = 0;
        const observer = new ResizeObserver(() => {
            window.cancelAnimationFrame(frame);
            frame = window.requestAnimationFrame(() => {
                dotNetReference.invokeMethodAsync("RequestRangeMeasureAsync");
            });
        });

        observer.observe(root);
        languageReaderRangeObservers.set(id, { observer, frame });
        return id;
    },

    unobserveRangeRoot: (id) => {
        const entry = languageReaderRangeObservers.get(id);
        if (!entry) {
            return;
        }

        window.cancelAnimationFrame(entry.frame);
        entry.observer.disconnect();
        languageReaderRangeObservers.delete(id);
    },


    observeReaderScroll: (root, dotNetReference) => {
        if (!root) {
            return "";
        }

        const id = `reader-scroll-${++languageReaderScrollObserverId}`;
        let frame = 0;
        let lastParagraphIndex = null;

        const notify = () => {
            window.cancelAnimationFrame(frame);

            frame = window.requestAnimationFrame(() => {
                const paragraphIndex = window.languageReaderSelection.getFirstVisibleParagraphIndex(root);
                if (paragraphIndex === null || paragraphIndex === lastParagraphIndex) {
                    return;
                }

                lastParagraphIndex = paragraphIndex;
                dotNetReference.invokeMethodAsync("NotifyVisibleParagraphChangedAsync", paragraphIndex);
            });
        };

        window.addEventListener("scroll", notify, { passive: true });
        window.addEventListener("resize", notify, { passive: true });
        notify();

        languageReaderScrollObservers.set(id, {
            disconnect: () => {
                window.cancelAnimationFrame(frame);
                window.removeEventListener("scroll", notify);
                window.removeEventListener("resize", notify);
            }
        });

        return id;
    },

    unobserveReaderScroll: (id) => {
        const observer = languageReaderScrollObservers.get(id);
        if (!observer) {
            return;
        }

        observer.disconnect();
        languageReaderScrollObservers.delete(id);
    },

    observeVisibility: (element, dotNetReference) => {
        if (!element || !window.IntersectionObserver) {
            return "";
        }

        const id = `visibility-${++languageReaderVisibilityObserverId}`;
        const observer = new IntersectionObserver((entries) => {
            for (const entry of entries) {
                if (entry.isIntersecting) {
                    dotNetReference.invokeMethodAsync("NotifyVisibleAsync");
                }
            }
        }, { rootMargin: "320px 0px 320px 0px" });

        observer.observe(element);
        languageReaderVisibilityObservers.set(id, observer);
        return id;
    },

    unobserveVisibility: (id) => {
        const observer = languageReaderVisibilityObservers.get(id);
        if (!observer) {
            return;
        }

        observer.disconnect();
        languageReaderVisibilityObservers.delete(id);
    },

    scrollElementHorizontally: (element, delta) => {
        if (!element || !delta) {
            return;
        }

        element.scrollBy({
            left: delta,
            behavior: "smooth"
        });
    }
};

function findParagraphAtPoint(root, clientX, clientY) {
    const directElement = document.elementFromPoint(clientX, clientY);
    const directParagraph = directElement?.closest?.("[data-paragraph-index]");
    if (directParagraph && root.contains(directParagraph)) {
        return directParagraph;
    }

    const range = getCaretRangeFromPoint(clientX, clientY);
    if (range) {
        const element = range.startContainer.nodeType === Node.ELEMENT_NODE
            ? range.startContainer
            : range.startContainer.parentElement;
        const paragraph = element?.closest?.("[data-paragraph-index]");
        if (paragraph && root.contains(paragraph)) {
            return paragraph;
        }
    }

    return null;
}

function getParagraphForNode(root, node) {
    const element = node?.nodeType === Node.ELEMENT_NODE
        ? node
        : node?.parentElement;
    const paragraph = element?.closest?.("[data-paragraph-index]");
    return paragraph && root.contains(paragraph) ? paragraph : null;
}

function getCaretRangeFromPoint(clientX, clientY) {
    if (document.caretPositionFromPoint) {
        const position = document.caretPositionFromPoint(clientX, clientY);
        if (!position) {
            return null;
        }

        const range = document.createRange();
        range.setStart(position.offsetNode, position.offset);
        range.collapse(true);
        return range;
    }

    if (document.caretRangeFromPoint) {
        return document.caretRangeFromPoint(clientX, clientY);
    }

    return null;
}

function getTextNode(paragraph) {
    if (!paragraph) {
        return null;
    }

    const walker = document.createTreeWalker(paragraph, NodeFilter.SHOW_TEXT, {
        acceptNode: (node) => {
            const parent = node.parentElement;
            if (parent?.closest?.("button,.reader-fragment-action")) {
                return NodeFilter.FILTER_REJECT;
            }

            return NodeFilter.FILTER_ACCEPT;
        }
    });
    let node = walker.nextNode();
    while (node) {
        if (node.textContent.length > 0) {
            return node;
        }

        node = walker.nextNode();
    }

    return null;
}

function getRangeBoundary(paragraph, startOffset, endOffset) {
    if (!paragraph) {
        return null;
    }

    const start = findBoundary(paragraph, startOffset, false);
    const end = findBoundary(paragraph, endOffset, true);
    if (!start || !end) {
        return null;
    }

    return {
        startNode: start.node,
        startOffset: start.offset,
        endNode: end.node,
        endOffset: end.offset
    };
}

function findBoundary(paragraph, originalOffset, preferEnd) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-start][data-original-end]"));
    if (chunks.length === 0) {
        return null;
    }

    for (const chunk of chunks) {
        const start = Number(chunk.dataset.originalStart);
        const end = Number(chunk.dataset.originalEnd);
        const textNode = getTextNode(chunk);
        if (!textNode) {
            continue;
        }

        if (originalOffset >= start && originalOffset <= end) {
            if (chunk.dataset.translated === "true") {
                const originalLength = Math.max(1, end - start);
                const textLength = textNode.textContent.length;
                const ratio = clamp((originalOffset - start) / originalLength, 0, 1);

                return {
                    node: textNode,
                    offset: clamp(Math.round(ratio * textLength), 0, textLength)
                };
            }

            return {
                node: textNode,
                offset: clamp(originalOffset - start, 0, textNode.textContent.length)
            };
        }
    }

    const fallback = preferEnd ? chunks[chunks.length - 1] : chunks[0];
    const textNode = getTextNode(fallback);
    if (!textNode) {
        return null;
    }

    return {
        node: textNode,
        offset: preferEnd ? textNode.textContent.length : 0
    };
}

function getOriginalOffsetFromDomPosition(paragraph, targetNode, targetOffset) {
    const element = targetNode.nodeType === Node.TEXT_NODE
        ? targetNode.parentElement
        : targetNode;

    const chunk = element?.closest?.("[data-original-start][data-original-end]");
    if (!chunk || !paragraph.contains(chunk)) {
        return 0;
    }

    const start = Number(chunk.dataset.originalStart);
    const end = Number(chunk.dataset.originalEnd);
    if (chunk.dataset.translated === "true") {
        const textNode = getTextNode(chunk);
        const textLength = textNode?.textContent?.length || 0;
        if (!textNode || textLength === 0 || targetNode !== textNode) {
            return start;
        }

        const originalLength = Math.max(1, end - start);
        const ratio = clamp(targetOffset / textLength, 0, 1);
        return clamp(start + Math.round(ratio * originalLength), start, end);
    }

    return clamp(start + targetOffset, start, end);
}

function getOriginalTextForRange(paragraph, startOffset, endOffset) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-start][data-original-end]"));
    const parts = [];

    for (const chunk of chunks) {
        const chunkStart = Number(chunk.dataset.originalStart);
        const chunkEnd = Number(chunk.dataset.originalEnd);
        if (chunkEnd <= startOffset || chunkStart >= endOffset) {
            continue;
        }

        const text = chunk.dataset.originalText || chunk.textContent || "";
        const sliceStart = clamp(startOffset - chunkStart, 0, text.length);
        const sliceEnd = clamp(endOffset - chunkStart, sliceStart, text.length);
        parts.push(text.slice(sliceStart, sliceEnd));
    }

    return parts.join("");
}

function getClosestOriginalOffset(paragraph, clientX, clientY) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-start][data-original-end]"));
    let closestOffset = Number(chunks[0]?.dataset.originalStart || 0);
    let closestDistance = Number.POSITIVE_INFINITY;

    for (const chunk of chunks) {
        const textNode = getTextNode(chunk);
        if (!textNode) {
            continue;
        }

        const originalStart = Number(chunk.dataset.originalStart);
        const originalEnd = Number(chunk.dataset.originalEnd);
        const length = textNode.textContent.length;
        for (let offset = 0; offset <= length; offset++) {
            const range = document.createRange();
            range.setStart(textNode, offset);
            range.setEnd(textNode, offset);
            const rect = range.getBoundingClientRect();
            const distance = Math.abs(rect.left - clientX) + Math.abs(rect.top - clientY);
            range.detach();

            if (distance >= closestDistance) {
                continue;
            }

            closestDistance = distance;
            closestOffset = chunk.dataset.translated === "true"
                ? originalStart
                : clamp(originalStart + offset, originalStart, originalEnd);
        }
    }

    return closestOffset;
}

function getParagraphOriginalLength(paragraph) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-end]"));
    if (chunks.length === 0) {
        return 0;
    }

    return Math.max(...chunks.map(chunk => Number(chunk.dataset.originalEnd)));
}

function getReaderViewportInsets() {
    return {
        top: getCssLength("--reader-header-offset") + getCssLength("--reader-header-visual-height", 96),
        bottom: getCssLength("--reader-bottom-offset")
    };
}

function getCssLength(variableName, fallback = 0) {
    const value = window.getComputedStyle(document.documentElement).getPropertyValue(variableName);
    if (!value) {
        return fallback;
    }

    const element = document.createElement("div");
    element.style.position = "absolute";
    element.style.visibility = "hidden";
    element.style.height = value.trim();
    document.body.appendChild(element);
    const pixels = element.getBoundingClientRect().height;
    document.body.removeChild(element);
    return pixels || fallback;
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}

window.languageReaderSelection.scrollRangeStartIntoViewIfNeeded = (root, paragraphIndex, startOffset) => {
    if (!root) {
        return;
    }

    const rect = getRangeRectForOffset(root, paragraphIndex, startOffset);
    if (!rect) {
        return;
    }

    scrollRectIntoViewIfNeeded(rect);
};

function getRangeRectForOffset(root, paragraphIndex, startOffset) {
    const paragraph = root.querySelector(`[data-paragraph-index="${paragraphIndex}"]`);
    if (!paragraph) {
        return null;
    }

    const paragraphLength = getParagraphOriginalLength(paragraph);
    const safeStart = clamp(startOffset, 0, paragraphLength);
    const safeEnd = clamp(safeStart + 1, safeStart, paragraphLength);

    const boundary = getRangeBoundary(paragraph, safeStart, safeEnd);
    if (!boundary) {
        return paragraph.getBoundingClientRect();
    }

    const range = document.createRange();
    range.setStart(boundary.startNode, boundary.startOffset);
    range.setEnd(boundary.endNode, boundary.endOffset);

    const rect = range.getBoundingClientRect();
    range.detach();

    if (!rect || rect.height <= 0) {
        return paragraph.getBoundingClientRect();
    }

    return rect;
}

function scrollRectIntoViewIfNeeded(rect) {
    const metrics = getReaderViewportInsets();

    const viewportTop = metrics.top + 16;
    const viewportBottom = window.innerHeight - metrics.bottom - 16;

    const isVisible = rect.top >= viewportTop && rect.bottom <= viewportBottom;

    if (isVisible) {
        return;
    }

    const top = window.scrollY + rect.top - metrics.top - 24;

    window.scrollTo({
        top: Math.max(0, top),
        behavior: "smooth"
    });
}
