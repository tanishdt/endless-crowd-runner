namespace EndlessCrowdRunner.Pooling
{
    /// <summary>
    /// Interface for pooled objects to handle reset and activation hooks without GC allocation.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
