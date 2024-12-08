using System;

namespace Tower_001.Scripts.GameLogic.Progression
{
    /// <summary>
    /// Defines the types of meta-progression systems available
    /// </summary>
    public enum ProgressionType
    {
        /// <summary>
        /// Ascension system - Higher tier progression that resets both level and prestige
        /// </summary>
        Ascension,

        /// <summary>
        /// Prestige system - Mid-tier progression that resets only level
        /// </summary>
        Prestige
    }
}
