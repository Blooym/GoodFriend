export default {
  async fetch(request, env, ctx) {
    const response = await fetch(env.METADATA_URL, {
      headers: { 
        "User-Agent": env.USER_AGENT,
        "X-Client-Key": env.CLIENT_KEY,
      },
    });

    const json = await response.json()
    if (!json.connections) {
      return new Response(JSON.stringify({
        schemaVersion: 1,
        label: "Connected",
        message: "Unknown",
        color: "red",
        cacheSeconds: 180,
      }));
    }

    const connectionsTotal = Object.values(json.connections).reduce((a, b) => a + b, 0);
    return new Response(JSON.stringify({
      schemaVersion: 1,
      label: "Connected",
      message: connectionsTotal.toString(),
      color: "brightgreen",
      cacheSeconds: env.CACHE_SECONDS || 3600,
    }));
  },
};
