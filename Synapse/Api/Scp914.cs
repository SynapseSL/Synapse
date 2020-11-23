using Mirror;
using Scp914;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Synapse.Api
{
    public class Scp914
    {
        internal Scp914() { }

        public Scp914Knob KnobState
        {
            get => Scp914Machine.singleton.knobState;
            set => Scp914Machine.singleton.SetKnobState(value);
        }

        public GameObject GameObject => Scp914Machine.singleton.gameObject;

        public bool IsActive => Scp914Machine.singleton.working;

        public Transform Intake
        {
            get => Scp914Machine.singleton.intake;
            set => Scp914Machine.singleton.intake = value;
        }

        public Transform Output
        {
            get => Scp914Machine.singleton.output;
            set => Scp914Machine.singleton.output = value;
        }

        public List<Scp914Recipe> Recipes { get; } = new List<Scp914Recipe>();
        
        public void Activate() => Scp914Machine.singleton.RpcActivate(NetworkTime.time);

        public int UpgradeItemID(int id)
        {
            var recipe = Recipes.FirstOrDefault(x => x.itemID == id);
            if (recipe == null)
                return -1;

            List<int> ids;
            switch (KnobState)
            {
                case Scp914Knob.Rough: ids = recipe.rough; break;
                case Scp914Knob.Coarse: ids = recipe.coarse; break;
                case Scp914Knob.OneToOne: ids = recipe.oneToOne; break;
                case Scp914Knob.Fine: ids = recipe.fine; break;
                case Scp914Knob.VeryFine: ids = recipe.veryFine; break;
                default: Logger.Get.Error("Synapse-Wrapper: Scp914ItemKnobState Error"); return -1;
            }

            return ids.Count == 0 ? -1 : ids[UnityEngine.Random.Range(0, ids.Count)];
        }
    }

    public class Scp914Recipe
    {
        public Scp914Recipe(global::Scp914.Scp914Recipe recipe)
        {
            itemID = (int)recipe.itemID;

            rough = recipe.rough.Select(x => (int)x).ToList();
            coarse = recipe.coarse.Select(x => (int)x).ToList();
            oneToOne = recipe.oneToOne.Select(x => (int)x).ToList();
            fine = recipe.oneToOne.Select(x => (int)x).ToList();
            veryFine = recipe.veryFine.Select(x => (int)x).ToList();
        }

        public Scp914Recipe()
        {
            itemID = -1;
            rough = new List<int> { };
            coarse = new List<int> { };
            oneToOne = new List<int> { };
            fine = new List<int> { };
            veryFine = new List<int> { };
        }

        public int itemID;

        public List<int> rough;

        public List<int> coarse;

        public List<int> oneToOne;

        public List<int> fine;

        public List<int> veryFine;
    }
}
