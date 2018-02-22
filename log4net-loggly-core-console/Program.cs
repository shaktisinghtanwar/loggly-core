﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net_loggly_console;

namespace log4Net.loggly.console
{

    class GlobalContextTest : IFixingRequired
    {
        public object GetFixedObject()
        {
            return ToString();
        }

        public override string ToString()
        {
            return DateTime.UtcNow.Millisecond.ToString();
        }
    }

    class Program
    {
        static void Main(string[] argArray)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Thread.SpinWait(4000);
            string repositoryName = string.Format("Repository");

            ILoggerRepository repository = LoggerManager.CreateRepository(repositoryName);
            log4net.Config.XmlConfigurator.Configure(repository);

            var log = LogManager.GetLogger(typeof(Program));

            Thread thread = Thread.CurrentThread;
            thread.Name = "Main Thread";
            ThreadContext.Properties["MainThreadContext"] = "MainThreadContextValue";
            log.Info("Thread test");
            log.Error("oops", new ArgumentOutOfRangeException("argArray"));
            log.Warn("hmmm", new ApplicationException("app exception"));
            log.Info("yawn");

            Thread newThread1 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread 1";
                ThreadContext.Properties["InnerThread1Context"] = "InnerThreadContext1Values";
                LogicalThreadContext.Properties["InnerLogicalThreadContext"] = "InnerLogicalThreadContextValues";

                using (ThreadContext.Stacks["NDC1"].Push("StackValue1"))
                {
                    log.Info("this is an inner thread 1");

                    using (ThreadContext.Stacks["NDC1"].Push("StackValue2"))
                    {
                        log.Info("inner ndc of inner thread 1");
                    }
                }

                using (LogicalThreadContext.Stacks["LogicalThread1"].Push("LogicalThread1_Stack"))
                {
                    log.Info("logical thread context 1 stack");
                    using (LogicalThreadContext.Stacks["LogicalThread1"].Push("LogicalThread1_Stack_2"))
                    {
                        log.Info("logical thread context 2 stack");
                    }
                }

                log.Info("without ndc of inner thread 1");
            });

            newThread1.Start();

            Thread newThread2 = new Thread(() =>
            {
                Thread curntThread = Thread.CurrentThread;
                curntThread.Name = "Inner thread 2";
                ThreadContext.Properties["InnerThread2Context"] = "InnerThreadContext2Values";
                log.Info("this is an inner thread 2");
            });

            newThread2.Start();

            //Test self referencing
            var parent = new Person { Name = "John Smith" };
            var child1 = new Person { Name = "Bob Smith", Parent = parent };
            var child2 = new Person { Name = "Suzy Smith", Parent = parent };
            parent.Children = new List<Person> { child1, child2 };
            log.Info(parent);

            log.Debug("zzzz");
            log.InfoFormat("Loggly is the best {0} to collect Logs.", "service");
            log.Info(new { type1 = "newcustomtype", value1 = "newcustomvalue" });
            log.Info(new TestObject());
            log.Info(null);

            Console.ReadKey();
        }
    }
}
