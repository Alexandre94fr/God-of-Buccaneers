using System.Collections.Generic;
using UnityEngine;

public static class TimeCounter
{
    static readonly List<double> _startTimes = new();

    #region Securities

    static bool IsIndexValid(int p_timerIndex, bool p_isPrintingError = true)
    {
        if (p_timerIndex < 0)
        {
            if (p_isPrintingError)
                Debug.LogError("ERROR ! You gave a negative index.");

            return false;
        }

        return true;
    }

    static bool IsIndexExistInStartTimes(int p_timerIndex, bool p_isPrintingError = true)
    {
        // If p_timerIndex is out of range of _startTimes
        if (p_timerIndex > _startTimes.Count - 1)
        {
            if (p_isPrintingError)
            {
                Debug.LogError(
                    "ERROR ! You gave a index that's not initialised.\n" +
                    $"Use TimeCounter.StartTimer({p_timerIndex}, ...) first."
                );
            }

            return false;
        }

        return true;
    }
    #endregion

    public static void StartTimer(int p_timerIndex, bool p_isWarningPrinted = true)
    {
        // Securities
        if (!IsIndexValid(p_timerIndex))
            return;

        // Will expand the list if p_timerIndex out of bounds
        if (p_timerIndex >= _startTimes.Count)
        {
            int itemsToAdd = p_timerIndex - _startTimes.Count + 1;

            // Initialising a new _startTimes's value until _startTimes have the correct size (_startTimes.Count)
            for (int i = 0; i < itemsToAdd; i++)
            {
                _startTimes.Add(0);
            }
        }

        // If p_isWarningPrinted is true and if the timer at this index is already used, show a warning
        if (p_isWarningPrinted && _startTimes[p_timerIndex] != 0)
            Debug.LogWarning($"Warning! You are overriding an active timer at index : {p_timerIndex}");

        _startTimes.Insert(p_timerIndex, Time.realtimeSinceStartup);
    }

    public static double StopTimer(int p_timerIndex, bool isPrintingResult = true, string p_printResultPrefix = "")
    {
        // Securities
        if (!IsIndexValid(p_timerIndex) || !IsIndexExistInStartTimes(p_timerIndex))
            return 0;

        double timeElapsed = Time.realtimeSinceStartup - _startTimes[p_timerIndex];

        if (isPrintingResult)
        {
            Debug.Log(
                $"{p_printResultPrefix}. \n" +
                $"Time elapsed : {timeElapsed} s."
            );
        }

        _startTimes.RemoveAt(p_timerIndex);
        
        return timeElapsed;
    }
}