# Build

```
> docker-compose -f docker-compose-redis.yml build
```

# Execute

To scale the consumers to test it up, you can use the scale flag

```
> docker-compose -f docker-compose-redis.yml up --scale consumer=3
```