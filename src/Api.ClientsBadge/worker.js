const cache = caches.default;

export default {
  async fetch(request, env, ctx) {
    const cachedResponse = await cache.match(request);
    if (cachedResponse) {
      return cachedResponse;
    }

    const metadata = await fetch(env.METADATA_URL, {
      headers: {
        "User-Agent": env.USER_AGENT,
        "X-Client-Key": env.CLIENT_KEY,
      },
    });

    const json = await metadata.json()
    if (!json.connections) {
      const response = new Response(JSON.stringify({
        schemaVersion: 1,
        label: "Connected",
        message: "Unknown",
        color: "red",
        cacheSeconds: 180,
      }));
      response.headers.set("Cache-Control", "max-age=180, must-revalidate, public");
      await cache.put(request, response.clone());
    }

    const connectionsTotal = Object.values(json.connections).reduce((a, b) => a + b, 0);
    const response = new Response(JSON.stringify({
      schemaVersion: 1,
      label: "Connected",
      message: parseInt(connectionsTotal).toLocaleString("en"),
      color: "brightgreen",
      cacheSeconds: env.CACHE_SECONDS || 3600,
    }));
    response.headers.set("Cache-Control", `max-age=${env.CACHE_SECONDS || 3600}, must-revalidate, public`);
    await cache.put(request, response.clone());
    return response;
  },
};
