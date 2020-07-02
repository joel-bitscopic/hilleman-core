namespace com.bitscopic.hilleman.core.domain.pooling
{
    public abstract class AbstractResource
    {
        /// <summary>
        /// When implementing an AbstractResourcePool, your pooled items should inherit this class 
        /// and implement this method (even if it always returns true) to ensure items being retrieved
        /// from the pool are still valid (e.g. a connection is still connected)
        /// </summary>
        /// <returns></returns>
        public abstract bool isAlive();

        public abstract void cleanUp(); // use in place of dispose because of ObjectDisposedException
    }
}
