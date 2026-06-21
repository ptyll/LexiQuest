// LexiQuest Service Worker (Development)
const CACHE_VERSION = 'v1';
const STATIC_CACHE = `lexiquest-static-${CACHE_VERSION}`;
const API_CACHE = `lexiquest-api-${CACHE_VERSION}`;
const DATA_CACHE = `lexiquest-data-${CACHE_VERSION}`;

const STATIC_ASSETS = [
    '/',
    '/offline.html',
    '/manifest.json',
    '/icon-192.png',
    '/icon-512.png',
    '/css/app.css',
    '/css/animations.css',
    '/css/responsive.css',
    '/_content/Tempo.Blazor/css/tempo-blazor.bundled.css',
    '/LexiQuest.Blazor.Client.styles.css'
];

// Install: pre-cache static assets
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then(cache => cache.addAll(STATIC_ASSETS))
            .then(() => self.skipWaiting())
    );
});

// Activate: clean old caches
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys
                    .filter(key => key.startsWith('lexiquest-') && key !== STATIC_CACHE && key !== API_CACHE && key !== DATA_CACHE)
                    .map(key => caches.delete(key))
            )
        ).then(() => self.clients.claim())
    );
});

// Fetch strategies
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Skip non-GET requests
    if (event.request.method !== 'GET') return;

    // Network First: API calls
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirst(event.request, API_CACHE));
        return;
    }

    // Cache First: static assets (CSS, JS, images, fonts, WASM)
    if (isStaticAsset(url.pathname)) {
        event.respondWith(cacheFirst(event.request, STATIC_CACHE));
        return;
    }

    // Stale While Revalidate: everything else
    event.respondWith(staleWhileRevalidate(event.request, DATA_CACHE));
});

function isStaticAsset(pathname) {
    return /\.(css|js|wasm|png|jpg|jpeg|gif|svg|ico|woff|woff2|ttf|eot|dll|blat|dat)$/i.test(pathname);
}

async function cacheFirst(request, cacheName) {
    const cached = await caches.match(request);
    if (cached) return cached;
    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        return offlineFallback();
    }
}

async function networkFirst(request, cacheName) {
    try {
        const response = await fetch(request);
        if (response.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, response.clone());
        }
        return response;
    } catch {
        const cached = await caches.match(request);
        return cached || offlineFallback();
    }
}

async function staleWhileRevalidate(request, cacheName) {
    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    const fetchPromise = fetch(request).then(response => {
        if (response.ok) {
            cache.put(request, response.clone());
        }
        return response;
    }).catch(() => cached || offlineFallback());

    return cached || fetchPromise;
}

function offlineFallback() {
    return caches.match('/offline.html') || new Response('Offline', { status: 503, statusText: 'Service Unavailable' });
}

// Listen for update messages from the app
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
