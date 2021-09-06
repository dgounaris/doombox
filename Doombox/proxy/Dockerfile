FROM openresty/openresty:1.19.3.1-8-alpine

RUN apk update && apk add bash && apk add gettext

COPY docker-entrypoint.sh /
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD /usr/local/openresty/nginx/sbin/nginx -c /usr/local/openresty/nginx/conf/nginx.conf -g "daemon off;"