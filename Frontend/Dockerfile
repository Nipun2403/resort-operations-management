# ─── Build Stage ───
FROM node:22-alpine AS build
WORKDIR /src

# Copy package files for dependency caching
COPY Frontend/package*.json ./
RUN npm install --legacy-peer-deps

# Copy source and build
COPY Frontend/ .
RUN npm run build

# ─── Runtime Stage (Nginx) ───
FROM nginx:alpine AS final

# Copy built assets
COPY --from=build /src/dist/Frontend/browser /usr/share/nginx/html

# Copy nginx configuration
COPY Frontend/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]