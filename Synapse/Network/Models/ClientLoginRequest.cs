namespace Synapse.Network.Models
{
    public class ClientLoginRequest
    {
        public string ClientUid { get; set; }
        public string ClientSecret { get; set; }
        public string ResponseCipher { get; set; }
    }
}