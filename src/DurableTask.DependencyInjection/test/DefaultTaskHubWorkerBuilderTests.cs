﻿using System;
using System.Threading.Tasks;
using DurableTask.Core;
using DurableTask.Core.Middleware;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using static DurableTask.TestHelpers;

namespace DurableTask.DependencyInjection.Tests
{
    public class DefaultTaskHubWorkerBuilderTests
    {
        [Fact]
        public void CtorArgumentNull()
            => RunTestException<ArgumentNullException>(_ => new DefaultTaskHubWorkerBuilder(null));

        [Fact]
        public void AddActivityNull()
            => RunTestException<ArgumentNullException>(b => b.AddActivity(null));

        [Fact]
        public void AddActivityMiddlewareNull()
            => RunTestException<ArgumentNullException>(b => b.UseActivityMiddleware(null));

        [Fact]
        public void AddOrchestrationNull()
            => RunTestException<ArgumentNullException>(b => b.AddOrchestration(null));

        [Fact]
        public void AddOrchestrationMiddlewareNull()
            => RunTestException<ArgumentNullException>(b => b.UseOrchestrationMiddleware(null));

        [Fact]
        public void BuildNullServiceProvider()
            => RunTestException<ArgumentNullException>(b => b.Build(null));

        [Fact]
        public void BuildOrchestrationNotSet()
            => RunTestException<InvalidOperationException>(b => b.Build(Mock.Of<IServiceProvider>()));

        [Fact]
        public void AddActivity()
        {
            RunTest(
                null,
                b => b.AddActivity(new TaskActivityDescriptor(typeof(TestActivity))),
                (_, services) => services.Should().HaveCount(2));
        }

        [Fact]
        public void AddActivityMiddleware()
        {
            RunTest(
                null,
                b => b.UseActivityMiddleware(new TaskMiddlewareDescriptor(typeof(TestMiddleware))),
                (_, services) => services.Should().HaveCount(2));
        }

        [Fact]
        public void AddOrchestration()
        {
            RunTest(
                null,
                b => b.AddOrchestration(new TaskOrchestrationDescriptor(typeof(TestOrchestration))),
                (_, services) => services.Should().HaveCount(2));
        }

        [Fact]
        public void AddOrchestrationMiddleware()
        {
            RunTest(
                null,
                b => b.UseOrchestrationMiddleware(new TaskMiddlewareDescriptor(typeof(TestMiddleware))),
                (_, services) => services.Should().HaveCount(2));
        }

        [Fact]
        public void BuildTaskHubWorker()
        {
            RunTest(
                null,
                b =>
                {
                    b.OrchestrationService = Mock.Of<IOrchestrationService>();
                    TaskHubWorker worker = b.Build(Mock.Of<IServiceProvider>());
                    worker.Should().NotBeNull();
                    worker.orchestrationService.Should().Be(b.OrchestrationService);
                },
                null);
        }

        [Fact]
        public void BuildTaskHubWorker_WithMiddleware()
        {
            RunTest(
                null,
                b =>
                {
                    b.OrchestrationService = Mock.Of<IOrchestrationService>();
                    b.UseActivityMiddleware(new TaskMiddlewareDescriptor(typeof(TestMiddleware)));
                    b.UseOrchestrationMiddleware(new TaskMiddlewareDescriptor(typeof(TestMiddleware)));
                    TaskHubWorker worker = b.Build(Mock.Of<IServiceProvider>());
                    worker.Should().NotBeNull();
                    worker.orchestrationService.Should().Be(b.OrchestrationService);
                },
                null);
        }

        private static void RunTestException<TException>(Action<DefaultTaskHubWorkerBuilder> act)
            where TException : Exception
        {
            TException exception = Capture<TException>(() => RunTest(null, act, null));
            exception.Should().NotBeNull();
        }

        private static void RunTest(
            Action<IServiceCollection> arrange,
            Action<DefaultTaskHubWorkerBuilder> act,
            Action<DefaultTaskHubWorkerBuilder, IServiceCollection> verify)
        {
            var services = new ServiceCollection();
            arrange?.Invoke(services);
            var builder = new DefaultTaskHubWorkerBuilder(services);

            act(builder);

            verify?.Invoke(builder, services);
        }

        private class TestOrchestration : TaskOrchestration
        {
            public override Task<string> Execute(OrchestrationContext context, string input)
            {
                throw new NotImplementedException();
            }

            public override string GetStatus()
            {
                throw new NotImplementedException();
            }

            public override void RaiseEvent(OrchestrationContext context, string name, string input)
            {
                throw new NotImplementedException();
            }
        }

        private class TestActivity : TaskActivity
        {
            public override string Run(TaskContext context, string input)
            {
                throw new NotImplementedException();
            }
        }

        private class TestMiddleware : ITaskMiddleware
        {
            public Task InvokeAsync(DispatchMiddlewareContext context, Func<Task> next)
            {
                throw new NotImplementedException();
            }
        }
    }
}
