using AdminToys;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class SynapseToyObject : DefaultSynapseObject
    {
        internal AdminToyBase ToyBase { get; set; }

        public override Vector3 Scale
        {
            get => GameObject.transform.localScale;
            set
            {
                ToyBase.transform.localScale = value;
                ToyBase.NetworkScale = value;
            }
        }
    }
}
