using System;
namespace Synapse.RCE
{
    internal class QueueAction
    {
        internal Action Action { get; set; }
        internal Exception Exception { get; set; }
        internal bool Ran { get; set; }
    }
}