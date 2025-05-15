using UnityEngine;
using R3;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    // ---------- RP
    [SerializeField] private SerializableReactiveProperty<float> _currentTimeSecond;
    public ReadOnlyReactiveProperty<float> CurrentTimeSecond => _currentTimeSecond;

    // ---------- Event
    public void StartTimer()
    {
        StartCoroutine(MeasureTime());
    }

    public void StopTimer()
    {
        StopCoroutine(MeasureTime());
    }

    public void ResetTimer()
    {
        _currentTimeSecond.Value = 0f;
    }

    // ---------- Method
    private IEnumerator MeasureTime()
    {
        while (true)
        {
            _currentTimeSecond.Value += Time.deltaTime;

            yield return null;
        }
    }
}
