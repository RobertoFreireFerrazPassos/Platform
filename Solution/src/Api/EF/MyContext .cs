using Microsoft.EntityFrameworkCore;
using TaskRunner.Domain;

namespace Api.EF
{
    public class MyContext : DbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options) { }
        public DbSet<JsFile> JsFile { get; set; }
    }
}
