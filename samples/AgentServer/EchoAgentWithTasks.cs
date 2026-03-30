using A2A;
using System.Text.Json;

namespace AgentServer;

public sealed class EchoAgentWithTasks : IAgentHandler
{
    public async Task ExecuteAsync(RequestContext context, AgentEventQueue eventQueue, CancellationToken cancellationToken)
    {
        var updater = new TaskUpdater(eventQueue, context.TaskId, context.ContextId);
        var userText = context.UserText;

        if (context.IsContinuation)
        {
            switch (userText)
            {
                case "Sorry I meant Belgium.":
                    await ProcessCapitalOfBelgiumAsync(updater, cancellationToken);
                    return;
                case "I want to fly from New York (JFK) to London (LHR) around October 10th, returning October 17th.":
                    await ProcessFlightConfirmationAsync(updater, cancellationToken);
                    return;
            }
        }
        else
        {
            switch (userText)
            {
                case "What is the capital of France?":
                    await ProcessCapitalOfFranceAsync(updater, cancellationToken);
                    return;
                case "I'd like to book a flight.":
                    await ProcessFlightBookingAsync(updater, cancellationToken);
                    return;
                case string s when s.StartsWith("Conduct a comprehensive analysis of quantum computing", StringComparison.Ordinal):
                    await ProcessQuantumAnalysisRequestAsync(updater, cancellationToken);
                    return;
            }
        }

        //if (context.Configuration?.ReturnImmediately == true)
        //{
        //    await updater.StartWorkAsync(cancellationToken: cancellationToken);
        //    await Task.Delay(3000, cancellationToken); // simulate slow work
        //}
    }

    public static AgentCard GetAgentCard(string agentUrl) =>
        new()
        {
            Name = "Echo Agent",
            Description = "Agent which will echo every message it receives.",
            Version = "1.0.0",
            SupportedInterfaces =
            [
                new AgentInterface
                {
                    Url = agentUrl,
                    ProtocolBinding = "JSONRPC",
                    ProtocolVersion = "1.0",
                }
            ],
            DefaultInputModes = ["text/plain"],
            DefaultOutputModes = ["text/plain"],
            Capabilities = new AgentCapabilities
            {
                Streaming = true,
                PushNotifications = false,
            },
            Skills =
            [
                new AgentSkill
                {
                    Id = "echo",
                    Name = "Echo",
                    Description = "Echoes back the user message with task tracking.",
                    Tags = ["echo", "test"],
                }
            ],
        };

    private static async Task ProcessCapitalOfFranceAsync(TaskUpdater updater, CancellationToken cancellationToken)
    {
        await updater.SubmitAsync(cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            await updater.StartWorkAsync(cancellationToken: cancellationToken);
            await Task.Delay(2000, cancellationToken);
        }

        await updater.AddArtifactAsync([Part.FromText("The capital of the France is Paris")], cancellationToken: cancellationToken);
        await updater.CompleteAsync(cancellationToken: cancellationToken);
    }

    private static async Task ProcessQuantumAnalysisRequestAsync(TaskUpdater updater, CancellationToken cancellationToken)
    {
        await updater.SubmitAsync(cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            await updater.StartWorkAsync(cancellationToken: cancellationToken);
            await Task.Delay(10000, cancellationToken);
        }

        var response = """
            Quantum computing has emerged as a transformative technology with the potential to revolutionize various fields, including cryptography.
            This comprehensive analysis explores the applications of quantum computing in cryptography, highlighting recent breakthroughs, implementation challenges,
            and future roadmap. Quantum cryptography leverages principles of quantum mechanics to enhance security protocols, offering unprecedented levels of protection
            against classical and quantum attacks. The report includes diagrams and visual representations to elucidate complex concepts, facilitating a deeper understanding
            of the subject matter."
            """;

        await updater.AddArtifactAsync([Part.FromText(response)], cancellationToken: cancellationToken);
        await Task.Delay(10000, cancellationToken);

        await updater.CompleteAsync(cancellationToken: cancellationToken);
    }

    private static async Task ProcessCapitalOfBelgiumAsync(TaskUpdater updater, CancellationToken cancellationToken)
    {
        await updater.SubmitAsync(cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            await updater.StartWorkAsync(cancellationToken: cancellationToken);
            await Task.Delay(2000, cancellationToken);
        }

        await updater.AddArtifactAsync([Part.FromText("The capital of the Belgium is Brussels")], cancellationToken: cancellationToken);
        await updater.CompleteAsync(cancellationToken: cancellationToken);
    }

    private static async Task ProcessFlightBookingAsync(TaskUpdater updater, CancellationToken cancellationToken)
    {
        await updater.SubmitAsync(cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            await updater.StartWorkAsync(cancellationToken: cancellationToken);
            await Task.Delay(500, cancellationToken);
        }

        await updater.RequireInputAsync(
            new Message
            {
                Role = Role.Agent,
                MessageId = Guid.NewGuid().ToString("N"),
                Parts = [Part.FromText("Sure, I can help with that! Where would you like to fly to, and from where? Also, what are your preferred travel dates?")]
            },
            cancellationToken);
    }

    private static async Task ProcessFlightConfirmationAsync(TaskUpdater updater, CancellationToken cancellationToken)
    {
        await updater.SubmitAsync(cancellationToken);

        for (int i = 0; i < 3; i++)
        {
            await updater.StartWorkAsync(cancellationToken: cancellationToken);
            await Task.Delay(500, cancellationToken);
        }

        var flightData = JsonSerializer.SerializeToElement(new
        {
            confirmationId = "XYZ123",
            from = "JFK",
            to = "LHR",
            departure = "2024-10-10T18:00:00Z",
            arrival = "2024-10-11T06:00:00Z"
        });

        await updater.AddArtifactAsync(
            [Part.FromData(flightData)],
            name: "FlightItinerary.json",
            cancellationToken: cancellationToken);

        await updater.CompleteAsync(
            message: new Message
            {
                Role = Role.Agent,
                MessageId = Guid.NewGuid().ToString("N"),
                Parts = [Part.FromText("Okay, I've found a flight for you. Confirmation XYZ123. Details are in the artifact.")]
            },
            cancellationToken: cancellationToken);
    }
}