# How to run sample on Docker

## 1. Build `docker-compose`

```
docker-compose build
```

## 2. Start just RabbitMQ

```
docker-compose up -d rabbitmq
```

## 3. Start CommandScaler

```
docker-compose up -d
```

### 3.1 If you want to scale your handlers

```
docker-compose up -d --scale commandscaler.sample.handler=N
```

Where `N` is the total of replicas of the Handler

## 4. Navigate to the API

```
http://localhost:8080/test
```

When it executes, should return: I've delayed Xms. My return message is: Y

## Considerations

If you check the logs of the handler
```
docker-compose logs -f commandscaler.sample.handler
```

When it receives a command you will see:
```
commandscaler.sample.handler_2  | info: CommandScaler.RabbitMQ.Handler.Handler.RabbitGenericHandler[0]
commandscaler.sample.handler_2  |       Received command.
```

Where the `_2` is the replica of the handler that received the command, you'll see different numbers everytime. RabbitMQ is dispatching messages to every Handler running.

