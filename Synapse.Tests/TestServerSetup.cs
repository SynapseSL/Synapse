using dnlib.DotNet;
using NUnit.Framework;
using Synapse.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Synapse.Tests
{
    [SetUpFixture]
    public class TestServer
    {
        private const string InstallationCommand = "steamcmd";
        private const string InstallationArgs = "+force_install_dir \"{0}\" +login anonymous +app_update 996560 +app_update 996560 validate +quit";

        public static IReadOnlyList<string> Logs { get; private set; }
        public static StreamWriter LocalAdminInputStream { get; private set; }

        private static string SynapseInstallationDirectory;
        private static string SynapsePluginDirectory;
        private static string ServerDirectory;
        private static string AssemblyCSharpDirectory;
        private static string AssemblyCSharpPath;
        private static string SynapseBuildFolder;
        private static string InjectorOutputDirectory;
        private static Process _localAdminProcess;
        private static ProcessStartInfo _localAdminStartInfo;
        private static readonly List<string> _logs;

        static TestServer()
        {
            _logs = new List<string>();
            Logs = _logs.AsReadOnly();
        }

        [OneTimeSetUp]
        public static async Task SetUp()
        {
            EnsureClosedLocalAdminSL();

            EnsurePaths();
            _localAdminStartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(ServerDirectory, "localadmin.exe"),
                Arguments = "7777",
                WorkingDirectory = ServerDirectory,
                RedirectStandardInput = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            DownloadSlServer();
            InjectSynapse();
            CreateLocalSynapseFolders();
            await GenerateConfigs();
            ActivateRceConfig();

            _localAdminProcess = new Process
            {
                StartInfo = _localAdminStartInfo
            };
            _localAdminProcess.OutputDataReceived += (_, e) => _logs.Add(e.Data);
            _ = _localAdminProcess.Start();
            _localAdminProcess.BeginOutputReadLine();
            LocalAdminInputStream = _localAdminProcess.StandardInput;

            // Give it 5 seconds to boot up and generate configs
            await Task.Delay(5000);

            var errorCount = _logs.Count(x => x.Contains(" error") || x.Contains(" Error") || x.Contains("[ERR]"));
            Assert.Zero(errorCount, "Error during Server startup");

            Console.WriteLine();

            void ActivateRceConfig()
            {
                var configPath = Path.Combine(SynapseInstallationDirectory, "configs", "server-shared", "config.syml");
                var configContent = File.ReadAllText(configPath);
                configContent = configContent.Replace("useLocalRceServer: false", "useLocalRceServer: true");
                File.WriteAllText(configPath, configContent);
            }

            async Task GenerateConfigs()
            {
                _localAdminProcess = Process.Start(_localAdminStartInfo);

                // Give it 5 seconds to boot up and generate configs
                await Task.Delay(5000);

                _localAdminProcess.Kill();
                while (!_localAdminProcess.HasExited)
                    ;
            }

            void CreateLocalSynapseFolders()
            {
                _ = Directory.CreateDirectory(SynapseInstallationDirectory);
                CopyDirectory(SynapseBuildFolder, SynapseInstallationDirectory, true);

                SynapsePluginDirectory = Path.Combine(SynapseInstallationDirectory, "plugins", "server-7777");
                _ = Directory.CreateDirectory(SynapsePluginDirectory);
            }

            void InjectSynapse()
            {
                var injector = new SynapseInjector(true, InjectorOutputDirectory);
                var assembly = ModuleDefMD.Load(AssemblyCSharpPath);
                injector.Start(assembly);
                assembly.Dispose();

                File.Delete(AssemblyCSharpPath);
                File.Move(Path.Combine(InjectorOutputDirectory, "Assembly-CSharp.dll"), AssemblyCSharpPath);
            }

            void DownloadSlServer()
            {
                Assert.DoesNotThrow(() =>
                {
                    var combinedPath = String.Format(InstallationArgs, ServerDirectory);
                    using var process = Process.Start(new ProcessStartInfo()
                    {
                        FileName = InstallationCommand,
                        Arguments = combinedPath
                    });
                    process.WaitForExit();

                    // Ensure download success
                    Assert.AreEqual(0, process.ExitCode);
                });
            }

            void EnsurePaths()
            {
                var testDirectoryInfo = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                var solutionDirectory = Path.Combine(testDirectoryInfo.Parent.Parent.Parent.Parent.FullName);

                // \\repos\\SynapseSL\\Synapse\\Synapse\\bin\\Synapse
                SynapseBuildFolder = Path.Combine(solutionDirectory, "Synapse", "bin", "Synapse");

                // \\repos\\SynapseSL\\Synapse\\Synapse.Tests\\bin\\Debug\\net48\\Synapse.Tests\\TestServer
                ServerDirectory = Path.Combine(testDirectoryInfo.FullName, "TestServer");

                // \\repos\\SynapseSL\\Synapse\\Synapse.Tests\\bin\\Debug\\net48\\Synapse.Tests\\TestServer\\SCPSL_Data\\Managed
                AssemblyCSharpDirectory = Path.Combine(ServerDirectory, "SCPSL_Data", "Managed");

                // \\repos\\SynapseSL\\Synapse\\Synapse.Tests\\bin\\Debug\\net48\\Synapse.Tests\\TestServer\\SCPSL_Data\\Managed\\Assembly-CSharp.dll
                AssemblyCSharpPath = Path.Combine(AssemblyCSharpDirectory, "Assembly-CSharp.dll");

                // \\repos\\SynapseSL\\Synapse\\Synapse.Tests\\bin\\Debug\\net48\\Synapse.Tests\\TestServer
                InjectorOutputDirectory = Path.Combine(testDirectoryInfo.FullName, "Injector_Output");

                // \\repos\\SynapseSL\\Synapse\\Synapse.Tests\\bin\\Debug\\net48\\Synapse.Tests\\TestServer\\Synapse
                SynapseInstallationDirectory = Path.Combine(ServerDirectory, "Synapse");

                if (Directory.Exists(ServerDirectory))
                    Directory.Delete(ServerDirectory, true);

                if (Directory.Exists(InjectorOutputDirectory))
                    Directory.Delete(InjectorOutputDirectory, true);

                _ = Directory.CreateDirectory(ServerDirectory);
                _ = Directory.CreateDirectory(InjectorOutputDirectory);
            }
        }

        [OneTimeTearDown]
        public static async Task Teardown()
        {
            EnsureClosedLocalAdminSL();

            // Give it a moment, localadmin ain't that fast.
            await Task.Delay(2500);

            Directory.Delete(ServerDirectory, true);
            Directory.Delete(InjectorOutputDirectory, true);
        }

        private static void EnsureClosedLocalAdminSL()
        {
            var localAdmin = Process.GetProcessesByName("LocalAdmin")?.FirstOrDefault();
            localAdmin?.Kill();

            var scpsl = Process.GetProcessesByName("SCPSL")?.FirstOrDefault();
            scpsl?.Kill();

            while ((!localAdmin?.HasExited ?? false) && (!scpsl?.HasExited ?? false))
                ;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            var dirs = dir.GetDirectories();

            // Create the destination directory
            _ = Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (var file in dir.GetFiles())
            {
                var targetFilePath = Path.Combine(destinationDir, file.Name);
                _ = file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (var subDir in dirs)
                {
                    var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}