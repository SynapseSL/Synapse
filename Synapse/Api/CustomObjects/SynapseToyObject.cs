using AdminToys;
using UnityEngine;

namespace Synapse.Api.CustomObjects
{
    public abstract class SynapseToyObject<TToy> : DefaultSynapseObject where TToy : AdminToyBase
    {
        public abstract TToy ToyBase { get; }

        public override GameObject GameObject => ToyBase.gameObject;

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
