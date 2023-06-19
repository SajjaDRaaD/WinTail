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

        private string _filePath;
        private IActorRef _reporterActor;
        private FileObserver _fileObserver;
        private Stream _fileStream;
        private StreamReader _fileStreamReader;

        public TailActor(IActorRef reporterActor,string filePath)
        {
            _filePath = filePath;
            _reporterActor = reporterActor;

        }

        protected override void PreStart()
        {

            _fileObserver = new FileObserver(Path.GetFullPath(_filePath), Self);
            _fileObserver.Start();

            _fileStream = new FileStream(Path.GetFullPath(_filePath),
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void PostStop()
        {
            _fileObserver.Dispose();
            _fileObserver = null;
            _fileStreamReader.Close();
            _fileStreamReader.Dispose();
            base.PostStop();
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
