namespace Synapse.Api.CustomObjects.CustomAttributes
{
    public class Breakable : AttributeHandler
    {
        public override string Name => "Breakable";

        public override void OnLoad(ISynapseObject synapseObject, System.ArraySegment<string> args)
        {
            var hp = 100;

            if (args.Count > 0 && int.TryParse(args.At(0), out var setHp))
                hp = setHp;

            synapseObject.ObjectData["hp"] = hp;

            Logger.Get.Debug("SetHealth to: " + hp);
        }
    }
}
