using UnityEngine;

namespace Synapse.Api
{
    public class Player : MonoBehaviour
    {
        public ReferenceHub Hub => GetComponent<ReferenceHub>();
    }
}
