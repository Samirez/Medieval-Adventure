﻿namespace GameDevTV.Utils
{
    /// <summary>
    /// Container class that wraps a value and ensures initialisation is 
    /// called just before first use.
    /// </summary>
    public class LazyValue<T>
    {
        private T _value;
        private volatile bool _initialized = false;
        private readonly object _sync = new object();
        private InitializerDelegate _initializer;

        public delegate T InitializerDelegate();

        /// <summary>
        /// Setup the container but don't initialise the value yet.
        /// </summary>
        /// <param name="initializer"> 
        /// The initialiser delegate to call when first used. 
        /// </param>
        public LazyValue(InitializerDelegate initializer)
        {
            if (initializer == null)
            {
                throw new System.ArgumentNullException(nameof(initializer));
            }
            _initializer = initializer;
        }
        /// <summary>
        /// Get or set the contents of this container.
        /// </summary>
        /// <remarks>
        /// Note that setting the value before initialisation will initialise 
        /// the class.
        /// </remarks>
        public T value
        {
            get
            {
                lock (_sync)
                {
                    if (!_initialized)
                    {
                        if (_initializer == null)
                        {
                            throw new System.ArgumentNullException(nameof(_initializer));
                        }
                        _value = _initializer();
                        _initialized = true;
                    }
                    return _value;
                }
            }
            set
            {
                lock (_sync)
                {
                    // Don't use default init anymore.
                    _initialized = true;
                    _value = value;
                }
            }
        }

        /// <summary>
        /// Force the initialisation of the value via the delegate.
        /// </summary>
        public void ForceInit()
        {
            lock (_sync)
            {
                if (!_initialized)
                {
                    if (_initializer == null)
                    {
                        throw new System.ArgumentNullException(nameof(_initializer));
                    }
                    _value = _initializer();
                    _initialized = true;
                }
            }
        }
    }
}