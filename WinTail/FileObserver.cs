using Akka.Actor;

namespace WinTail;

public class FileObserver : IDisposable
{
    private readonly string _absoluteFilePath;
    private readonly string _fileNameOnly;
    private readonly string _fileDir;
    private FileSystemWatcher _watcher;
    private readonly IActorRef _tailActor;

    public FileObserver(string aboluteFilePath, IActorRef tailActor)
    {
        _absoluteFilePath = aboluteFilePath;
        _tailActor = tailActor;
        _fileDir = Path.GetDirectoryName(aboluteFilePath);
        _fileNameOnly = Path.GetFileName(aboluteFilePath);
    }

    public void Start()
    {

        _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

        _watcher.Changed += OnFileChanged;
        _watcher.Error += OnFileError;

        _watcher.EnableRaisingEvents = true;
    }
    void OnFileError(object sender, ErrorEventArgs e)
    {
        _tailActor.Tell(new TailActor.FileError(_fileNameOnly,
                e.GetException().Message),
            ActorRefs.NoSender);
    }
    void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}