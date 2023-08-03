#!/bin/bash

sed -i "s|{token}|$TOKEN|g" discord_wolbot.dll.config
sed -i "s|{application_id}|$APPLICATION_ID|g" discord_wolbot.dll.config
sed -i "s|{guild_id}|$GUILD_ID|g" discord_wolbot.dll.config
sed -i "s|{admin_id}|$ADMIN_ID|g" discord_wolbot.dll.config

sed -i "s|sed|#sed|g" Boot.sh

exec "$@"