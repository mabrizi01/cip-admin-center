import type { NextConfig } from "next";

// The browser talks to /cip-api/*; Next.js forwards it to the CIP Admin Center API.
// CIP_API_BASE_URL is server-side configuration (see .env.example).
const apiBaseUrl = process.env.CIP_API_BASE_URL ?? "http://localhost:8080";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return [{ source: "/cip-api/:path*", destination: `${apiBaseUrl}/:path*` }];
  },
};

export default nextConfig;
