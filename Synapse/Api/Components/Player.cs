using UnityEngine;

namespace Synapse.Api.Components
{
    public class Player : MonoBehaviour
    {
        public string UserId
        {
            get => ClassManager.UserId;
            set => ClassManager.UserId = value;
        }

        public CharacterClassManager ClassManager => Hub.characterClassManager;
        
        public ReferenceHub Hub => GetComponent<ReferenceHub>();
    }
}
