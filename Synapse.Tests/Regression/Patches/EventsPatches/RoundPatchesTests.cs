using NUnit.Framework;
using Synapse.RCE;
using System.Linq;
using System.Threading.Tasks;

namespace Synapse.Tests.Regression.Patches.EventsPatches
{
    public class RoundPatchesTests
    {
        private SynapseRceClient _client;

        [SetUp]
        public void SetUp() => _client = new SynapseRceClient(9090);

        [Test, Sequential]
        public async Task StartRound_Invoke_NoError()
        {
            var logIndexBefore = TestServer.Logs.Count;
            var response = _client.ExecuteFromCode("Synapse.Api.Round.Get.StartRound();");
            Assert.IsTrue(response.IsSuccess);

            await Task.Delay(50);

            var logs = TestServer.Logs.Skip(logIndexBefore);
            var errorLog = logs.FirstOrDefault(_ => _.Contains("[ERR]"));
            var roundStartLog = logs.FirstOrDefault(_ => _.Contains("New round has been started."));

            Assert.IsNull(errorLog);
            Assert.IsNotNull(roundStartLog);
        }
    }
}
