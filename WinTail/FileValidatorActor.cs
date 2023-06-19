using Akka.Actor;

namespace WinTail;
public class FileValidatorActor : UntypedActor
{
    private readonly IActorRef _consoleWriterActor;

    public FileValidatorActor(IActorRef consoleWriterActor)
    {
        _consoleWriterActor = consoleWriterActor;
    }

    protected override void OnReceive(object message)
    {
        var msg = message as string;

        if (string.IsNullOrEmpty(msg))
        {
            _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank please try again later"));

            Sender.Tell(new Messages.ContinueProcessing());
        }
        else
        {
            var valid = IsFileUri(msg);

            if (valid)
            {
                // Signal Successful Input
                _consoleWriterActor.Tell(new Messages.InputSuccess($"Starting processing for {msg}"));

                // Start Cordinator
                var tailCordinatorActor = Context.ActorSelection("akka://MyActorSystem/user/$a");
                tailCordinatorActor.Tell(new TailCordinatorActor.StartTail(msg,_consoleWriterActor));
            }
            else
            {
                _consoleWriterActor.Tell(new Messages.ValidationError($"{msg} is not an existing uri on disk"));
                Sender.Tell(new Messages.ContinueProcessing());
            }
        }
       
    }

    private bool IsFileUri(string path)
    {
        return File.Exists(path);
    }
}