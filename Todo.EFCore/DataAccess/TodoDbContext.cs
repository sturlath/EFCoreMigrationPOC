using EFCoreMigrationWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreMigrationWeb.DataAccess
{
    class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options) { }

        public DbSet<Todo> Todos => Set<Todo>();
    }
}
