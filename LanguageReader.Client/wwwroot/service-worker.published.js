self.importScripts("./service-worker-assets.js");

const cacheNamePrefix = "offline-cache-";
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [
    /\.dll$/,
    /\.pdb$/,
    /\.wasm$/,
    /\.html$/,
    /\.js$/,
    /\.json$/,
    /\.css$/,
    /\.woff$/,
    /\.woff2$/,
    /\.png$/,
    /\.jpg$/,
    /\.jpeg$/,
    /\.gif$/,
    /\.ico$/,
    /\.svg$/,
    /\.webmanifest$/
];
const offlineAssetsExclude = [
    /^service-worker\.js$/,
    /^service-worker-assets\.js$/
];
const offlineAssets = self.assetsManifest.assets
    .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
    .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)));
const offlineAssetUrls = new Set(offlineAssets.map(asset => asset.url));

self.addEventListener("install", event => event.waitUntil(onInstall()));
self.addEventListener("activate", event => event.waitUntil(onActivate()));
self.addEventListener("fetch", onFetch);

async function onInstall() {
    const cache = await caches.open(cacheName);
    const indexAsset = offlineAssets.find(asset => asset.url === "index.html");
    if (!indexAsset) {
        throw new Error("Unable to install service worker: index.html is missing from the asset manifest.");
    }

    await cacheRequiredAsset(cache, indexAsset);
    await Promise.all(offlineAssets
        .filter(asset => asset.url !== "index.html")
        .map(asset => cacheOptionalAsset(cache, asset)));

    self.skipWaiting();
}

async function onActivate() {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
    await self.clients.claim();
}

function onFetch(event) {
    if (event.request.method !== "GET") {
        return;
    }

    if (event.request.mode === "navigate") {
        event.respondWith(serveCachedIndex());
        return;
    }

    const requestUrl = new URL(event.request.url);
    if (requestUrl.origin !== self.location.origin) {
        return;
    }

    const assetUrl = getAssetUrl(requestUrl);
    if (!offlineAssetUrls.has(assetUrl)) {
        return;
    }

    event.respondWith(serveCachedAsset(event.request));
}

async function cacheRequiredAsset(cache, asset) {
    const request = createAssetRequest(asset);
    const response = await fetch(request);
    if (!isUsableResponse(response)) {
        throw new Error(`Unable to cache required asset '${asset.url}'.`);
    }

    await cache.put(request, response);
}

async function cacheOptionalAsset(cache, asset) {
    try {
        const request = createAssetRequest(asset);
        const response = await fetch(request);
        if (isUsableResponse(response)) {
            await cache.put(request, response);
        }
    } catch {
        // A single optional asset should not prevent the PWA from installing.
    }
}

async function serveCachedIndex() {
    const cache = await caches.open(cacheName);
    const cachedIndex = await cache.match("index.html");
    if (cachedIndex) {
        return cachedIndex;
    }

    return fetch("index.html", { cache: "no-cache" });
}

async function serveCachedAsset(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    return fetchWithoutRedirectedResponse(request);
}

function createAssetRequest(asset) {
    return new Request(asset.url, { integrity: asset.hash, cache: "no-cache" });
}

function getAssetUrl(requestUrl) {
    return requestUrl.pathname.replace(/^\//, "") || "index.html";
}

async function fetchWithoutRedirectedResponse(request) {
    const response = await fetch(request);
    return isUsableResponse(response) ? response : Response.error();
}

function isUsableResponse(response) {
    return response.ok && !response.redirected && response.type !== "opaqueredirect";
}
