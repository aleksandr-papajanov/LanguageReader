const languageReaderRangeObservers = new Map();
const languageReaderVisibilityObservers = new Map();
const languageReaderScrollObservers = new Map();
const languageReaderSelectionObservers = new Map();
let languageReaderRangeObserverId = 0;
let languageReaderVisibilityObserverId = 0;
let languageReaderScrollObserverId = 0;
let languageReaderSelectionObserverId = 0;

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

        if (rangeIntersectsTranslatedDisplay(root, range)) {
            return null;
        }

        const blockIndex = Number(startParagraph.dataset.blockIndex);
        const startOffset = getOriginalOffsetFromDomPosition(startParagraph, range.startContainer, range.startOffset);
        const endOffset = getOriginalOffsetFromDomPosition(endParagraph, range.endContainer, range.endOffset);
        if (blockIndex < 0 || endOffset <= startOffset) {
            return null;
        }

        const expandedRange = expandOriginalRangeToWordBoundaries(startParagraph, startOffset, endOffset);
        const trimmedRange = trimOriginalRange(startParagraph, expandedRange.startOffset, expandedRange.endOffset);
        if (!trimmedRange) {
            return null;
        }

        return {
            blockIndex,
            startOffset: trimmedRange.startOffset,
            endOffset: trimmedRange.endOffset,
            selectedText: trimmedRange.selectedText
        };
    },

    clearSelectedText: () => {
        clearNativeSelection();
    },

    measureFloatingPanels: (root) => {
        const marker = root?.querySelector(".reader-fragment-action--measure");
        const selectionActions = root?.querySelector(".reader-selection-actions--measure");
        const translationPopup = root?.querySelector(".reader-fragment-actions--measure");
        const markerGap = getSelectMarkerGap(root);
        const markerRect = marker?.getBoundingClientRect();
        const selectionRect = selectionActions?.getBoundingClientRect();
        const translationRect = translationPopup?.getBoundingClientRect();

        return {
            markerShift: (markerRect?.width || 0) + markerGap,
            markerHeight: markerRect?.height || 0,
            selectionActionsWidth: selectionRect?.width || 0,
            selectionActionsHeight: selectionRect?.height || 0,
            translationPopupWidth: translationRect?.width || 0,
            translationPopupHeight: translationRect?.height || 0
        };
    },

    getTextOffsetAtPoint: (root, clientX, clientY) => {
        if (!root) {
            return null;
        }

        const paragraph = findParagraphAtPoint(root, clientX, clientY);
        if (!paragraph) {
            return null;
        }

        if (isTranslatedDisplayPoint(paragraph, clientX, clientY)) {
            return null;
        }

        const range = getCaretRangeFromPoint(clientX, clientY);
        let offset = 0;

        if (range && paragraph.contains(range.startContainer)) {
            if (isInsideTranslatedDisplay(paragraph, range.startContainer)) {
                return null;
            }

            offset = getOriginalOffsetFromDomPosition(paragraph, range.startContainer, range.startOffset);
        } else {
            offset = getClosestOriginalOffset(paragraph, clientX, clientY);
        }

        return {
            blockIndex: Number(paragraph.dataset.blockIndex),
            offset: clamp(offset, 0, getParagraphOriginalLength(paragraph))
        };
    },

    measureRanges: (root, ranges) => {
        if (!root || !Array.isArray(ranges)) {
            return [];
        }

        const rootRect = root.getBoundingClientRect();
        const excludedRects = getOverlayExclusionRects(root);
        const result = [];

        for (const item of ranges) {
            const paragraph = root.querySelector(`[data-block-index="${item.blockIndex}"]`);
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

                for (const visibleRect of subtractExcludedRects(rect, excludedRects)) {
                    result.push({
                        id: item.id,
                        kind: item.kind,
                        layer: item.layer,
                        blockIndex: item.blockIndex,
                        startOffset: item.startOffset,
                        endOffset: item.endOffset,
                        displayText: item.displayText || null,
                        left: visibleRect.left - rootRect.left + root.scrollLeft,
                        top: visibleRect.top - rootRect.top + root.scrollTop,
                        width: visibleRect.width,
                        height: visibleRect.height
                    });
                }
            }

            range.detach();
        }

        return result;
    },

    scrollParagraphOffsetIntoViewIfNeeded: (root, blockIndex, offset) => {
        if (!root) {
            return;
        }

        const rect = getRangeRectForOffset(root, blockIndex, offset);
        if (!rect) {
            return;
        }

        scrollRectIntoViewIfNeeded(rect);
    },

    getReaderProgressBlockIndex: (root) => {
        if (!root) {
            return null;
        }

        const paragraphs = Array.from(root.querySelectorAll("[data-block-index]"));
        if (paragraphs.length === 0) {
            return null;
        }

        const metrics = getReaderViewportInsets();
        const viewportTop = metrics.top + 12;
        const viewportBottom = window.innerHeight - metrics.bottom - 12;
        const pageEnd = root.querySelector("[data-reader-page-end]");
        if (pageEnd && isElementEndVisible(pageEnd, viewportTop, viewportBottom + 24)) {
            return Number(paragraphs[paragraphs.length - 1].dataset.blockIndex);
        }

        let lastPartiallyVisible = null;
        let lastParagraphWithVisibleEnd = null;

        for (const paragraph of paragraphs) {
            const rect = paragraph.getBoundingClientRect();
            if (rect.bottom > viewportTop && rect.top < viewportBottom) {
                lastPartiallyVisible = Number(paragraph.dataset.blockIndex);
            }

            if (rect.bottom >= viewportTop && rect.bottom <= viewportBottom) {
                lastParagraphWithVisibleEnd = Number(paragraph.dataset.blockIndex);
            }
        }

        return lastParagraphWithVisibleEnd ?? lastPartiallyVisible ?? Number(paragraphs[paragraphs.length - 1].dataset.blockIndex);
    },

    getFirstVisibleBlockIndex: (root) => {
        return window.languageReaderSelection.getReaderProgressBlockIndex(root);
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
        let lastBlockIndex = null;

        const notify = () => {
            window.cancelAnimationFrame(frame);

            frame = window.requestAnimationFrame(() => {
                const blockIndex = window.languageReaderSelection.getReaderProgressBlockIndex(root);
                if (blockIndex === null || blockIndex === lastBlockIndex) {
                    return;
                }

                lastBlockIndex = blockIndex;
                dotNetReference.invokeMethodAsync("NotifyVisibleBlockChangedAsync", blockIndex);
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

    observeNativeSelection: (root, dotNetReference) => {
        if (!root) {
            return "";
        }

        const id = `reader-selection-${++languageReaderSelectionObserverId}`;
        let timeout = 0;
        let lastSignature = "";
        let pointerSelection = null;
        let pointerSelectionTimer = 0;
        let lastPointerPreviewAt = 0;
        let lastPointerPreviewSignature = "";
        let suppressClickUntil = 0;

        const emitSelectedRange = (selectedRange, force = false) => {
            if (!selectedRange || !selectedRange.selectedText?.trim()) {
                return false;
            }

            const signature = `${selectedRange.blockIndex}:${selectedRange.startOffset}:${selectedRange.endOffset}:${selectedRange.selectedText}`;
            if (!force && signature === lastSignature) {
                return false;
            }

            lastSignature = signature;
            dotNetReference.invokeMethodAsync(
                "NotifyNativeSelectionChangedAsync",
                selectedRange.blockIndex,
                selectedRange.startOffset,
                selectedRange.endOffset,
                selectedRange.selectedText);

            return true;
        };

        const notify = () => {
            window.clearTimeout(timeout);

            timeout = window.setTimeout(() => {
                if (pointerSelection?.isSelecting || usesCustomTouchSelection()) {
                    clearNativeSelection();
                    return;
                }

                const selection = window.getSelection();
                if (!selectionIntersectsRoot(root, selection)) {
                    return;
                }

                const selectedRange = window.languageReaderSelection.getSelectedRange(root);
                if (!selectedRange || !selectedRange.selectedText?.trim()) {
                    dotNetReference.invokeMethodAsync("NotifyNativeSelectionRejectedAsync");
                    return;
                }

                emitSelectedRange(selectedRange);
            }, 220);
        };

        const startPointerSelection = (event) => {
            if (!isCustomSelectionPointer(event) || isReaderInteractiveTarget(event.target)) {
                return;
            }

            const hit = window.languageReaderSelection.getTextOffsetAtPoint(root, event.clientX, event.clientY);
            if (!hit) {
                return;
            }

            pointerSelection = {
                pointerId: event.pointerId,
                startHit: hit,
                latestHit: hit,
                startX: event.clientX,
                startY: event.clientY,
                isSelecting: false
            };

            window.clearTimeout(pointerSelectionTimer);
            pointerSelectionTimer = window.setTimeout(() => {
                if (!pointerSelection || pointerSelection.pointerId !== event.pointerId) {
                    return;
                }

                pointerSelection.isSelecting = true;
                root.classList.add("reader-page--touch-selecting");
                setPointerCaptureSafely(root, event.pointerId);
                clearNativeSelection();
            }, 120);
        };

        const updatePointerSelection = (event) => {
            if (!pointerSelection || pointerSelection.pointerId !== event.pointerId) {
                return;
            }

            const deltaX = event.clientX - pointerSelection.startX;
            const deltaY = event.clientY - pointerSelection.startY;
            const distance = Math.hypot(deltaX, deltaY);
            if (!pointerSelection.isSelecting && distance > 8 && Math.abs(deltaX) > Math.abs(deltaY) * 0.55) {
                pointerSelection.isSelecting = true;
                root.classList.add("reader-page--touch-selecting");
                setPointerCaptureSafely(root, event.pointerId);
                clearNativeSelection();
            }

            if (!pointerSelection.isSelecting) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            clearNativeSelection();

            const hit = window.languageReaderSelection.getTextOffsetAtPoint(root, event.clientX, event.clientY);
            if (hit && hit.blockIndex === pointerSelection.startHit.blockIndex) {
                pointerSelection.latestHit = hit;
            }

            previewPointerSelection(root, dotNetReference, pointerSelection);
        };

        const finishPointerSelection = (event) => {
            if (!pointerSelection || pointerSelection.pointerId !== event.pointerId) {
                return;
            }

            window.clearTimeout(pointerSelectionTimer);
            const completedSelection = pointerSelection;
            pointerSelection = null;
            root.classList.remove("reader-page--touch-selecting");
            releasePointerCaptureSafely(root, event.pointerId);

            if (!completedSelection.isSelecting) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
            clearNativeSelection();

            const selectedRange = getPointerSelectedRange(root, completedSelection.startHit, completedSelection.latestHit);
            if (!selectedRange || !selectedRange.selectedText?.trim()) {
                dotNetReference.invokeMethodAsync("NotifyNativeSelectionRejectedAsync");
                return;
            }

            suppressClickUntil = Date.now() + 350;
            emitSelectedRange(selectedRange, true);
        };

        const previewPointerSelection = (root, dotNetReference, selection) => {
            const now = Date.now();
            if (now - lastPointerPreviewAt < 70) {
                return;
            }

            const selectedRange = getPointerSelectedRange(root, selection.startHit, selection.latestHit);
            if (!selectedRange || !selectedRange.selectedText?.trim()) {
                return;
            }

            const signature = `${selectedRange.blockIndex}:${selectedRange.startOffset}:${selectedRange.endOffset}:${selectedRange.selectedText}`;
            if (signature === lastPointerPreviewSignature) {
                return;
            }

            lastPointerPreviewAt = now;
            lastPointerPreviewSignature = signature;
            emitSelectedRange(selectedRange);
        };

        const cancelPointerSelection = (event) => {
            if (!pointerSelection || pointerSelection.pointerId !== event.pointerId) {
                return;
            }

            window.clearTimeout(pointerSelectionTimer);
            pointerSelection = null;
            root.classList.remove("reader-page--touch-selecting");
            releasePointerCaptureSafely(root, event.pointerId);
            clearNativeSelection();
        };

        const suppressSyntheticClick = (event) => {
            if (Date.now() >= suppressClickUntil) {
                return;
            }

            event.preventDefault();
            event.stopPropagation();
        };

        document.addEventListener("selectionchange", notify);
        root.addEventListener("pointerdown", startPointerSelection, { passive: true });
        root.addEventListener("pointermove", updatePointerSelection, { passive: false });
        root.addEventListener("pointerup", finishPointerSelection, { passive: false });
        root.addEventListener("pointercancel", cancelPointerSelection, { passive: true });
        root.addEventListener("click", suppressSyntheticClick, true);

        languageReaderSelectionObservers.set(id, {
            disconnect: () => {
                window.clearTimeout(timeout);
                window.clearTimeout(pointerSelectionTimer);
                document.removeEventListener("selectionchange", notify);
                root.removeEventListener("pointerdown", startPointerSelection);
                root.removeEventListener("pointermove", updatePointerSelection);
                root.removeEventListener("pointerup", finishPointerSelection);
                root.removeEventListener("pointercancel", cancelPointerSelection);
                root.removeEventListener("click", suppressSyntheticClick, true);
            }
        });

        return id;
    },

    unobserveNativeSelection: (id) => {
        const observer = languageReaderSelectionObservers.get(id);
        if (!observer) {
            return;
        }

        observer.disconnect();
        languageReaderSelectionObservers.delete(id);
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

function clearNativeSelection() {
    const selection = window.getSelection();
    if (!selection) {
        return;
    }

    selection.removeAllRanges();
    selection.empty?.();

    window.requestAnimationFrame(() => {
        const currentSelection = window.getSelection();
        currentSelection?.removeAllRanges();
        currentSelection?.empty?.();
    });
}

function usesCustomTouchSelection() {
    return window.matchMedia?.("(pointer: coarse)")?.matches || navigator.maxTouchPoints > 0;
}

function isCustomSelectionPointer(event) {
    return event.pointerType === "touch"
        || event.pointerType === "pen"
        || (!event.pointerType && usesCustomTouchSelection());
}

function isReaderInteractiveTarget(target) {
    const element = target?.nodeType === Node.TEXT_NODE
        ? target.parentElement
        : target;

    return !!element?.closest?.(
        "button,a,input,textarea,select,[contenteditable='true'],[contenteditable='plaintext-only']," +
        ".reader-fragment-action,.reader-fragment-actions,.reader-selection-actions");
}

function getPointerSelectedRange(root, startHit, endHit) {
    if (!root || !startHit || !endHit || startHit.blockIndex !== endHit.blockIndex) {
        return null;
    }

    const startOffset = Math.min(startHit.offset, endHit.offset);
    const endOffset = Math.max(startHit.offset, endHit.offset);
    if (endOffset <= startOffset) {
        return null;
    }

    const paragraph = root.querySelector(`[data-block-index="${startHit.blockIndex}"]`);
    if (!paragraph) {
        return null;
    }

    const expandedRange = expandOriginalRangeToWordBoundaries(paragraph, startOffset, endOffset);
    const trimmedRange = trimOriginalRange(paragraph, expandedRange.startOffset, expandedRange.endOffset);
    if (!trimmedRange) {
        return null;
    }

    return {
        blockIndex: startHit.blockIndex,
        startOffset: trimmedRange.startOffset,
        endOffset: trimmedRange.endOffset,
        selectedText: trimmedRange.selectedText
    };
}

function setPointerCaptureSafely(element, pointerId) {
    try {
        element.setPointerCapture?.(pointerId);
    } catch {
        // Some mobile browsers can release the pointer before capture is available.
    }
}

function releasePointerCaptureSafely(element, pointerId) {
    try {
        element.releasePointerCapture?.(pointerId);
    } catch {
        // Ignore stale pointer captures; the selection state has already been reset.
    }
}

function selectionIntersectsRoot(root, selection) {
    if (!root || !selection || selection.rangeCount === 0 || selection.isCollapsed) {
        return false;
    }

    for (let index = 0; index < selection.rangeCount; index++) {
        if (rangeIntersectsNode(selection.getRangeAt(index), root)) {
            return true;
        }
    }

    return false;
}

function findParagraphAtPoint(root, clientX, clientY) {
    const directElement = document.elementFromPoint(clientX, clientY);
    if (directElement?.closest?.("button,a,.reader-fragment-action,.reader-fragment-actions,.reader-selection-actions")) {
        return null;
    }

    const directParagraph = directElement?.closest?.("[data-block-index]");
    if (directParagraph && root.contains(directParagraph)) {
        return isPointInsideParagraphText(directParagraph, clientX, clientY)
            ? directParagraph
            : null;
    }

    const range = getCaretRangeFromPoint(clientX, clientY);
    if (range) {
        const element = range.startContainer.nodeType === Node.ELEMENT_NODE
            ? range.startContainer
            : range.startContainer.parentElement;
        const paragraph = element?.closest?.("[data-block-index]");
        if (paragraph
            && root.contains(paragraph)
            && isPointInsideElement(paragraph, clientX, clientY)
            && isPointInsideParagraphText(paragraph, clientX, clientY)) {
            return paragraph;
        }
    }

    return null;
}

function getSelectMarkerGap(root) {
    if (!root) {
        return 0;
    }

    const value = window.getComputedStyle(root).getPropertyValue("--reader-translation-marker-gap");
    return Number.parseFloat(value) || 0;
}

function isPointInsideParagraphText(paragraph, clientX, clientY) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-start][data-original-end]"));

    for (const chunk of chunks) {
        if (chunk.dataset.translated === "true") {
            continue;
        }

        const textNode = getTextNode(chunk);
        if (!textNode) {
            continue;
        }

        const range = document.createRange();
        range.selectNodeContents(textNode);
        const rects = Array.from(range.getClientRects());
        range.detach();

        if (rects.some(rect => isPointInsideTextRect(rect, clientX, clientY))) {
            return true;
        }
    }

    return false;
}

function isPointInsideTextRect(rect, clientX, clientY) {
    const horizontalTolerance = 3;
    const verticalTolerance = 3;

    return clientX >= rect.left - horizontalTolerance
        && clientX <= rect.right + horizontalTolerance
        && clientY >= rect.top - verticalTolerance
        && clientY <= rect.bottom + verticalTolerance;
}

function getOverlayExclusionRects(root) {
    return Array.from(root.querySelectorAll(".reader-translated-fragment__marker"))
        .flatMap(marker => Array.from(marker.getClientRects()))
        .filter(rect => rect.width > 0 && rect.height > 0)
        .map(rect => ({
            left: rect.left,
            top: rect.top,
            right: rect.right,
            bottom: rect.bottom,
            width: rect.width,
            height: rect.height
        }));
}

function subtractExcludedRects(rect, excludedRects) {
    let visibleRects = [{
        left: rect.left,
        top: rect.top,
        right: rect.right,
        bottom: rect.bottom,
        width: rect.width,
        height: rect.height
    }];

    for (const excluded of excludedRects) {
        visibleRects = visibleRects.flatMap(visible => subtractSingleRect(visible, excluded));
        if (visibleRects.length === 0) {
            break;
        }
    }

    return visibleRects;
}

function subtractSingleRect(rect, excluded) {
    const overlapLeft = Math.max(rect.left, excluded.left);
    const overlapRight = Math.min(rect.right, excluded.right);
    const overlapTop = Math.max(rect.top, excluded.top);
    const overlapBottom = Math.min(rect.bottom, excluded.bottom);

    if (overlapRight <= overlapLeft || overlapBottom <= overlapTop) {
        return [rect];
    }

    const parts = [];
    addVisibleRect(parts, rect.left, rect.top, overlapLeft, rect.bottom);
    addVisibleRect(parts, overlapRight, rect.top, rect.right, rect.bottom);

    return parts;
}

function addVisibleRect(parts, left, top, right, bottom) {
    const width = right - left;
    const height = bottom - top;
    if (width <= 0.5 || height <= 0.5) {
        return;
    }

    parts.push({ left, top, right, bottom, width, height });
}

function isPointInsideElement(element, clientX, clientY) {
    const rect = element.getBoundingClientRect();
    return clientX >= rect.left
        && clientX <= rect.right
        && clientY >= rect.top
        && clientY <= rect.bottom;
}

function isElementEndVisible(element, viewportTop, viewportBottom) {
    const rect = element.getBoundingClientRect();
    return rect.bottom >= viewportTop && rect.bottom <= viewportBottom;
}

function getParagraphForNode(root, node) {
    const element = node?.nodeType === Node.ELEMENT_NODE
        ? node
        : node?.parentElement;
    const paragraph = element?.closest?.("[data-block-index]");
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

function rangeIntersectsTranslatedDisplay(root, range) {
    const translatedChunks = Array.from(root.querySelectorAll("[data-original-start][data-original-end][data-translated=\"true\"]"));

    for (const chunk of translatedChunks) {
        if (rangeIntersectsNode(range, chunk)) {
            return true;
        }
    }

    return false;
}

function rangeIntersectsNode(range, node) {
    const nodeRange = document.createRange();
    nodeRange.selectNodeContents(node);

    try {
        const startsBeforeNodeEnds = range.compareBoundaryPoints(Range.START_TO_END, nodeRange) < 0;
        const endsAfterNodeStarts = range.compareBoundaryPoints(Range.END_TO_START, nodeRange) > 0;
        return startsBeforeNodeEnds && endsAfterNodeStarts;
    } finally {
        nodeRange.detach();
    }
}

function isTranslatedDisplayPoint(paragraph, clientX, clientY) {
    const element = document.elementFromPoint(clientX, clientY);
    if (!element) {
        return false;
    }

    const chunk = element.closest?.("[data-original-start][data-original-end]");
    return !!chunk && paragraph.contains(chunk) && chunk.dataset.translated === "true";
}

function isInsideTranslatedDisplay(paragraph, node) {
    const element = node.nodeType === Node.TEXT_NODE
        ? node.parentElement
        : node;

    const chunk = element?.closest?.("[data-original-start][data-original-end]");
    return !!chunk && paragraph.contains(chunk) && chunk.dataset.translated === "true";
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

function trimOriginalRange(paragraph, startOffset, endOffset) {
    const selectedText = getOriginalTextForRange(paragraph, startOffset, endOffset);
    if (!selectedText) {
        return null;
    }

    const leadingWhitespace = selectedText.match(/^\s*/)?.[0].length ?? 0;
    const trailingWhitespace = selectedText.match(/\s*$/)?.[0].length ?? 0;
    const trimmedText = selectedText.slice(leadingWhitespace, selectedText.length - trailingWhitespace);
    if (!trimmedText) {
        return null;
    }

    return {
        startOffset: startOffset + leadingWhitespace,
        endOffset: endOffset - trailingWhitespace,
        selectedText: trimmedText
    };
}

function expandOriginalRangeToWordBoundaries(paragraph, startOffset, endOffset) {
    const paragraphLength = getParagraphOriginalLength(paragraph);
    let safeStart = clamp(startOffset, 0, paragraphLength);
    let safeEnd = clamp(endOffset, safeStart, paragraphLength);
    const text = getOriginalTextForRange(paragraph, 0, paragraphLength);

    while (safeStart > 0 && isWordCharacter(text[safeStart - 1]) && isWordCharacter(text[safeStart])) {
        safeStart--;
    }

    while (safeEnd < paragraphLength && isWordCharacter(text[safeEnd - 1]) && isWordCharacter(text[safeEnd])) {
        safeEnd++;
    }

    return {
        startOffset: safeStart,
        endOffset: safeEnd
    };
}

function isWordCharacter(value) {
    return !!value && /[\p{L}\p{N}]/u.test(value);
}

function getClosestOriginalOffset(paragraph, clientX, clientY) {
    const chunks = Array.from(paragraph.querySelectorAll("[data-original-start][data-original-end]"))
        .filter(chunk => chunk.dataset.translated !== "true");
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

window.languageReaderSelection.scrollRangeStartIntoViewIfNeeded = (root, blockIndex, startOffset) => {
    if (!root) {
        return;
    }

    const rect = getRangeRectForOffset(root, blockIndex, startOffset);
    if (!rect) {
        return;
    }

    scrollRectIntoViewIfNeeded(rect);
};

function getRangeRectForOffset(root, blockIndex, startOffset) {
    const paragraph = root.querySelector(`[data-block-index="${blockIndex}"]`);
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

(function installGlobalSelectionGuard() {
    const controlSelector = "input, textarea, select, [contenteditable='true'], [contenteditable='plaintext-only']";
    const readerTextSelector = ".reader-page [data-block-index]";

    function isAllowedSelectionTarget(target) {
        const element = target?.nodeType === Node.TEXT_NODE
            ? target.parentElement
            : target;

        if (element?.closest?.(controlSelector)) {
            return true;
        }

        return !usesCustomTouchSelection() && !!element?.closest?.(readerTextSelector);
    }

    document.addEventListener("selectstart", (event) => {
        if (isAllowedSelectionTarget(event.target)) {
            return;
        }

        event.preventDefault();
    }, true);

    document.addEventListener("selectionchange", () => {
        const selection = window.getSelection();
        if (!selection || selection.rangeCount === 0 || selection.isCollapsed) {
            return;
        }

        if (isAllowedSelectionTarget(selection.anchorNode) && isAllowedSelectionTarget(selection.focusNode)) {
            return;
        }

        clearNativeSelection();
    });

    document.addEventListener("contextmenu", (event) => {
        if (event.target?.closest?.(controlSelector)) {
            return;
        }

        event.preventDefault();
    }, true);
})();
