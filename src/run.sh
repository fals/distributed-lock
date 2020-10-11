$(docker ps -q)
docker stop $(docker ps -aq)
docker rm $(docker ps -aq)
docker volume rm $(docker volume ls --filter dangling=true -q)
docker-compose build
docker-compose up --scale consumer=3 --no-recreate