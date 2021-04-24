using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pagr.Models;
using Pagr.Sample.Entities;
using Pagr.Sample.Services;
using Pagr.Services;

namespace Pagr.Sample
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(opts =>
            {
                opts.EnableEndpointRouting = false;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=.\\pagr.db"));

            services.Configure<PagrOptions>(Configuration.GetSection("Pagr"));

            services.AddScoped<IPagrCustomSortMethods, PagrCustomSortMethods>();
            services.AddScoped<IPagrCustomFilterMethods, PagrCustomFilterMethods>();
            services.AddScoped<IPagrProcessor, ApplicationPagrProcessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            PrepareDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        private static void PrepareDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        }
    }
}
