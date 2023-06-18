using Akka.Actor;
using WinTail;


// Creating actor system object
var MyActorSystem = ActorSystem.Create("MyActorSystem");

// Creating actors
Props consoleWriteProps = Props.Create<ConsoleWriterActor>();
IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriteProps, "consoleWriterActor");


Props validationProps = Props.Create(() => new ValidationActor(consoleWriterActor));
IActorRef validationActor = MyActorSystem.ActorOf(validationProps, "validationActor");

Props consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

MyActorSystem.WhenTerminated.Wait();