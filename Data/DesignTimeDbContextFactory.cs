using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CafeteriaUNAL.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CafeteriaContext>
    {
        public CafeteriaContext CreateDbContext(string[] args)
        {
            // Construir la configuración
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            // Crear el DbContextOptionsBuilder
            var builder = new DbContextOptionsBuilder<CafeteriaContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlite(connectionString);

            return new CafeteriaContext(builder.Options);
        }
    }
}