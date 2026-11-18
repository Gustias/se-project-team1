import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const frontendDir = path.dirname(fileURLToPath(import.meta.url));

const certPath = path.join(frontendDir, ".cert", "localhost.pem");
const keyPath = path.join(frontendDir, ".cert", "localhost.key");

const hasCertificate = 
  fs.existsSync(certPath) && fs.existsSync(keyPath);

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    ...(hasCertificate && {
      https: {
        key: fs.readFileSync(keyPath),
        cert: fs.readFileSync(certPath),
      },
    }),
  },
});