#!/usr/bin/env bash
set -eu

cp /mnt/doombox/location.conf.template /etc/nginx/conf.d/location.conf.template
cp /mnt/doombox/default.conf.template /etc/nginx/conf.d/default.conf.template

locations=($PROXY_LOCATIONS)
delays=($PROXY_LOCATION_DELAYS)
for i in "${!locations[@]}"; do
    export PROXY_LOCATION="${locations[i]}"
    export PROXY_DELAY="${delays[i]}"
    envsubst '${PROXY_DELAY} ${PROXY_LOCATION}' < /etc/nginx/conf.d/location.conf.template >> /etc/nginx/conf.d/location.conf.resolved
    echo "" >> /etc/nginx/conf.d/location.conf.resolved
done

export PROXY_LOCATIONS_RESOLVED=$(cat /etc/nginx/conf.d/location.conf.resolved)
envsubst '${PROXY_LOCATIONS_RESOLVED} ${PROXY_TARGET}' < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

exec "$@"