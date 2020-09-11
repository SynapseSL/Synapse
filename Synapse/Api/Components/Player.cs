using UnityEngine;

namespace Synapse.Api.Components
{
    public class Player : MonoBehaviour
    {
        public ReferenceHub Hub => GetComponent<ReferenceHub>();
    }
}
