using System.Text;
using cms.Domain.Entities;
using cmsUserManagment.Application.Common;
using cmsUserManagment.Application.Common.Settings;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Repositories;
using cmsUserManagment.Infrastructure.Kafka;
using cmsUserManagment.Infrastructure.Persistance;
using cmsUserManagment.Infrastructure.Security;
using cmsUserManagment.Infrastructure.Services;
using cmsUserManagment.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using cmsUserManagment.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

string? redisOptionsConfiguration = builder.Configuration["Redis:Connection"];

builder.Services.AddStackExchangeRedisCache(redisOptions =>
{
    redisOptions.Configuration = redisOptionsConfiguration;
});

builder.Services.AddSingleton<IJwtTokenProvider, JwtTokenProvider>(sp =>
{
    JwtSettings jwtSettings = sp.GetRequiredService<IOptions<JwtSettings>>().Value;
    return new JwtTokenProvider(jwtSettings);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])
            ),
        };

        // Allow SignalR to pass token via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CMS User Management API", Version = "v1" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            },
        }
    );
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

builder.Services.AddScoped<HeadersManager>();
builder.Services.AddScoped<JwtDecoder>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddHttpClient<IEmailService, EmailService>();
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.EnsureCreatedAsync();

    // Existing databases may be missing tables added after the initial schema.
    // EnsureCreatedAsync does nothing on an existing DB, so create each table explicitly.
    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `Roles` (
            `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
            `Description` longtext CHARACTER SET utf8mb4 NULL,
            `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_Roles_Name` (`Name`)
        ) CHARACTER SET=utf8mb4");

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `ActivityLogs` (
            `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
            `Action` longtext CHARACTER SET utf8mb4 NOT NULL,
            `Details` longtext CHARACTER SET utf8mb4 NULL,
            `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
            `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`Id`),
            KEY `IX_ActivityLogs_UserId` (`UserId`),
            CONSTRAINT `FK_ActivityLogs_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE SET NULL
        ) CHARACTER SET=utf8mb4");

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `Notifications` (
            `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `Message` longtext CHARACTER SET utf8mb4 NOT NULL,
            `Type` longtext CHARACTER SET utf8mb4 NULL,
            `IsRead` tinyint(1) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`Id`),
            KEY `IX_Notifications_UserId` (`UserId`),
            CONSTRAINT `FK_Notifications_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4");

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `UserProfiles` (
            `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `DisplayName` varchar(100) CHARACTER SET utf8mb4 NULL,
            `AvatarUrl` longtext CHARACTER SET utf8mb4 NULL,
            `Bio` varchar(500) CHARACTER SET utf8mb4 NULL,
            `Timezone` varchar(100) CHARACTER SET utf8mb4 NULL,
            `FirstName` longtext CHARACTER SET utf8mb4 NULL,
            `LastName` longtext CHARACTER SET utf8mb4 NULL,
            `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
            `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            `UpdatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`UserId`),
            CONSTRAINT `FK_UserProfiles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4");

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `PasswordResetTokens` (
            `Id` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `Code` longtext CHARACTER SET utf8mb4 NOT NULL,
            `ExpiresAt` datetime(6) NOT NULL,
            `IsUsed` tinyint(1) NOT NULL,
            `CreatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`Id`),
            KEY `IX_PasswordResetTokens_UserId` (`UserId`),
            CONSTRAINT `FK_PasswordResetTokens_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4");

    // Backfill columns added to UserProfiles after the initial schema
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `DisplayName` varchar(100) CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `Bio` varchar(500) CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `Timezone` varchar(100) CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `FirstName` longtext CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `LastName` longtext CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL"); } catch {}
    try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE `UserProfiles` ADD COLUMN `UpdatedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000'"); } catch {}

    // Drop UserRoles if it has the old wrong schema (single Id PK instead of composite)
    var wrongPk = await db.Database
        .SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'UserRoles' AND COLUMN_NAME = 'Id'")
        .FirstOrDefaultAsync();

    if (wrongPk > 0)
        await db.Database.ExecuteSqlRawAsync("DROP TABLE `UserRoles`");

    await db.Database.ExecuteSqlRawAsync(@"
        CREATE TABLE IF NOT EXISTS `UserRoles` (
            `UserId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `RoleId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
            `AssignedAt` datetime(6) NOT NULL DEFAULT '0001-01-01 00:00:00.000000',
            PRIMARY KEY (`UserId`, `RoleId`),
            KEY `IX_UserRoles_RoleId` (`RoleId`),
            CONSTRAINT `FK_UserRoles_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles` (`Id`) ON DELETE CASCADE,
            CONSTRAINT `FK_UserRoles_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
        ) CHARACTER SET=utf8mb4");

    foreach (var (name, desc) in new[] { (AppRoles.User, "Standard user"), (AppRoles.Admin, "Administrator") })
    {
        if (!await db.Roles.AnyAsync(r => r.Name == name))
            await db.Roles.AddAsync(new Role { Name = name, Description = desc });
    }
    await db.SaveChangesAsync();
}

app.UsePathBase("/api/auth");

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowSpecificOrigins");

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<JwtValidationMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<cmsUserManagment.Infrastructure.Repositories.NotificationHub>("/hubs/notifications");
app.Run();
