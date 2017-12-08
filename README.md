# CommandScaler

A simple command handler pattern implementation, but fully scalable with RabbitMQ (maybe more?).

This app was heavily inspired by the amazing https://github.com/jbogard/MediatR.

With this app is possible to scale your command handlers to multiple machines (or processes) by using RabbitMQ as the responsible to distribute your commands.

# Configuring

## 1. Configure `RabbitConfiguration` class on your Startup.cs

```
services.Configure<RabbitConfiguration>(Configuration.GetSection("RabbitMQ"));
```

JSON
```
{
  "RabbitMQ": {
    "Host": "127.0.0.1",
    "User": "admin",
    "Password": "123456"
  }
}
```

## 2. Add CommandScaler stuff
```
services.AddCommandScaler(new[] { typeof(YourCommand).Assembly })
        .AddRabbitMQCommandScaler()
        .ConfigureRabbitMQHandler();
```

For `AddCommandScaler` method you should pass the assemblies where your `ICommandHandler` are defined (you can have multiple here), so the lib will be able to scan and register them.

## 3. Start the CommandScaler handler
### 3.1 Start just the `IBus`

On your `Configure` method on `Startup.cs` add:
```
await app.StartCommandScaler();
```

### 3.2 Start `IBus` and `RabbitGenericHandler`
```
await app.StartRabbitMQHandler();
```
This will start the listener.

# Using

To start using, you should inject `IBus` interface when you need to send commands:

```

    public class TestController : Controller
    {
        private readonly IBus _bus;
        public TestController(IBus bus)
        {
            _bus = bus;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Test()
        {
            var result = await _bus.Send(new TestCommand { MyValue = 1 });
            return Ok(result);
        }
    }

    public class TestCommand
    {
        public int MyValue { get; set; }
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, int>
    {
        public Task<int> Handle(TestCommand command)
        {
            return command.MyValue;
        }
    }

```

On `result` you'll have the value returned from the `Handle` method of your `ICommandHandler`.

Check the samples if you have any doubt.
