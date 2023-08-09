using WTW.SecurityRepository.Interfaces;
using WTW.SecurityRepository.Repositories;
using WTW.SecurityServices.Interfaces;
using WTW.SecurityServices.Services;

namespace WTW.SecurityAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddSingleton<IBaseRepository, BaseRepository>();
            builder.Services.AddSingleton<ISecurityRepository, SecurityRepository.Repositories.SecurityRepository>();
            builder.Services.AddSingleton<IBloomFilter, BloomFilter>();
            builder.Services.AddScoped<ISecurityService, SecurityService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors();

            app.MapControllers();

            Task.Run(() => app.Services.GetService<IBloomFilter>().LoadData());

            app.Run();
        }
    }
}