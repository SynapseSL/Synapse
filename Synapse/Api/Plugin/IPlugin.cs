namespace Synapse.Api.Plugin
{
    public interface IPlugin
    {

        void Load();
        void Enable();
        void Reload();
        void Disable();
        
    }


    public abstract class AbstractPlugin : IPlugin
    {
        public virtual void Load()
        {
            
        }

        public virtual void Enable()
        {
            
        }

        public virtual void Reload()
        {
            
        }

        public virtual void Disable()
        {
            
        }
    }
}