using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conn_string = builder.Configuration.GetConnectionString("Sqlite");

builder.Services.AddDbContext<TodoDbContext>(option =>
    option.UseSqlite(conn_string));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Items", async (TodoDbContext db) =>
{
    return await db.TodoItems.ToListAsync();
})
    .WithName("Get_All_TODO's");

app.MapGet("/Items/{id}", async (TodoDbContext db, int id) =>
{
    var item = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
    .WithName("Get_TODO_By_ID");

app.MapPost("/Items", async (TodoDbContext db, Todo item) =>
{
    if (db.TodoItems.FirstOrDefault(x=> x.Id == item.Id) is not null)
    {
        return Results.BadRequest();
    }
    db.TodoItems.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/Items/{item.Id}", item);
})
    .WithName("Add_New_TODO");

app.MapPut("/Items/{id}", async (TodoDbContext db, int id, Todo item) =>
{
    var existeditem = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == item.Id);
    if (existeditem is null)
    {
        return Results.NotFound();
    }
    existeditem.Title = item.Title;
    existeditem.IsCompleted = item.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("Items/{id}", async (TodoDbContext db, int id) =>
{
    var todo = await db.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    db.TodoItems.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
    .WithName("Delete_By_ID");

app.UseHttpsRedirection();

app.Run();

class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {

    }
    public DbSet<Todo> TodoItems { get; set; }

}

class Todo
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

