﻿// Copyright (c) Jacob Viau. All rights reserved.
// Licensed under the APACHE 2.0. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using DurableTask.Core;
using DurableTask.DependencyInjection.Properties;

namespace DurableTask.DependencyInjection.Orchestrations
{
    /// <summary>
    /// An orchestration that wraps the real orchestration type.
    /// </summary>
    internal class WrapperOrchestration : TaskOrchestration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrapperOrchestration"/> class.
        /// </summary>
        /// <param name="innerOrchestrationType">The inner orchestration type to use.</param>
        public WrapperOrchestration(Type innerOrchestrationType)
        {
            Check.NotNull(innerOrchestrationType, nameof(innerOrchestrationType));
            Check.ConcreteType<TaskOrchestration>(innerOrchestrationType, nameof(innerOrchestrationType));

            InnerOrchestrationType = innerOrchestrationType;
        }

        /// <summary>
        /// Gets the inner orchestrations type.
        /// </summary>
        public Type InnerOrchestrationType { get; }

        /// <summary>
        /// Gets or sets the inner orchestration.
        /// </summary>
        public TaskOrchestration InnerOrchestration { get; set; }

        /// <inheritdoc />
        public override Task<string> Execute(OrchestrationContext context, string input)
        {
            CheckInnerOrchestration();
            using (OrchestrationScope.EnterScope(context.OrchestrationInstance.InstanceId))
            {
                // While this looks wrong to not await this task before disposing the scope,
                // DurableTask orchestrations are never resumed after yielding. They will only
                // be replayed from scratch.
                return InnerOrchestration.Execute(context, input);
            }
        }

        /// <inheritdoc />
        public override string GetStatus()
        {
            CheckInnerOrchestration();
            return InnerOrchestration.GetStatus();
        }

        /// <inheritdoc />
        public override void RaiseEvent(OrchestrationContext context, string name, string input)
        {
            CheckInnerOrchestration();
            InnerOrchestration.RaiseEvent(context, name, input);
        }

        private void CheckInnerOrchestration()
        {
            if (InnerOrchestration == null)
            {
                throw new InvalidOperationException(Strings.InnerOrchestrationNull);
            }
        }
    }
}
