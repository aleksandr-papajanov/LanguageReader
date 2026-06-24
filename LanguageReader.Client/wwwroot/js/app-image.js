window.appImage = {
  async renderHalftone(canvas, src, options = {}) {
    if (!canvas || !src) {
      return false;
    }

    try {
      const img = new Image();
      img.crossOrigin = "anonymous";
      img.src = src;

      await img.decode();

      const rect = canvas.getBoundingClientRect();
      const cssSize = Math.max(rect.width || 180, rect.height || 180, 96);
      const dpr = Math.min(window.devicePixelRatio || 1, 2);
      const size = Math.max(220, Math.min(options.size ?? Math.round(cssSize * dpr), 900));
      const referenceSize = options.referenceSize ?? 180;
      const scale = Math.max(0.75, Math.min(size / referenceSize, 2.4));
      const cell = Math.max(4, Math.round((options.cell ?? 5) * scale));
      const blackCell = Math.max(cell, Math.round((options.blackCell ?? 7) * scale));

      canvas.width = size;
      canvas.height = size;

      const source = document.createElement("canvas");
      source.width = size;
      source.height = size;

      const sctx = source.getContext("2d", { willReadFrequently: true });
      drawImageCover(sctx, img, size, size);

      const data = sctx.getImageData(0, 0, size, size).data;
      const ctx = canvas.getContext("2d");

      ctx.fillStyle = "#f7f1e5";
      ctx.fillRect(0, 0, size, size);

      // Keep the original photo readable. The dots are texture, not the whole image.
      ctx.save();
      ctx.globalAlpha = options.underlayOpacity ?? 0.88;
      ctx.filter = `grayscale(0.38) sepia(0.05) contrast(1.18) brightness(0.96) blur(${options.underlayBlur ?? 0.18}px)`;
      drawImageCover(ctx, img, size, size);
      ctx.restore();

      drawDotLayer(ctx, data, size, blackCell, "rgb(18,16,14)", 45, 0.48);
      drawDotLayer(ctx, data, size, cell, "rgb(122,96,54)", 12, 0.16);

      // Subtle paper tooth and print scanlines.
      ctx.save();
      ctx.globalAlpha = 0.055;
      ctx.fillStyle = "#fff8e8";
      for (let y = 0; y < size; y += Math.max(3, Math.round(3 * scale))) {
        ctx.fillRect(0, y, size, 1);
      }
      ctx.restore();

      return true;
    } catch {
      return false;
    }
  },

  dispose() {
  }
};

function drawImageCover(ctx, img, width, height) {
  const imageRatio = img.naturalWidth / img.naturalHeight;
  const targetRatio = width / height;
  let sourceWidth = img.naturalWidth;
  let sourceHeight = img.naturalHeight;
  let sourceX = 0;
  let sourceY = 0;

  if (imageRatio > targetRatio) {
    sourceWidth = img.naturalHeight * targetRatio;
    sourceX = (img.naturalWidth - sourceWidth) / 2;
  } else {
    sourceHeight = img.naturalWidth / targetRatio;
    sourceY = (img.naturalHeight - sourceHeight) / 2;
  }

  ctx.drawImage(img, sourceX, sourceY, sourceWidth, sourceHeight, 0, 0, width, height);
}

function drawDotLayer(ctx, data, size, cell, color, angleDeg, alpha) {
  const angle = angleDeg * Math.PI / 180;
  const cos = Math.cos(angle);
  const sin = Math.sin(angle);
  const center = size / 2;
  const span = size * 1.45;

  ctx.save();
  ctx.fillStyle = color;
  ctx.globalAlpha = alpha;

  for (let gy = -span; gy <= span; gy += cell) {
    for (let gx = -span; gx <= span; gx += cell) {
      const x = center + gx * cos - gy * sin;
      const y = center + gx * sin + gy * cos;

      if (x < 0 || y < 0 || x >= size || y >= size) {
        continue;
      }

      const i = (Math.floor(y) * size + Math.floor(x)) * 4;
      const r = data[i];
      const g = data[i + 1];
      const b = data[i + 2];
      const luminance = (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255;
      const ink = Math.pow(1 - luminance, 1.35);
      const radius = ink * cell * 0.34;

      if (radius < 0.35) {
        continue;
      }

      ctx.beginPath();
      ctx.arc(x, y, radius, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  ctx.restore();
}
