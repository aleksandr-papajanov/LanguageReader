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

self.addEventListener("install", event => event.waitUntil(onInstall()));
self.addEventListener("activate", event => event.waitUntil(onActivate()));
self.addEventListener("fetch", onFetch);

async function onInstall() {
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: "no-cache" }));

    const cache = await caches.open(cacheName);
    await Promise.all(assetsRequests.map(request => cacheAsset(cache, request)));
}

async function onActivate() {
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
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

    event.respondWith(serveCachedAsset(event.request));
}

async function cacheAsset(cache, request) {
    try {
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

    return fetchWithoutRedirectedResponse("index.html");
}

async function serveCachedAsset(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    return fetchWithoutRedirectedResponse(request);
}

async function fetchWithoutRedirectedResponse(request) {
    const response = await fetch(request);
    return isUsableResponse(response) ? response : Response.error();
}

function isUsableResponse(response) {
    return response.ok && !response.redirected && response.type !== "opaqueredirect";
}
