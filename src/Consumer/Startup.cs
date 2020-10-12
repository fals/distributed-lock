using System.Threading;
using Consumer.HostedServices;
using Consumer.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using StackExchange.Redis;

namespace Consumer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            Thread.Sleep(20000);
            services.Configure<ConsumerSettings>(Configuration);
            services.AddControllers();

            services.AddSingleton<ConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<ConsumerSettings>>().Value;
                var configuration = ConfigurationOptions.Parse(settings.ConnectionStrings.RedisCache, true);

                configuration.ResolveDns = true;

                return ConnectionMultiplexer.Connect(configuration);
            });

            services.AddSingleton<ZooKeeper>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<ConsumerSettings>>().Value;
                var logger = sp.GetRequiredService<ILogger<LogWatcher>>();
                var watcher = new LogWatcher(logger);
                return new ZooKeeper(settings.ConnectionStrings.ZooKeeper, 30000, watcher);
            });


            services.AddSingleton<MongoDb>();
            //services.AddScoped<IDistributedLock, RedisCacheDistributedLock>();
            services.AddScoped<IDistributedLock, ZooKeeperDistributedLock>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddHostedService<ProcessTransactionsHostedService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
