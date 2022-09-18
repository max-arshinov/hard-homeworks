using System;
using System.Threading;

namespace Hw3.Tests;

public class SingleInitializationSingleton
{
    private static readonly object Locker = new();

    private static volatile bool _isInitialized = false;

    public const int DefaultDelay = 3_000;
    
    public int Delay { get; }

    private SingleInitializationSingleton(int delay = DefaultDelay)
    {
        Delay = delay;
        // imitation of complex initialization logic
        Thread.Sleep(delay);
    }

    internal static void Reset()
    {
        _isInitialized = false;
        // volatile inside, see Lazy.CreateValue method !!!
        // Small amount of additional memory is allocated on delegate
        // but long-running initialization method is not triggered
        // until .Instance is called
        _lazy = new(() => new SingleInitializationSingleton());
    }

    // volatile inside, see Lazy.CreateValue method !!!
    private static Lazy<SingleInitializationSingleton> _lazy =
        new(() => new SingleInitializationSingleton());

    public static void Initialize(int delay)
    {
        if (delay < 1)
        {
            throw new ArgumentException("Delay must be >= 1", nameof(delay));
        }

        var alreadyInitializedText = "Can be only initialized once, but it's already initialized";
        if (!_isInitialized)
        {
            lock (Locker)
            {
                // volatile on _isInitialized might be redundant because bool is most likely atomic
                // and most likely there are implicit memory barriers https://www.albahari.com/threading/part4.aspx#_Memory_Barriers_and_Volatility
                // BUT !!!
                
                // https://www.ecma-international.org/publications-and-standards/standards/ecma-335/
                // The specification is defined in terms of acquire/release;
                // These implicit barriers are not  guaranteed!
                // Additionally, it's hard to say if these barriers are still the case in .NET Core+:
                // https://stackoverflow.com/questions/58693683/what-memory-model-is-implemented-in-net-core
                if (_isInitialized)
                {
                    throw new InvalidOperationException(alreadyInitializedText);
                }

                // volatile inside, see Lazy.CreateValue method !!!
                _lazy = new Lazy<SingleInitializationSingleton>(() =>
                    new SingleInitializationSingleton(delay));

                _isInitialized = true;
            }
        }
        else
        {
            throw new InvalidOperationException(alreadyInitializedText);
        }
    }

    public static SingleInitializationSingleton Instance => _lazy.Value;
}