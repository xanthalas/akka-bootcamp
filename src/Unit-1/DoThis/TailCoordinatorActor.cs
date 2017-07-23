using System;
using Akka.Actor;

namespace WinTail
{
    public class TailCoordinatorActor : UntypedActor
    {
        #region Message types
        /// <summary>
        /// Start tailing the file at the given path
        /// </summary>
        public class StartTail
        {
            public string Filepath { get; private set; }

            public IActorRef ReporterActor { get; private set; }

            public StartTail(string filePath, IActorRef reporterActor)
            {
                Filepath = filePath;
                ReporterActor = reporterActor;
            }
        }

        /// <summary>
        /// Stop tailing the file at the given path
        /// </summary>
        public class StopTail
        {
            public string Filepath { get; private set; }

            public StopTail(string filePath)
            {
                Filepath = Filepath;
            }
        }
        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;

                //Create a TailActor as a child of this instance of TailCoordinatorActor
                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.Filepath)));
            }
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                10,
                TimeSpan.FromSeconds(30),
                x =>    //localOnlyDecider
                {
                    //Maybe we consider ArithmeticException to not be application critical so we just ignore it and keep going
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannpt recover from, stop the failing actor
                    else if (x is NotSupportedException) return Directive.Stop;

                    //In all other cases, just restart the failing actor
                    else return Directive.Restart;
                } );
        }
    }
}
