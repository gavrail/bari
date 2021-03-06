﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bari.Console.UI;
using Bari.Core;
using Bari.Core.Build;
using Bari.Core.Build.Cache;
using Bari.Core.Commands.Clean;
using Bari.Core.Generic;
using Bari.Core.Model;
using Bari.Core.Process;
using Bari.Core.UI;
using Ninject;
using Ninject.Modules;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using QuickGraph;
using QuickGraph.Algorithms;

namespace Bari.Console
{
    static class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof (Program));

        static int Main(string[] args)
        {
            var consoleParams = new ConsoleParameters(args);
            if (consoleParams.VerboseOutput)
                EnableConsoleDebugLog();

            var root = Kernel.Root;

            // Binding UI interfaces
            root.Bind<IUserOutput>().To<ConsoleUserInterface>().InSingletonScope();
            root.Bind<IParameters>().ToConstant(consoleParams).InSingletonScope();

            // Binding special directories
            var suiteRoot = new LocalFileSystemDirectory(Environment.CurrentDirectory);
            root.Bind<IFileSystemDirectory>()
                .ToConstant(suiteRoot)
                .WhenTargetHas<SuiteRootAttribute>();
            root.Bind<IFileSystemDirectory>()
                .ToConstant(suiteRoot.GetChildDirectory("target", createIfMissing: true))
                .WhenTargetHas<TargetRootAttribute>();

            // Binding core services
            Kernel.RegisterCoreBindings();

            // Binding default cache
            var cacheDir = new Lazy<IFileSystemDirectory>(() => 
                suiteRoot.GetChildDirectory("cache", createIfMissing: true)
                         .GetChildDirectory(root.Get<Suite>().ActiveGoal.Name, createIfMissing: true));
            root.Bind<Lazy<IFileSystemDirectory>>()
                .ToConstant(cacheDir)
                .WhenTargetHas<CacheRootAttribute>();
            root.Bind<IBuildCache>().To<FileBuildCache>();            

            // Loading fix plugins
            var pluginLoader = root.Get<IPluginLoader>();
            foreach (var module in GetOrderedModuleList(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Bari.Plugins.*.dll"))
                pluginLoader.Load(module);

            // Initializing builder store
            Kernel.InitializeBuilderStore();

            // Initializing the cache cleaner
            root.Bind<ICleanExtension>().ToConstant(new CacheCleaner(cacheDir, root.Get<IBuilderEnumerator>(), () => root.Get<ISoftCleanPredicates>()));

            var process = root.Get<MainProcess>();
            try
            {
                if (process.Run())
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                var output = root.Get<IUserOutput>();
                output.Error(ex.ToString());                

                return 2;
            }
        }

        private static IEnumerable<EquatableEdge<INinjectModule>> GetModuleGraph(string path, string pattern)
        {
            var instanceCache = new Dictionary<Type, INinjectModule>();

            foreach (var file in Directory.GetFiles(path, pattern))
            {
                var assembly = Assembly.LoadFile(file);
                var modules = from type in assembly.GetTypes()
                              where type.GetInterfaces().Contains(typeof(INinjectModule))
                              select type;

                foreach (var module in modules)
                {
                    var instance = CreateModuleInstance(module, instanceCache);

                    yield return new EquatableEdge<INinjectModule>(instance, instance);

                    var deps = module.GetCustomAttributes(typeof (DependsOnAttribute), false).Cast<DependsOnAttribute>();
                    foreach (var dep in deps)
                    {
                        var dependentInstance = CreateModuleInstance(dep.DependentModuleType, instanceCache);
                        yield return new EquatableEdge<INinjectModule>(dependentInstance, instance);
                    }
                }
            }
        }

        private static INinjectModule CreateModuleInstance(Type module, Dictionary<Type, INinjectModule> instanceCache)
        {
            INinjectModule instance;
            if (!instanceCache.TryGetValue(module, out instance))
            {
                instance = (INinjectModule) Activator.CreateInstance(module);
                instanceCache.Add(module, instance);
            }

            return instance;
        }

        private static IEnumerable<INinjectModule> GetOrderedModuleList(string path, string pattern)
        {
            var graph = GetModuleGraph(path, pattern).ToAdjacencyGraph<INinjectModule, EquatableEdge<INinjectModule>>();
            graph.RemoveEdgeIf(edge => edge.IsSelfEdge<INinjectModule, EquatableEdge<INinjectModule>>());
            return graph.TopologicalSort();
        }

        private static void EnableConsoleDebugLog()
        {
            log4net.Appender.IAppender appender;
            var consoleAppender = new log4net.Appender.ColoredConsoleAppender
                {
                    Layout = new SimpleLayout(),
                    Threshold = Level.All
                };
            try
            {
                consoleAppender.ActivateOptions();
                appender = consoleAppender;
            }
            catch (EntryPointNotFoundException)
            {
                var fallbackAppender = new log4net.Appender.ConsoleAppender
                {
                    Layout = new SimpleLayout(),
                    Threshold = Level.All
                };

                fallbackAppender.ActivateOptions();
                appender = fallbackAppender;
            }

            var repo = (Hierarchy)log4net.LogManager.GetRepository();
            var root = repo.Root;
            root.AddAppender(appender);

            repo.Configured = true;

            log.Info("Verbose logging to console enabled.");
        }
    }
}
