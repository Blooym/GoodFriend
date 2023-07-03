export default {
  async fetch(request, env, ctx) {
    const response = await fetch(env.METADATA_URL, {
      headers: { "User-Agent": env.USER_AGENT },
    });

    const json = await response.json()
    if (!json.connected_clients) {
      return new Response(JSON.stringify({
        schemaVersion: 1,
        label: "connected",
        message: "Unknown",
        color: "red",
        cacheSeconds: 180,
      }));
    }

    const connected = json.connected_clients.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    return new Response(JSON.stringify({
      schemaVersion: 1,
      label: "connected",
      message: connected,
      color: "brightgreen",
      cacheSeconds: env.CACHE_SECONDS || 3600,
    }));
  },
};
