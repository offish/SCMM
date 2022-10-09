﻿using CommandQuery;
using SCMM.Shared.Abstractions.Messaging;
using SCMM.Shared.Abstractions.Messaging.Attributes;
using SCMM.Shared.API.Messages;
using SCMM.Steam.API.Commands;

namespace SCMM.Worker.Server.Handlers
{
    [Concurrency(MaxConcurrentCalls = 1)]
    public class ImportAppItemDefinitionsArchiveHandler : IMessageHandler<ImportAppItemDefinitionsArchiveMessage>
    {
        private readonly ICommandProcessor _commandProcessor;

        public ImportAppItemDefinitionsArchiveHandler(ICommandProcessor commandProcessor)
        {
            _commandProcessor = commandProcessor;
        }

        public async Task HandleAsync(ImportAppItemDefinitionsArchiveMessage message, IMessageContext context)
        {
            await _commandProcessor.ProcessAsync(new ImportSteamAppItemDefinitionsArchiveRequest()
            {
                AppId = message.AppId,
                ItemDefinitionsDigest = message.ItemDefinitionsDigest,
                ParseChanges = message.ParseChanges
            });
        }
    }
}
