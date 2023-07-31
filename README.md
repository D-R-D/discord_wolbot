# discord_wol

docker image: https://hub.docker.com/r/zign/discord_wolbot  
dockerイメージはまだ未完成  
docker-compose upしたら一回落としてボリューム内の[App.config](/discord_wol/App.config)と[machine.json](/discord_wol/config/machine.json)に設定を書き込む必要あり  
  
docker-compose.yml example
``` 
version: "3"

services:
  woltest:
    image: discord_wol:r6.0_v0.2
    container_name: woltest
    network_mode: "host"
    deploy:
      resources:
        limits:
          memory: 256M
          cpus: '0.25'
    environment:
      TZ: "Asia/Tokyo"
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
