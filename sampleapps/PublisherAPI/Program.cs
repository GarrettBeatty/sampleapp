// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

using System.Text.Json;
using AWS.Messaging.Telemetry.OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PublisherAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAWSMessageBus(bus =>
{
    // To load the configuration from appsettings.json instead of the code below, uncomment this and remove the following lines.
    // bus.LoadConfigurationFromSettings(builder.Configuration);

    // Standard SQS Queue
    var mpfQueueUrl = "SQSURL";
    bus.AddSQSPublisher<ChatMessage>(mpfQueueUrl, "chatMessage");

    bus.ConfigureSerializationOptions(options =>
    {
        options.SystemTextJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy= JsonNamingPolicy.CamelCase,
        };
    });

    // Logging data messages is disabled by default to protect sensitive user data. If you want this enabled, uncomment the line below.
    // bus.EnableMessageContentLogging();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PublisherAPI"))
    .WithTracing(tracing => tracing
        .AddAWSMessagingInstrumentation()
        .AddAWSInstrumentation()
        .AddXRayTraceId()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("otelurl");
            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
