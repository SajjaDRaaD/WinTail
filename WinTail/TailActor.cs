using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace WinTail
{
    public class TailActor : UntypedActor
    {

        #region MessageTypes

        public class FileWrite
        {
            public string FileName { get; private set; }

            public FileWrite(string fileName)
            {
                FileName = fileName;
            }
        }

        public class FileError
        {
            public string FileName { get; private set; }
            public string Reason { get; private set; }

            public FileError(string fileName, string reason)
            {
                FileName = fileName;
                Reason = reason;
            }
        }

        public class InitialRead
        {
            public string FileName { get; private set; }
            public string Text { get; private set; }

            public InitialRead(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        #endregion

        private readonly string _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _fileObserver;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor,string filePath)
        {
            _filePath = filePath;
            _reporterActor = reporterActor;

            _fileObserver = new FileObserver(Path.GetFullPath(_filePath),Self);
            _fileObserver.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);
        }

        protected override void OnReceive(object message)
        {
            if (message is FileWrite)
            {
                var text = _fileStreamReader.ReadToEnd();

                if (!String.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
                else if (message is FileError)
                {
                    var fileError = message as FileError;
                    _reporterActor.Tell($"Tail Error : {fileError.Reason}");
                }
                else if (message is InitialRead)
                {
                    var initialRead = message as InitialRead;
                    _reporterActor.Tell(initialRead.Text);
                }
            }
        }
    }
}
