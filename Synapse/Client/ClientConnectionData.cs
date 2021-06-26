using System.IO;
using System.Net;
using JWT.Algorithms;
using JWT.Builder;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace Synapse.Client
{
    public class ClientConnectionData
    {
        [Newtonsoft.Json.JsonProperty("sub")]
        public string Sub { get; set; }

        [Newtonsoft.Json.JsonProperty("aud")]
        public string Aud { get; set; }

        [Newtonsoft.Json.JsonProperty("iss")]
        public string Iss { get; set; }

        [Newtonsoft.Json.JsonProperty("uuid")]
        public string Uuid { get; set; }

        [Newtonsoft.Json.JsonProperty("session")]
        public string Session { get; set; }

        public static ClientConnectionData DecodeJWT(string jwt)
        {
            var webClient = new WebClient();
            var pem = webClient.DownloadString(ClientManager.CentralServer + "/session/verificationKey");
            var pr = new PemReader(new StringReader(pem));
            var publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);
            var rsa = System.Security.Cryptography.RSA.Create();
            rsa.ImportParameters(rsaParams);
            var payload = JwtBuilder.Create()
                .WithAlgorithm(new RS256Algorithm(rsa))
                .MustVerifySignature()
                .Decode<ClientConnectionData>(jwt);
            return payload;
        }
    }
}
