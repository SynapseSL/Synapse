using System;

namespace Synapse.Network
{
    [Serializable]
    public class InstanceMessage : SerializableObjectWrapper
    {
        public string Sender { get; set; }
        public string Receiver { get; set; } = "@";
        public string Subject { get; set; }
        public string ReferenceId { get; set; } = Guid.NewGuid().ToString();

        public InstanceMessage CreateResponse<T>(T obj, string subject = null)
        {
            var msg = new InstanceMessage
            {
                Subject = subject ?? Subject + "Res",
                Sender = Server.Get.NetworkManager.Client.ClientIdentifier,
                Receiver = Sender,
                ReferenceId = ReferenceId
            };
            msg.Update(obj);
            return msg;
        }

        public static InstanceMessage CreateBroadcast<T>(string subject, T obj)
        {
            var msg = new InstanceMessage
            {
                Subject = subject,
                Sender = Server.Get.NetworkManager.Client.ClientIdentifier,
                Receiver = "@"
            };
            msg.Update(obj);
            return msg;
        }

        public static InstanceMessage CreateSend<T>(string subject, T obj, string receiver)
        {
            var msg = new InstanceMessage
            {
                Subject = subject,
                Sender = Server.Get.NetworkManager.Client.ClientIdentifier,
                Receiver = receiver
            };
            msg.Update(obj);
            return msg;
        }

        public static InstanceMessage CreateSend<T>(string subject, T obj, string receiver, string reference)
        {
            var msg = new InstanceMessage
            {
                Subject = subject,
                Sender = Server.Get.NetworkManager.Client.ClientIdentifier,
                Receiver = receiver,
                ReferenceId = reference
            };
            msg.Update(obj);
            return msg;
        }
    }
}