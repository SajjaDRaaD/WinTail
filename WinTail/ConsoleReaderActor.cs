using System.Runtime.InteropServices.ObjectiveC;
using Akka.Actor;
namespace WinTail;

public class ConsoleReaderActor : UntypedActor
{
    public const string ExitCommand = "exit";
    public const string StartCommand = "start";

    protected override void OnReceive(object message)
    {
        var msg = message as string;

        if (message.Equals(StartCommand))
        {
            DoPrintInstructions();
        }

        GetAndValidateInput();
    }

    private void DoPrintInstructions()
    {
        Console.WriteLine("Please provide the URI of a log file on disk.\n");
    }

    private void GetAndValidateInput()
    {
        var message = Console.ReadLine();
        if (!string.IsNullOrEmpty(message) &&
            String.Equals(message, ExitCommand, StringComparison.OrdinalIgnoreCase))
        {
            // if user typed ExitCommand, shut down the entire actor
            // system (allows the process to exit)
            Context.System.Terminate();
            return;
        }

        // otherwise, just hand message off to validation actor
        // (by telling its actor ref)

        var validationActor = Context.ActorSelection("akka://MyActorSystem/user/validationActor");
        validationActor.Tell(message);
    }

}