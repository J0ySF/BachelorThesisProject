namespace Utils
{
    /// <summary>
    /// Interface to control how an ordered pool handles it's items.
    /// </summary>
    public interface IOrderedPoolController<T>
    {
        /// <summary>Called to instantiate a new item in the pool.</summary>
        /// <param name="index">Refers to the pool item index.</param>
        T Instantiate(int index);

        /// <summary>Called to revert an item to it's pre-extraction state.</summary>
        void Rewind(T item)
        {
        }
    }
}