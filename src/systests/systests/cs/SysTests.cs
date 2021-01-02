﻿using System;
using System.IO;
using System.Linq;

namespace systests
{
    public class SysTests: SysTestBase
    {
        private readonly string debugOrDebugMono;
        
        public SysTests ()
            : base(Environment.CurrentDirectory)
        {
            var isMono = Type.GetType("Mono.Runtime") != null;
            debugOrDebugMono = isMono ? "debug-mono" : "debug";
        }

        public void Run()
        {
            Log("Executing system tests for bari\n");

            Steps(new Action[] 
            {
                () => Initialize(),
                () => ExeBuildWithGoal("single-cs-exe", debugOrDebugMono, Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 11, "Test executable running" + Console.Out.NewLine),
                () => SimpleExeBuild("module-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine),
                () => SimpleExeBuild("module-ref-test-withrt", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine),
                () => SimpleExeBuild("suite-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine),
                () => SimpleExeBuild("fsrepo-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 9, "Dependency acquired" + Console.Out.NewLine),
                () => SimpleExeBuild("alias-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 9, "Dependency acquired" + Console.Out.NewLine),
                () => ContentTest(),
                () => SimpleExeBuild("runtime-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 0, ""),
                () => SimpleExeBuild("script-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 11, string.Join("",
                    "Hello_base!!!",
                    Console.Out.NewLine,
                    Console.Out.NewLine,
                    "Hello_world!!!",
                    Console.Out.NewLine,
                    Console.Out.NewLine)),
                () => ExeProductBuild("postprocessor-script-test", "main", Path.Combine("target", "main", "HelloWorld.exe"), 11, string.Join("",
                    "Hello_world!!!",
                    Console.Out.NewLine,
                    Console.Out.NewLine)),
                () => MultiSolutionTest()
            }.Concat(isRunningOnMono ? new Action[0] : new Action[] {
                () => SimpleExeBuild("embedded-resources-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 11, "Hello world!" + Console.Out.NewLine + "WPF Hello world WPF!" + Console.Out.NewLine),
                () => SimpleExeBuild("cpp-rc-support", Path.Combine("target", "Module1", "hello.exe"), 13, "Test C++ executable running"),
                () => SimpleExeBuild("mixed-cpp-cli", Path.Combine("target", "Module1", "hello.exe"), 11, "Hello World" + Console.Out.NewLine),
                () => SimpleExeBuild("regfree-com-server", Path.Combine("target", "client", "comclient.exe"), 0, "Hello world" + Console.Out.NewLine),
                () => SimpleExeBuild("single-cpp-exe", Path.Combine("target", "Module1", "hello.exe"), 13, "Test C++ executable running"),
                () => SimpleExeBuild("static-lib-test", Path.Combine("target", "test", "hello.exe"), 10, "Hello world!" + Console.Out.NewLine),
                () => SimpleExeBuild("cpp-version", Path.Combine("target", "Module1", "hello.exe"), 11, "1.2.3.4" + Console.Out.NewLine + "1.2.3.4" + Console.Out.NewLine),
                () => X86X64Test(),
                () => CppReleaseTest(),
                () => SimpleExeBuild("custom-plugin-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 11, string.Join("",
                    "Hello base!!!",
                    Console.Out.NewLine,
                    Console.Out.NewLine,
                    "Hello world!!!",
                    Console.Out.NewLine,
                    Console.Out.NewLine)),
                // TODO: custom-plugin-test for mono
                () => SimpleExeBuild("single-fs-exe", Path.Combine("target", "Module", "Exe1.exe"), 12, "Test F# executable running" + Console.Out.NewLine),
                // TODO: F# support with mono
            }).ToArray());
        }

        private void ContentTest()
        {
            Log("..content-test..");
            Clean("content-test");
            Build("content-test");
            InternalCheckExe("content-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 11, "Test executable running" + Console.Out.NewLine);

            var contentBeside = File.ReadAllText(Path.Combine(root, "content-test", "target", "HelloWorld", "content-beside-cs.txt"));
            if (contentBeside != "content-beside-cs")
                throw new SysTestException("Wrong content in content-beside-cs.txt: " + contentBeside);

            var additionalContent = File.ReadAllText(Path.Combine(root, "content-test", "target", "HelloWorld", "additional-content.txt"));
            if (additionalContent != "additional-content")
                throw new SysTestException("Wrong content in additional-content.txt: " + additionalContent);

            Log("OK\n");
        }

        private void X86X64Test()
        {
            Log("..x86-x64-test..");
            ExeBuildWithGoal("x86-x64-test", "debug-x86", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 32, "32 bit" + Console.Out.NewLine);
            ExeBuildWithGoal("x86-x64-test", "debug-x64", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 64, "64 bit" + Console.Out.NewLine);
            Log("OK\n");
        }

        private void CppReleaseTest()
        {
            Log("..cpp-release-test..");
            ExeBuildWithGoal("cpp-release-test", "custom-release", Path.Combine("target", "Module1", "hello.exe"), 13, "Test C++ executable running");
            Log("OK\n");
        }

        private void MultiSolutionTest()
        {
            Log("..multi-solution-test..");
            Clean("suite-ref-test", logPrefix: "multi-solution-test-1.1-");
            BuildProduct("suite-ref-test", "all", logPrefix: "multi-solution-test-1.1-");
            InternalCheckExe("suite-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine);
            Directory.Delete(Path.Combine(root, "suite-ref-test", "target"), true);

            BuildProduct("suite-ref-test", "HelloWorld", logPrefix: "multi-solution-test-1.2-");
            InternalCheckExe("suite-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine);
            Directory.Delete(Path.Combine(root, "suite-ref-test", "target"), true);

            BuildProduct("suite-ref-test", "all", logPrefix: "multi-solution-test-1.3-");
            InternalCheckExe("suite-ref-test", Path.Combine("target", "HelloWorld", "HelloWorld.exe"), 10, "TEST" + Console.Out.NewLine);

            Log("OK\n");
        }
    }
}

