using NUnit.Framework;
using Synapse.RCE;
using Synapse.RCE.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Synapse.Tests.Regression
{
    public class RceTests
    {
        private SynapseRceClient _client;

        [SetUp]
        public void Setup() => _client = new SynapseRceClient(9090);

        [Test, Sequential]
        public void Client_Can_Connect_When_Correct_Port()
        {
            var client = new SynapseRceClient(9090);
            var response = client.ExecuteFromCode("int i = 10;");
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(RceStatus.Success, response.Status);
        }

        [Test, Sequential]
        public void Client_Cannot_Connect_When_Wrong_Port()
        {
            var client = new SynapseRceClient(10_000);
            var response = client.ExecuteFromCode("int i = 10;");
            Assert.IsFalse(response.IsSuccess);
            Assert.AreEqual(RceStatus.ConnectionFailed, response.Status);
        }

        [Test, Sequential]
        public async Task Server_Can_Execute_Logging()
        {
            var logIndexBefore = TestServer.Logs.Count;
            const string outputText = "Testing-Output";
            var response = _client.ExecuteFromCode($"Synapse.Api.Logger.Get.Info(\"{outputText}\");");
            Assert.IsTrue(response.IsSuccess);
            Assert.AreEqual(RceStatus.Success, response.Status);
            Assert.IsNotEmpty(response.Content);

            await Task.Delay(50);

            var logs = TestServer.Logs.Skip(logIndexBefore);
            var targetLog = logs.FirstOrDefault(_ => _.Contains("[INF]") && _.Contains(outputText));
            Assert.IsNotNull(targetLog);
        }

        [Test, Sequential]
        public async Task Client_Cannot_Execute_Twice_For_Same_Assembly()
        {
            var logIndexBefore = TestServer.Logs.Count;
            _ = _client.ExecuteFromCode($"int i = 10;", "Testing");
            var response2 = _client.ExecuteFromCode($"int i = 10;", "Testing");

            Assert.AreEqual(RceStatus.AssemblyAlreadyLoaded, response2.Status);
            Assert.IsFalse(response2.IsSuccess);

            await Task.Delay(50);

            var logs = TestServer.Logs.Skip(logIndexBefore);
            var targetLog = logs.FirstOrDefault(_ => _.Contains("[WRN]") && _.Contains("RCE attempted to load an already loaded assembly"));
            Assert.IsNotNull(targetLog);
        }
    }
}
