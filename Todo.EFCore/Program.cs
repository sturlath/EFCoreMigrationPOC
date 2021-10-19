using EFCoreMigrationWeb.DataAccess;
using EFCoreMigrationWeb.Models;
using Microsoft.EntityFrameworkCore;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=todos.db";

builder.Services.AddDbContext<TodoDbContext>(options =>
{
    options.UseSqlServer(connectionString);
}).AddDatabaseDeveloperPageExceptionFilter();

// or use SqlLite
//builder.Services.AddSqlite<TodoContext>(connectionString)
//                .AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await EnsureDb(app.Services, app.Logger);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.MapGet("/error", () => Results.Problem("An error occurred.", statusCode: 500))
   .ExcludeFromDescription();

app.MapSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello World!")
    .WithName("Hello");

app.MapGet("/hello", () => new { Hello = "World" })
    .WithName("HelloObject");

app.MapGet("/todos", async (TodoDbContext db) =>
    await db.Todos.ToListAsync())
    .WithName("GetAllTodos");

app.MapGet("/todos/complete", async (TodoDbContext db) =>
    await db.Todos.Where(t => t.IsComplete).ToListAsync())
    .WithName("GetCompleteTodos");

app.MapGet("/todos/incomplete", async (TodoDbContext db) =>
    await db.Todos.Where(t => !t.IsComplete).ToListAsync())
    .WithName("GetIncompleteTodos");

app.MapGet("/todos/{id}", async (int id, TodoDbContext db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound())
    .WithName("GetTodoById")
    .Produces<Todo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapPost("/todos", async (Todo todo, TodoDbContext db) =>
    {
        if (!MiniValidator.TryValidate(todo, out var errors))
            return Results.ValidationProblem(errors);

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return Results.Created($"/todos/{todo.Id}", todo);
    })
    .WithName("CreateTodo")
    .ProducesValidationProblem()
    .Produces<Todo>(StatusCodes.Status201Created);

app.MapPut("/todos/{id}", async (int id, Todo inputTodo, TodoDbContext db) =>
    {
        if (!MiniValidator.TryValidate(inputTodo, out var errors))
            return Results.ValidationProblem(errors);

        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return Results.NotFound();

        todo.Title = inputTodo.Title;
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();

        return Results.NoContent();
    })
    .WithName("UpdateTodo")
    .ProducesValidationProblem()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.MapPut("/todos/{id}/mark-complete", async (int id, TodoDbContext db) =>
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            todo.IsComplete = true;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        else
        {
            return Results.NotFound();
        }
    })
    .WithName("MarkComplete")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.MapPut("/todos/{id}/mark-incomplete", async (int id, TodoDbContext db) =>
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            todo.IsComplete = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }
        else
        {
            return Results.NotFound();
        }
    })
    .WithName("MarkIncomplete")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.MapDelete("/todos/{id}", async (int id, TodoDbContext db) =>
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return Results.Ok(todo);
        }

        return Results.NotFound();
    })
    .WithName("DeleteTodo")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.MapDelete("/todos/delete-all", async (TodoDbContext db) =>
    Results.Ok(await db.Database.ExecuteSqlRawAsync("DELETE FROM Todos")))
    .WithName("DeleteAll")
    .Produces<int>(StatusCodes.Status200OK);

app.Run();

async Task EnsureDb(IServiceProvider services, ILogger logger)
{
    logger.LogInformation("Ensuring database exists and is up to date at connection string '{connectionString}'", connectionString);

    using var db = services.CreateScope().ServiceProvider.GetRequiredService<TodoDbContext>();
    //await db.Database.MigrateAsync(); //<-- We are not using this for this example!
}