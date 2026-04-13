# Build da aplicação Angular
FROM node:20 AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

# Servir com Nginx
FROM nginx:alpine
COPY --from=build /app/dist/questionario-frontend/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
