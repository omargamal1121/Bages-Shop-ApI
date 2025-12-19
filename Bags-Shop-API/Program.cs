using Bags_Shop_API.ContextFile;
using Bags_Shop_API.Middleware;
using Bags_Shop_API.Models;
using Bags_Shop_API.Repo;
using Bags_Shop_API.Services.Behaviors;
using Bags_Shop_API.Services.ProductServices;
using Bags_Shop_API.Services.ProductServices.ProductFactories;
using Bags_Shop_API.UnitOfWorkService;
using Bags_Shop_API.Services.PaymentServices;
using Bags_Shop_API.Services.OrderServices;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Services.AccountServices;
using Microsoft.AspNetCore.Identity.UI.Services;
using CloudinaryDotNet;
using Hangfire;
using Hangfire.SqlServer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bags_Shop_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Monster") ?? throw new Exception("Can't find string connection")));
            
            builder.Services.AddScoped<IMainRepository<Product>, MainRepository<Product>>();
            builder.Services.AddScoped<IMainRepository<Image>, MainRepository<Image>>();
            builder.Services.AddScoped<IMainRepository<Collection>, MainRepository<Collection>>();
            builder.Services.AddScoped<IMainRepository<Order>, MainRepository<Order>>();
            builder.Services.AddScoped<IMainRepository<OrderItem>, MainRepository<OrderItem>>();
            builder.Services.AddScoped<IMainRepository<Discount>, MainRepository<Discount>>();
            builder.Services.AddScoped<IMainRepository<Payment>, MainRepository<Payment>>();
            builder.Services.AddScoped<IMainRepository<PaymentWebhook>, MainRepository<PaymentWebhook>>();

            builder.Services.AddTransient<IProductMapper, ProductMapper>();
            builder.Services.AddTransient<IProductFactory, ProductFactory>();

            builder.Services.AddScoped<IOrderServices, OrderServices>();
            builder.Services.AddScoped<IPaymentServices, PaymentServices>();
            builder.Services.AddScoped<IPayMobServices, PayMobServices>();
            builder.Services.AddScoped<IPaymentProcessor, PayMobServices>();
            builder.Services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
            builder.Services.AddScoped<IErrorNotificationService, ErrorNotificationService>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IAccountEmailService, AccountEmailService>();
            
            var cloudinaryAccount = new Account(
                builder.Configuration["CloudinarySettings:CloudName"],
                builder.Configuration["CloudinarySettings:ApiKey"],
                builder.Configuration["CloudinarySettings:ApiSecret"]
            );
            builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));
            
            builder.Services.AddMediatR(typeof(Program).Assembly);
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

            builder.Services.AddSingleton<ICacheTokenProvider, CacheTokenProvider>();

            builder.Services.AddScoped<Bags_Shop_API.Services.ImageServices.IImageValidationService, Bags_Shop_API.Services.ImageServices.ImageValidationService>();
            builder.Services.AddScoped<Bags_Shop_API.Services.ImageServices.ICloudinaryImageService, Bags_Shop_API.Services.ImageServices.CloudinaryImageService>();

            builder.Services.AddScoped<Bags_Shop_API.Services.DiscountServices.IDiscountMapper, Bags_Shop_API.Services.DiscountServices.DiscountMapper>();
            builder.Services.AddScoped<Bags_Shop_API.Services.DiscountServices.IDiscountFactory, Bags_Shop_API.Services.DiscountServices.DiscountFactory>();
    
            builder.Services.AddScoped<Bags_Shop_API.Services.CollectionServices.ICollectionMapper, Bags_Shop_API.Services.CollectionServices.CollectionMapper>();
            builder.Services.AddScoped<Bags_Shop_API.Services.CollectionServices.ICollectionFactory, Bags_Shop_API.Services.CollectionServices.CollectionFactory>();
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<Bags_Shop_API.Services.DiscountServices.Jobs.IDiscountSchedulerService, Bags_Shop_API.Services.DiscountServices.Jobs.DiscountSchedulerService>();
            builder.Services.AddScoped<Bags_Shop_API.Services.DiscountServices.Jobs.DiscountJobService>();
            
            builder.Services.AddHangfire(config =>
                config.UseStorage(
                    new SqlServerStorage(
                        builder.Configuration.GetConnectionString("LocalSql"),
                        new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(10),
                        }
                    )
                )
            );

            builder.Services.AddHangfireServer();
            builder.Logging.AddConsole();
            
            var app = builder.Build();
            app.UseCors("AllowFrontend");
            app.UseHttpsRedirection();

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
            app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
          
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
          
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter(builder.Configuration) }
            });

            app.MapControllers();

            app.Run();
        }
    }
}
