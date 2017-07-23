using System;
﻿using Akka.Actor;

namespace WinTail
{
    #region Program
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // initialize MyActorSystem
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            //// time to make your first actors!
            //var consoleWriterActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            ////var consoleReaderActor = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(consoleWriterActor)));
            //var consoleReaderActor = MyActorSystem.ActorOf(Props.Create<ConsoleReaderActor>(consoleWriterActor));       //Note the line above uses lambda syntax to create the Props. 
            //                                                                                                            //You can also use generic syntax as shown here. 
            //DON'T USE TYPEOF
            //Code above for creating actors has been commented out so we can create the Props separately. Normally you just inline them with the actor create
            Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            /* No longer needed - replaced by FileValidatorActor 
            Props validationActorProps = Props.Create(() => new ValidationActor(consoleWriterActor));
            IActorRef validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");
            */
            Props tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
            IActorRef tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");

            //Props fileValidatorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor, tailCoordinatorActor));      Replaced with ActorSelection version
            Props fileValidatorProps = Props.Create(() => new FileValidatorActor(consoleWriterActor));
            IActorRef validationActor = MyActorSystem.ActorOf(fileValidatorProps, "validationActor");

            //Props consoleReaderProps = Props.Create(() => new ConsoleReaderActor(validationActor));   No longer required no we have replaced it with the ActorSelection version
            Props consoleReaderProps = Props.Create(() => new ConsoleReaderActor());
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // tell console reader to begin
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
    #endregion
}
