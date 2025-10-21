using A2A;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AgentServer;

public class EchoAgentWithTasks
{
    private ITaskManager? _taskManager;
    private Dictionary<string, (CancellationTokenSource, Task)> tasksById = [];

    public void Attach(ITaskManager taskManager)
    {
        _taskManager = taskManager;
        taskManager.OnTaskCreated = OnTaskCreatedAsync;
        taskManager.OnTaskUpdated = OnTaskUpdatedAsync;
        //taskManager.OnTaskUpdated = ProcessMessageAsync;
        taskManager.OnAgentCardQuery = GetAgentCardAsync;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private async Task OnTaskCreatedAsync(AgentTask task, CancellationToken token)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (task.History?.Any(m => m.Parts.Any(p => ((TextPart)p).Text == "What is the capital of France?")) ?? false)
        {
            StartProcessingCapitalOfFranceTask(task);
        }
        else if (task.History?.Any(m => m.Parts.Any(p => ((TextPart)p).Text == "I'd like to book a flight.")) ?? false)
        {
            StartProcessingFlightBookingTask(task);
        }
    }

    private async Task OnTaskUpdatedAsync(AgentTask task, CancellationToken token)
    {
        if (task.History?.Any(m => m.Parts.Any(p => ((TextPart)p).Text == "Sorry I meant Belgium.")) ?? false)
        {
            // Cancel execution of the existing task but keep existing task alive
            if (tasksById.TryGetValue(task.Id, out (CancellationTokenSource, Task) tuple))
            {
                tasksById.Remove(task.Id);
                await tuple.Item1.CancelAsync();
            }

            StartProcessingCapitalOfBelgiumTask(task);
        }
        else if (task.History?.Any(m => m.Parts.Any(p => ((TextPart)p).Text == "I want to fly from New York (JFK) to London (LHR) around October 10th, returning October 17th.")) ?? false)
        {
            // Cancel execution of the existing task but keep existing task alive
            if (tasksById.TryGetValue(task.Id, out (CancellationTokenSource, Task) tuple))
            {
                tasksById.Remove(task.Id);
                await tuple.Item1.CancelAsync();
            }

            StartProcessingFlightConfirmationTask(task);
        }
    }

    private void StartProcessingCapitalOfFranceTask(AgentTask agentTask)
    {
        var cancellationSource = new CancellationTokenSource();

        var task = Task.Run(async () =>
        {
            TaskState targetState = TaskState.Submitted;
            int iterationNumber = 1;

            while (iterationNumber < 10)
            {
                if (cancellationSource.IsCancellationRequested)
                {
                    break;
                }

                switch (iterationNumber)
                {
                    case 1:
                        targetState = TaskState.Submitted;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        targetState = TaskState.Working;
                        break;
                    case 5:
                        targetState = TaskState.Completed;
                        break;
                }

                if (targetState == TaskState.Completed)
                {
                    await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
                    {
                        Parts = [new TextPart() {
                                        Text = "The capital of the France is Paris"
                                    }],
                    });

                    await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, final: true, cancellationToken: cancellationSource.Token);

                    break;
                }

                await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, cancellationToken: cancellationSource.Token);

                await Task.Delay(2000, cancellationSource.Token);

                iterationNumber++;
            }

            cancellationSource.Dispose();
            tasksById.Remove(agentTask.Id);
        }, cancellationSource.Token);

        tasksById.Add(agentTask.Id, (cancellationSource, task));
    }

    private void StartProcessingCapitalOfBelgiumTask(AgentTask agentTask)
    {
        var cancellationSource = new CancellationTokenSource();

        var task = Task.Run(async () =>
        {
            TaskState targetState = TaskState.Submitted;
            int iterationNumber = 1;

            while (iterationNumber < 10)
            {
                if (cancellationSource.IsCancellationRequested)
                {
                    break;
                }

                switch (iterationNumber)
                {
                    case 1:
                        targetState = TaskState.Submitted;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        targetState = TaskState.Working;
                        break;
                    case 5:
                        targetState = TaskState.Completed;
                        break;
                }

                if (targetState == TaskState.Completed)
                {
                    await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
                    {
                        Parts = [new TextPart() {
                                Text = "The capital of the Belgium is Brussels"
                            }],
                    });

                    await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, final: true, cancellationToken: cancellationSource.Token);

                    break;
                }

                await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, cancellationToken: cancellationSource.Token);

                await Task.Delay(2000, cancellationSource.Token);

                iterationNumber++;
            }

            cancellationSource.Dispose();
            tasksById.Remove(agentTask.Id);
        }, cancellationSource.Token);

        tasksById.Add(agentTask.Id, (cancellationSource, task));
    }

    private void StartProcessingFlightBookingTask(AgentTask agentTask)
    {
        var cancellationSource = new CancellationTokenSource();

        var task = Task.Run(async () =>
        {
            TaskState targetState = TaskState.Submitted;
            int iterationNumber = 1;

            while (iterationNumber < 10)
            {
                if (cancellationSource.IsCancellationRequested)
                {
                    break;
                }

                switch (iterationNumber)
                {
                    case 1:
                        targetState = TaskState.Submitted;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        targetState = TaskState.Working;
                        break;
                    case 5:
                        targetState = TaskState.InputRequired;
                        break;
                }

                if (targetState == TaskState.InputRequired)
                {
                    const string message = "Sure, I can help with that! Where would you like to fly to, and from where? Also, what are your preferred travel dates?";

                    await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: CreateTextMessage(message), final: true, cancellationToken: cancellationSource.Token);

                    break;
                }

                await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, cancellationToken: cancellationSource.Token);

                await Task.Delay(500, cancellationSource.Token);

                iterationNumber++;
            }

            cancellationSource.Dispose();
            tasksById.Remove(agentTask.Id);
        }, cancellationSource.Token);

        tasksById.Add(agentTask.Id, (cancellationSource, task));
    }

    private void StartProcessingFlightConfirmationTask(AgentTask agentTask)
    {
        var cancellationSource = new CancellationTokenSource();

        var task = Task.Run(async () =>
        {
            TaskState targetState = TaskState.Submitted;
            int iterationNumber = 1;

            while (iterationNumber < 10)
            {
                if (cancellationSource.IsCancellationRequested)
                {
                    break;
                }

                switch (iterationNumber)
                {
                    case 1:
                        targetState = TaskState.Submitted;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        targetState = TaskState.Working;
                        break;
                    case 5:
                        targetState = TaskState.Completed;
                        break;
                }

                if (targetState == TaskState.Completed)
                {
#pragma warning disable CS0168 // Variable is declared but never used
                    try
                    {
                        await _taskManager!.ReturnArtifactAsync(agentTask.Id, new Artifact()
                        {
                            Name = "FlightItinerary.json",
                            Parts = [new DataPart() {
                                Data = {
                                    { "confirmationId", JsonSerializer.SerializeToElement("XYZ123") },
                                    { "from", JsonSerializer.SerializeToElement("JFK") },
                                    { "to", JsonSerializer.SerializeToElement("LHR") },
                                    { "departure", JsonSerializer.SerializeToElement("2024-10-10T18:00:00Z") },
                                    { "arrival", JsonSerializer.SerializeToElement("2024-10-11T06:00:00Z") }
                                }
                            }],
                        });
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
#pragma warning restore CS0168 // Variable is declared but never used

                    const string message = "Okay, I've found a flight for you. Confirmation XYZ123. Details are in the artifact.";

                    await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: CreateTextMessage(message), final: true, cancellationToken: cancellationSource.Token);

                    break;
                }

                await _taskManager!.UpdateStatusAsync(agentTask.Id, targetState, message: null, cancellationToken: cancellationSource.Token);

                await Task.Delay(500, cancellationSource.Token);

                iterationNumber++;
            }

            cancellationSource.Dispose();
            tasksById.Remove(agentTask.Id);
        }, cancellationSource.Token);

        tasksById.Add(agentTask.Id, (cancellationSource, task));
    }

    private Task<AgentCard> GetAgentCardAsync(string agentUrl, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<AgentCard>(cancellationToken);
        }

        var capabilities = new AgentCapabilities()
        {
            Streaming = true,
            PushNotifications = false,
        };

        return Task.FromResult(new AgentCard()
        {
            Name = "Echo Agent",
            Description = "Agent which will echo every message it receives.",
            Url = agentUrl,
            Version = "1.0.0",
            DefaultInputModes = ["text"],
            DefaultOutputModes = ["text"],
            Capabilities = capabilities,
            Skills = [],
        });
    }

    private static Message CreateTextMessage(string text)
    {
        return new Message()
        {
            MessageId = Guid.NewGuid().ToString(),
            Role = MessageRole.Agent,
            Parts = [new TextPart() { Text = text }],
        };
    }
}