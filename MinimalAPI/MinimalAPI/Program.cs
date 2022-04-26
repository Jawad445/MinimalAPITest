using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TodoRepo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Items", ([FromServices] TodoRepo repo) =>
{
    return repo.GetALL();
});

app.MapGet("/Items/{id}", ([FromServices] TodoRepo repo, int id) =>
{
    var item = repo.GetById(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapPost("/Items", ([FromServices] TodoRepo repo, Todo item) =>
{
    if (repo.GetById(item.Id) is not null)
    {
        return Results.BadRequest();
    }
    repo.AddItem(item);
    return Results.Created($"/Items/{item.Id}", item);
});

app.MapPut("/Items/{id}", ([FromServices] TodoRepo repo, int id, Todo item) =>
{
    if (repo.GetById(id) is null)
    {
        return Results.NotFound();
    }
    repo.UpdateItem(item);
    return Results.Ok(item);
});

app.MapDelete("Items/{id}", ([FromServices] TodoRepo repo, int id) =>
{
    if (repo.GetById(id) is null)
    {
        return Results.NotFound();
    }

    repo.RemoveItem(id);
    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();

class TodoRepo
{
    private Dictionary<int, Todo> _items = new Dictionary<int, Todo>();
    public TodoRepo()
    {
        _items.Add(1, new Todo(1, "Wake up at 10", false));
        _items.Add(2, new Todo(2, "Take shower", false));
        _items.Add(3, new Todo(3, "Check Fuel", false));
        _items.Add(4, new Todo(4, "Get in office", false));
    }

    public int Count => _items.Count;
    public IEnumerable<Todo> GetALL() => _items.Values.ToList();

    public Todo? GetById(int id)
    {
        if (_items.ContainsKey(id))
        {
            return _items[id];
        }
        return null;
    }

    public void AddItem(Todo item) => _items.Add(item.Id, item);
    public void UpdateItem(Todo item) => _items[item.Id] = item;
    public void RemoveItem(int id) => _items.Remove(id);
}
internal record Todo(int Id, string Title, bool IsCompleted);