# Api Clients Badge

This folder houses the code that runs as a Cloudflare Worker that allows for creating a badge that displays the number of clients connected to the API.

## Usage

```
[![Connected Users](https://img.shields.io/endpoint?url=WORKER_URL/&label=Connected%20Users)](#)
```

## Environment Variables

| Variable         | Description                                                           |
| ---------------- | --------------------------------------------------------------------- |
| `METADATA_URL`   | The URL pointing to the metadata endpoint of the API                  |
| `USER_AGENT`     | The useragent to use when fetching the metadata from the API          |
| `CACHE_SECONDS`  | How long the response from the API will be cached for on the badge    |
