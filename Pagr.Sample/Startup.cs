using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            using var scope = app.ApplicationServices.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            // TIME MEASUREMENT
            var times = new List<long>();
            app.Use(async (context, next) =>
            {
                var sw = new Stopwatch();
                sw.Start();
                await next.Invoke();
                sw.Stop();
                times.Add(sw.ElapsedMilliseconds);
                var text = $"AVG: {(int)times.Average()}ms; AT {sw.ElapsedMilliseconds}; COUNT: {times.Count()}";
                Console.WriteLine(text);
                await context.Response.WriteAsync($"<!-- {text} -->");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
