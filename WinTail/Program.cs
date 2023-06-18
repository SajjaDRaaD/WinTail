using Akka.Actor;
using WinTail;


// Creating actor system object
var MyActorSystem = ActorSystem.Create("MyActorSystem");

// Creating actors
var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));

consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

MyActorSystem.WhenTerminated.Wait();