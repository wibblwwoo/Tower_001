using System;
using Tower_001.Scripts.GameLogic.Balance;

namespace Tower.GameLogic.Core
{
    /// <summary>
    /// Centralized random number generator for the entire game.
    /// Provides deterministic random number generation based on a seed.
    /// All game systems should use this manager instead of creating their own Random instances.
    /// </summary>
    public class RandomManager
    {
        private static RandomManager _instance;
        private Random _masterRandom;
        private int _currentSeed;

        public static RandomManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RandomManager();
                }
                return _instance;
            }
        }

        private RandomManager()
        {
            // Use the default seed from config, fallback to system time if empty/null
            var seedString = GameBalanceConfig.RandomGeneration.DefaultSeed;
            if (string.IsNullOrEmpty(seedString))
            {
                SetNewSeed(Environment.TickCount);
            }
            else
            {
                SetNewSeed(seedString.GetHashCode());
            }
        }

        /// <summary>
        /// Sets a new seed for the random number generator.
        /// Use this to create reproducible sequences of random numbers.
        /// </summary>
        public void SetNewSeed(int seed)
        {
            _currentSeed = seed;
            _masterRandom = new Random(seed);
            Console.WriteLine($"RandomManager initialized with seed: {seed}");
        }

        /// <summary>
        /// Gets the current seed being used.
        /// Useful for debugging or reproducing specific random sequences.
        /// </summary>
        public int CurrentSeed => _currentSeed;

        /// <summary>
        /// Creates a new Random instance seeded from the master random.
        /// Use this when you need a separate random sequence that's still deterministic.
        /// </summary>
        public Random CreateNewRandom()
        {
            return new Random(_masterRandom.Next());
        }

        /// <summary>
        /// Gets a random integer between min (inclusive) and max (exclusive).
        /// </summary>
        public int Next(int min, int max)
        {
            return _masterRandom.Next(min, max);
        }

        /// <summary>
        /// Gets a random double between 0.0 and 1.0.
        /// </summary>
        public double NextDouble()
        {
            return _masterRandom.NextDouble();
        }
    }
}