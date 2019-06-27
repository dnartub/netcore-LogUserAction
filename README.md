# netcore-LogUserAction
.Net Core Libruary for logging user's http-request data and changes in Entity

### Db Context Example (codefirst)
```csharp
[LogEntity] // log changes for this entity class
public class MyTable
{
   [Key]
   public Guid Id { get; set; }
   public  string Name { get; set; }
}

// inherit context from LogContext. This one includes LogUserAction table class
public class MyContext : LogContext
{
    // ILogUserActionService implement new instance on request liftime
    public MyContext(DbContextOptions options, ILogUserActionService logUserActionService) : base(options,logUserActionService)
    {
    }
}
```

#### Adding to DI-service

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddLogUserAction();
    
    // then add your db-context inherited from LogContext
    services.AddDbContext<MyContext>(options => ...);
}

// or it's the same
public void ConfigureServices(IServiceCollection services)
{
    // LogUserActionModel implement new instance on request liftime
    services.AddScoped<LogUserActionModel>();
    // ILogUserActionService implement new instance on request liftime
    services.AddScoped<ILogUserActionService, LogUserActionService>();

    // for find any LogContext service
    services.AddSingleton(services);

    // then add your db-context inherited from LogContext
    services.AddDbContext<MyContext>(options => ...);
}
```

#### Controler's Action

```csharp
[HttpPost]
[LogUserAction]
[DisplayName("Adding new data")] // action user-friendly discription in LogUserActionModel
public async Task<IActionResult> Post([FromBody] MyModel model)
{
    ...
}
```


#### Result Example

```json
{
  "id": "7b61b27e-51f6-42c9-a7e3-66c044afde08",
  "logTime": "2019-06-27T16:50:47.018446",
  "login": "ivanov",
  "userIP": "127.0.0.1",
  "userName": "Ivanov Ivan",
  "actionDescription": "Deleting scheduler",
  "apiResult": "success",
  "errorMessageResult": null,
  "requestDetails": {
    "methodInfo": "Web.Host.Service.Api.EntryPoints.SchedulesController.Delete(Guid id)",
    "routePath": "/api/Schedules/f303ab83-2cc1-4116-9ca0-a4ffa9e5504b",
    "headers": {
      "Connection": [
        "keep-alive"
      ],
      "Accept": [
        "application/json, text/plain, */*"
      ],
      "Accept-Encoding": [
        "gzip, deflate, br"
      ],
      "Accept-Language": [
        "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7,zh-TW;q=0.6,zh-CN;q=0.5,zh;q=0.4"
      ],
      "Host": [
        "127.0.0.1:7777"
      ],
      "Referer": [
        "http://localhost:8080/admin/entrypoints"
      ],
      "User-Agent": [
        "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36"
      ],
      "Origin": [
        "http://localhost:8080"
      ]
    },
    "routeValues": {
      "action": "Delete",
      "controller": "Schedules",
      "id": "f303ab83-2cc1-4116-9ca0-a4ffa9e5504b"
    },
    "queryValues": {
    },
    "formValues": null,
    "bodyValues": null
  },
  "databaseDetails": {
    "added": null,
    "modified": null,
    "deleted": [
      {
        "tableName": "Schedule",
        "values": {
          "Id": "f303ab83-2cc1-4116-9ca0-a4ffa9e5504b",
          "EntryPointId": "39226de9-5454-46ea-a294-d04ca474092b",
          "WeekDay": 2,
          "StartPauseTime": "11:11:11",
          "EndPauseTime": "11:11:11"
        }
      }
    ]
  }
}

```
