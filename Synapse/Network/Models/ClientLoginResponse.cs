namespace Synapse.Network.Models
{
    public class ClientLoginResponse : SuccessfulStatus
    {
        public string ClientToken { get; set; }
        public string ResponseCipher { get; set; }
    }
}