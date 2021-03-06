﻿using System;
using System.Linq;
using DurableTask.Core;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static DurableTask.TestHelpers;

namespace DurableTask.DependencyInjection.Tests.Extensions
{
    public class TaskHubWorkerBuilderExtensionsTests
    {
        [Fact]
        public void WithOrchestrationService_ArgumentNull()
            => RunTestException<ArgumentNullException>(
                _ => TaskHubWorkerBuilderExtensions.WithOrchestrationService(null, Mock.Of<IOrchestrationService>()));

        [Fact]
        public void WithOrchestration_ServiceIsSet()
            => RunTest(
                builder =>
                {
                    IOrchestrationService service = Mock.Of<IOrchestrationService>();
                    ITaskHubWorkerBuilder returned = builder.WithOrchestrationService(service);

                    builder.Should().NotBeNull();
                    builder.Should().BeSameAs(returned);
                    return service;
                },
                (mock, service) =>
                {
                    mock.VerifySet(m => m.OrchestrationService = service, Times.Once);
                });

        [Fact]
        public void AddClient_ClientAdded()
            => RunTest(
                builder => builder.AddClient(),
                (mock, builder) =>
                {
                    builder.Should().Be(mock.Object);
                    builder.Services.Should().HaveCount(1);
                    builder.Services.Single().Lifetime.Should().Be(ServiceLifetime.Singleton);
                    builder.Services.Single().ServiceType.Should().Be(typeof(TaskHubClient));
                });

        [Fact]
        public void ClientFactory_NoOrchestration()
            => RunTestException<InvalidOperationException>(
                builder =>
                {
                    builder.AddClient();
                    IServiceProvider provider = builder.Services.BuildServiceProvider();
                    provider.GetService<TaskHubClient>();
                });

        [Fact]
        public void ClientFactory_NotOrchestrationClient()
            => RunTestException<InvalidOperationException>(
                builder =>
                {
                    var mockOrchestrationService = new Mock<IOrchestrationService>();
                    Mock.Get(builder)
                        .Setup(m => m.OrchestrationService)
                        .Returns(mockOrchestrationService.Object);
                    builder.AddClient();
                    IServiceProvider provider = builder.Services.BuildServiceProvider();
                    provider.GetService<TaskHubClient>();
                });

        [Fact]
        public void ClientFactory_ClientReturned()
            => RunTest(
                builder =>
                {
                    var mockOrchestrationService = new Mock<IOrchestrationService>();
                    mockOrchestrationService.As<IOrchestrationServiceClient>();
                    Mock.Get(builder)
                        .Setup(m => m.OrchestrationService)
                        .Returns(mockOrchestrationService.Object);
                    builder.AddClient();
                    IServiceProvider provider = builder.Services.BuildServiceProvider();
                    return provider.GetService<TaskHubClient>();
                },
                (mock, client) =>
                {
                    client.Should().NotBeNull();
                });

        private static void RunTestException<TException>(Action<ITaskHubWorkerBuilder> act)
            where TException : Exception
        {
            bool Act(ITaskHubWorkerBuilder builder)
            {
                act(builder);
                return true;
            }

            TException exception = Capture<TException>(() => RunTest(Act, null));
            exception.Should().NotBeNull();
        }

        private static void RunTest<TResult>(
            Func<ITaskHubWorkerBuilder, TResult> act,
            Action<Mock<ITaskHubWorkerBuilder>, TResult> verify)
        {
            var mock = new Mock<ITaskHubWorkerBuilder>();
            mock.Setup(x => x.Services).Returns(new ServiceCollection());

            TResult result = act(mock.Object);
            verify?.Invoke(mock, result);
        }
    }
}
