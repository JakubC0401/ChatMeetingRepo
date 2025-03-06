using ChatMeeting.Core.Domain;
using ChatMeeting.Core.Domain.Interfaces.Repositories;
using ChatMeeting.Core.Domain.Interfaces.Services;
using ChatMeeting.Infrastructure.Repositories;
using ChatMeeting.Core.Application.Services;
using Microsoft.EntityFrameworkCore;
using ChatMeeting.Core.Domain.Options;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ChatMeeting.Core.Domain.Interfaces.Producer;
using ChatMeeting.Infrastructure.Producer;

namespace ChatMeeting.API.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            AddCustomAuthentication(services, configuration);

            var connectionString = configuration.GetValue<string>("ConnectionString");

            services.AddDbContext<ChatDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettingsOption>(options => configuration.GetSection(nameof(JwtSettingsOption)).Bind(options));
            services.Configure<KafkaOption>(options => configuration.GetSection(nameof(KafkaOption)).Bind(options));
            return services;
        }

        private static void AddCustomAuthentication(IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettingsOption)).Get<JwtSettingsOption>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey)) 
            {
                throw new ArgumentException("Secret key is empty");
            }

            var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => 
            {
                options.SaveToken = true;
                options.TokenValidationParameters = GetTokenValidationParams(key);

                options.Events = GetEvents();
            });
        }

        private static JwtBearerEvents GetEvents()
        {
            return new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["token"];
                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/messageHub"))
                    {
                        
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        }

        private static TokenValidationParameters GetTokenValidationParams(byte[] key)
        {
            return new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(120)
            };
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IChatRepository, ChatRepository>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IChatService, ChatService>();
            services.AddTransient<IJwtService, JwtService>();
            services.AddSingleton(new UserConnectionService());
            services.AddSignalR();
            services.AddTransient<IKafkaProducer, KafkaProducer> ();
            return services;
        }
    }
}
