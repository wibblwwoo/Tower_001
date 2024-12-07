using System;
using static GlobalEnums;

namespace Tower.GameLogic.StatSystem
{
    /// <summary>
    /// Arguments for when a stat threshold is reached
    /// </summary>
    public class StatThresholdEventArgs : EventArgs
    {
        public StatType StatType { get; }
        public float PreviousValue { get; }
        public float NewValue { get; }
        public float ThresholdValue { get; }
        public bool IsIncreasing { get; }

        public StatThresholdEventArgs(StatType statType, float previousValue, float newValue, float thresholdValue)
        {
            StatType = statType;
            PreviousValue = previousValue;
            NewValue = newValue;
            ThresholdValue = thresholdValue;
            IsIncreasing = newValue > previousValue;
        }
    }

    /// <summary>
    /// Represents a threshold that triggers an event when crossed
    /// </summary>
    public class StatThreshold
    {
        public StatType StatType { get; }
        public float Value { get; }
        public bool Triggered { get; private set; }
        public Action<StatThresholdEventArgs> OnThresholdReached { get; }

        public StatThreshold(StatType statType, float value, Action<StatThresholdEventArgs> onThresholdReached)
        {
            StatType = statType;
            Value = value;
            OnThresholdReached = onThresholdReached;
        }

        /// <summary>
        /// Check if the threshold has been crossed and trigger the event if necessary
        /// </summary>
        public void CheckThreshold(float previousValue, float newValue)
        {
            if (Triggered) return;

            bool crossedThreshold = (previousValue < Value && newValue >= Value) || 
                                  (previousValue > Value && newValue <= Value);

            if (crossedThreshold)
            {
                Triggered = true;
                OnThresholdReached?.Invoke(new StatThresholdEventArgs(StatType, previousValue, newValue, Value));
            }
        }

        /// <summary>
        /// Reset the threshold to allow it to trigger again
        /// </summary>
        public void Reset()
        {
            Triggered = false;
        }
    }
}
