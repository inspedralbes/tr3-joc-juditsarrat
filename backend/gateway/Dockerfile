FROM nginx:alpine

# Copiem la nostra configuració personalitzada
COPY nginx.conf /etc/nginx/nginx.conf

# Exposem el port 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
