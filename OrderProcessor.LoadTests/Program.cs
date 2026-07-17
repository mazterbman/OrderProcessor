using System.Text;
using System.Text.Json;
using NBomber.CSharp;
using NBomber.Http.CSharp;

// 1. HTTP-клиент — им будем стрелять
using var httpClient = new HttpClient();

// 2. Один выстрел (scenario) — что делает виртуальный пользователь
var scenario = Scenario.Create("create_order", async context =>
    {
        // тело запроса — JSON с items
        var body = JsonSerializer.Serialize(new { items = new[] { new { productId = Guid.NewGuid(), count = 1, price = 100 } } });
        var request = Http.CreateRequest("POST", "http://localhost:8080/api/orders")
            .WithBody(new StringContent(body, Encoding.UTF8, "application/json"));

        var response = await Http.Send(httpClient, request);

        return response; // NBomber сам поймёт успех/провал по статусу
    })
    .WithoutWarmUp()
    .WithLoadSimulations(
        // 3. профиль нагрузки -- см. вопрос 2
        Simulation.RampingInject(rate: 100,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(30))
    );

// 4. запуск
NBomberRunner
    .RegisterScenarios(scenario)
    .Run();