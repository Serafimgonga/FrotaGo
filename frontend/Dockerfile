# ==========================
# Build Angular
# ==========================
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm install

COPY . .

RUN NODE_OPTIONS="--max-old-space-size=512" npm run build -- --configuration production

# ==========================
# Nginx
# ==========================
FROM nginx:alpine

COPY nginx/default.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist/*/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
