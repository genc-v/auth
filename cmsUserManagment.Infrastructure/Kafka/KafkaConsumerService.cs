using System.Text.Json;

using cms.Domain.Entities;

using Confluent.Kafka;

using cmsUserManagment.Application.DTO;
using cmsUserManagment.Application.Interfaces;
using cmsUserManagment.Infrastructure.Persistance;
using cmsUserManagment.Infrastructure.Repositories;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace cmsUserManagment.Infrastructure.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string _topic;
    private readonly ConsumerConfig _consumerConfig;

    public KafkaConsumerService(
        IServiceScopeFactory scopeFactory,
        IHubContext<NotificationHub> hubContext,
        IConfiguration configuration,
        ILogger<KafkaConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
        _topic = configuration["Kafka:FilesUploadedTopic"] ?? "files.uploaded";

        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:ConsumerGroup"] ?? "cms-user-management",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() => Consume(stoppingToken), stoppingToken);
    }

    private void Consume(CancellationToken stoppingToken)
    {
        using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started, listening on topic {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    ConsumeResult<Ignore, string> result = consumer.Consume(stoppingToken);

                    if (result?.Message?.Value == null) continue;

                    ProcessMessage(result.Message.Value).GetAwaiter().GetResult();
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing Kafka message");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessMessage(string messageValue)
    {
        FileUploadedEvent? evt;
        try
        {
            evt = JsonSerializer.Deserialize<FileUploadedEvent>(messageValue,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Kafka message: {Message}", messageValue);
            return;
        }

        if (evt == null || string.IsNullOrEmpty(evt.AssetId)) return;

        // assetId format: "{userId}/{entryId}/{timestamp}-{filename}"
        string[] parts = evt.AssetId.Split('/');
        if (parts.Length < 2 || !Guid.TryParse(parts[0], out Guid userId)) return;

        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        UserProfile? profile = await db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            if (!await db.Users.AnyAsync(u => u.Id == userId)) return;
            profile = new UserProfile { UserId = userId };
            db.UserProfiles.Add(profile);
        }

        profile.AvatarUrl = evt.Url;
        profile.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        _logger.LogInformation("Updated AvatarUrl for user {UserId} → {Url}", userId, evt.Url);

        // Push real-time notification to the user
        INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.CreateNotification(new CreateNotificationDto
        {
            UserId = userId,
            Message = "Your profile picture has been updated.",
            Type = "profile_picture"
        });
    }
}
