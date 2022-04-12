namespace Synapse.Api.CustomObjects
{
    public interface IRefreshable
    {
        public void Refresh();

        public bool UpdateEveryFrame { get; }
    }
}
