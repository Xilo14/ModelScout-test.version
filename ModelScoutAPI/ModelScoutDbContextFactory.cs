using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace ModelScoutAPI {
    public class ModelScoutDbContextFactory : IDesignTimeDbContextFactory<ModelScoutDbContext> {
        public ModelScoutDbContext CreateDbContext(string[] args) {
            var optionsBuilder = new DbContextOptionsBuilder<ModelScoutDbContext>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();


            // получаем строку подключения из файла appsettings.json
            var connectionString = configuration.GetSection("DefaultConnection").Value;
            optionsBuilder.UseNpgsql(connectionString);

            return new ModelScoutDbContext(optionsBuilder.Options);
        }
    }
}
