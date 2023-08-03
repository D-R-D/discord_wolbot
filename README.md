# discord_wol

docker image: https://hub.docker.com/r/zign/discord_wolbot  
dockerイメージはまだ満足いかない   
  
docker-compose.yml example
``` 
version: "3"

services:
  wol_bot:
    image: zign/discord_wolbot:r6.0_v0.2
    container_name: wol_test
    network_mode: "host"
    deploy:
      resources:
        limits:
          memory: 256M
          cpus: '0.25'
    environment:
      - TZ=Asia/Tokyo
      - TOKEN={bot_token}
      - APPLICATION_ID={bot_application_id}
      - GUILD_ID={guild_id_0,guild_id_1,...}
      - ADMIN_ID={user_id0,user_id1,...}
    restart: unless-stopped
    volumes:
      - setting_volume:/discord_wol

volumes:
  setting_volume:
    driver: local
    driver_opts:
      type: 'none'
      device: ./discord_wol
      o: 'bind'
``` 
