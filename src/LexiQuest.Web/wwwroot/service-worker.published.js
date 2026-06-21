// LexiQuest Service Worker (Production)
// Integrates with Blazor WASM asset manifest for aggressive caching

self.importScripts('./service-worker-assets.js');

const CACHE_VERSION = 'v1';
const STATIC_CACHE = `lexiquest-static-${CACHE_VERSION}`;
const API_CACHE = `lexiquest-api-${CACHE_VERSION}`;

// Install: cache all assets from the manifest
self.addEventListener('install', event => {
    event.waitUntil(
        (async () => {
            const cache = await caches.open(STATIC_CACHE);
            const assetsToCache = self.assetsManifest.assets
                .filter(asset => asset.hash)
                .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

            for (const request of assetsToCache) {
                try {
                    await cache.add(request);
                } catch (err) {
                    console.warn(`Failed to cache: ${request.url}`, err);
                }
            }

            // Also cache the offline fallback
            try {
                await cache.add(new Request('/offline.html', { cache: 'no-cache' }));
            } catch { /* offline.html is optional */ }

            await self.skipWaiting();
        })()
    );
});

// Activate: clean old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        (async () => {
            const keys = await caches.keys();
            await Promise.all(
                keys
                    .filter(key => key.startsWith('lexiquest-') && key !== STATIC_CACHE && key !== API_CACHE)
                    .map(key => caches.delete(key))
            );
            await self.clients.claim();
        })()
    );
});

// Fetch strategies
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    if (event.request.method !== 'GET') return;

    // Network First: API calls
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirst(event.request));
        return;
    }

    // Cache First with manifest validation: all other requests
    event.respondWith(cacheFirstWithManifest(event.request));
});

async function cacheFirstWithManifest(request) {
    // Check if this URL is in the asset manifest
    const url = new URL(request.url);
    const isManifestAsset = self.assetsManifest.assets.some(
        asset => url.pathname.endsWith(asset.url)
    );

    if (isManifestAsset) {
        const cached = await caches.match(request);
        if (cached) return cached;
    }

    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(STATIC_CACHE);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const cached = await caches.match(request);
        return cached || offlineFallback();
    }
}

async function networkFirst(request) {
    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(API_CACHE);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const cached = await caches.match(request);
        return cached || new Response(JSON.stringify({ error: 'offline' }), {
            status: 503,
            headers: { 'Content-Type': 'application/json' }
        });
    }
}

function offlineFallback() {
    return caches.match('/offline.html') || new Response('Offline', { status: 503, statusText: 'Service Unavailable' });
}

// Listen for update messages
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

self.addEventListener('push', event => {
    var payload = {};
    if (event.data) {
        try {
            payload = event.data.json();
        } catch {
            payload = { title: 'LexiQuest', body: event.data.text() };
        }
    }

    var title = payload.title || 'LexiQuest';
    var options = {
        body: payload.body || payload.message || '',
        icon: '/icon-192.png',
        badge: '/icon-192.png',
        data: {
            url: payload.url || payload.actionUrl || '/dashboard'
        }
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', event => {
    event.notification.close();

    var targetUrl = event.notification.data && event.notification.data.url
        ? event.notification.data.url
        : '/dashboard';

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true })
            .then(clientList => {
                for (const client of clientList) {
                    if ('focus' in client) {
                        client.navigate(targetUrl);
                        return client.focus();
                    }
                }

                if (clients.openWindow) {
                    return clients.openWindow(targetUrl);
                }
            })
    );
});
