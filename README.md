## ⚠️Post about the implementation here

Please, read before using it.

https://blog.fals.io/2020-10-13-distributed-lock/

# Build

```
> docker-compose -f docker-compose-redis.yml build
```

# Execute

To scale the consumers to test it up, you can use the scale flag

```
> docker-compose -f docker-compose-redis.yml up --scale consumer=3
```

