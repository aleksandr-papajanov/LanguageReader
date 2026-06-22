const appImageRenderers = new WeakMap();

window.appImage = {
    async renderHalftone(canvas, src, options = {}) {
        if (!canvas || !src) {
            return false;
        }

        try {
            disposeRenderer(canvas);
            const img = await loadImage(src);
            const render = () => renderHalftoneCanvas(canvas, img, options);

            render();

            if (window.ResizeObserver) {
                let frame = 0;
                const observer = new ResizeObserver(() => {
                    window.cancelAnimationFrame(frame);
                    frame = window.requestAnimationFrame(render);
                });

                observer.observe(canvas);
                appImageRenderers.set(canvas, {
                    dispose: () => {
                        window.cancelAnimationFrame(frame);
                        observer.disconnect();
                    }
                });
            }

            return true;
        } catch {
            return false;
        }
    },

    dispose(canvas) {
        disposeRenderer(canvas);
    }
};

function disposeRenderer(canvas) {
    const renderer = appImageRenderers.get(canvas);
    if (!renderer) {
        return;
    }

    renderer.dispose();
    appImageRenderers.delete(canvas);
}

function renderHalftoneCanvas(canvas, img, options) {
    const sizing = resolveHalftoneSizing(canvas, options);
    const width = sizing.width;
    const height = sizing.height;
    const cell = sizing.cell;
    const blackCell = sizing.blackCell;

    if (canvas.width !== width || canvas.height !== height) {
        canvas.width = width;
        canvas.height = height;
    }

    const source = document.createElement("canvas");
    source.width = width;
    source.height = height;

    const sctx = source.getContext("2d", { willReadFrequently: true });
    drawImageCover(sctx, img, width, height);

    const data = sctx.getImageData(0, 0, width, height).data;
    const ctx = canvas.getContext("2d");

    ctx.clearRect(0, 0, width, height);
    ctx.fillStyle = options.paperColor || "#f7f4ed";
    ctx.fillRect(0, 0, width, height);

    drawOriginalUnderlay(ctx, source, width, height, options);

    const cmykAt = (x, y) => {
        const safeX = clamp(Math.floor(x), 0, width - 1);
        const safeY = clamp(Math.floor(y), 0, height - 1);
        const i = (safeY * width + safeX) * 4;

        const r = data[i] / 255;
        const g = data[i + 1] / 255;
        const b = data[i + 2] / 255;

        const k = 1 - Math.max(r, g, b);
        const d = 1 - k || 0.00001;

        return {
            c: Math.min(1, ((1 - r - k) / d) * 0.95),
            m: Math.min(1, ((1 - g - k) / d) * 0.95),
            y: Math.min(1, ((1 - b - k) / d) * 0.9),
            k: Math.min(1, k * 1.25)
        };
    };

    const drawLayer = (channel, color, angleDeg, layerCell, alpha) => {
        const angle = angleDeg * Math.PI / 180;
        const cos = Math.cos(angle);
        const sin = Math.sin(angle);
        const centerX = width / 2;
        const centerY = height / 2;
        const span = Math.max(width, height) * 1.6;

        ctx.fillStyle = color;
        ctx.globalAlpha = alpha;

        for (let gy = -span; gy <= span; gy += layerCell) {
            for (let gx = -span; gx <= span; gx += layerCell) {
                const x = centerX + gx * cos - gy * sin;
                const y = centerY + gx * sin + gy * cos;

                if (x < 0 || y < 0 || x >= width || y >= height) {
                    continue;
                }

                const amount = Math.pow(cmykAt(x, y)[channel], 0.82);
                const radius = amount * layerCell * 0.5;

                if (radius < Math.max(0.35, layerCell * 0.08)) {
                    continue;
                }

                ctx.beginPath();
                ctx.arc(x, y, radius, 0, Math.PI * 2);
                ctx.fill();
            }
        }

        ctx.globalAlpha = 1;
    };

    drawLayer("y", "rgb(245,209,45)", 0, cell, 0.34);
    drawLayer("c", "rgb(0,150,185)", 15, cell, 0.72);
    drawLayer("m", "rgb(205,45,125)", 75, cell, 0.68);
    drawLayer("k", "rgb(20,18,16)", 45, blackCell, 0.78);
}

function drawOriginalUnderlay(ctx, source, width, height, options) {
    const opacity = clamp(Number(options.underlayOpacity ?? 0.28), 0, 1);
    if (opacity <= 0) {
        return;
    }

    const blur = Math.max(0, Number(options.underlayBlur ?? 0.8));
    ctx.save();
    ctx.globalAlpha = opacity;
    ctx.filter = `saturate(0.55) contrast(0.88) brightness(1.08) blur(${blur}px)`;
    ctx.drawImage(source, 0, 0, width, height);
    ctx.restore();
}

function resolveHalftoneSizing(canvas, options) {
    const rect = getCanvasRenderRect(canvas);
    const pixelRatio = Math.min(window.devicePixelRatio || 1, 2);
    const maxTextureSize = options.size ?? 900;
    const cssWidth = Math.max(rect.width, 1);
    const cssHeight = Math.max(rect.height, 1);
    const textureScale = Math.min(pixelRatio, maxTextureSize / Math.max(cssWidth, cssHeight));
    const width = Math.max(1, Math.round(cssWidth * textureScale));
    const height = Math.max(1, Math.round(cssHeight * textureScale));
    const scale = width / cssWidth;
    const referenceSize = options.referenceSize ?? 220;
    const shortSide = Math.min(cssWidth, cssHeight);
    const responsiveScale = clamp(Math.sqrt(shortSide / referenceSize), 0.72, 1.28);
    const visibleCell = clamp((options.cell ?? 6) * responsiveScale, 3.5, 8);
    const visibleBlackCell = clamp((options.blackCell ?? 5) * responsiveScale, 3, 7);

    return {
        width,
        height,
        cell: visibleCell * scale,
        blackCell: visibleBlackCell * scale
    };
}

function getCanvasRenderRect(canvas) {
    const rect = canvas.getBoundingClientRect();
    if (rect.width >= 2 && rect.height >= 2) {
        return rect;
    }

    const parentRect = canvas.parentElement?.getBoundingClientRect();
    if (parentRect && parentRect.width >= 2 && parentRect.height >= 2) {
        return parentRect;
    }

    return { width: 180, height: 180 };
}

function loadImage(src) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = "anonymous";
        img.decoding = "async";
        img.onload = () => resolve(img);
        img.onerror = reject;
        img.src = src;
    });
}

function drawImageCover(ctx, img, targetWidth, targetHeight) {
    const sourceWidth = img.naturalWidth || img.width;
    const sourceHeight = img.naturalHeight || img.height;
    const sourceRatio = sourceWidth / sourceHeight;
    const targetRatio = targetWidth / targetHeight;

    let sx = 0;
    let sy = 0;
    let sw = sourceWidth;
    let sh = sourceHeight;

    if (sourceRatio > targetRatio) {
        sw = sourceHeight * targetRatio;
        sx = (sourceWidth - sw) / 2;
    } else if (sourceRatio < targetRatio) {
        sh = sourceWidth / targetRatio;
        sy = (sourceHeight - sh) / 2;
    }

    ctx.drawImage(img, sx, sy, sw, sh, 0, 0, targetWidth, targetHeight);
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), max);
}
