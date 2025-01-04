
using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class Debouncer
{
    private bool isDebouncing = false;

    public async void Debounce(int debounceTimeMs, Action action)
    {
        if (!isDebouncing)
        {
            isDebouncing = true;
            action.Invoke(); // 액션 실행
            await Task.Delay(debounceTimeMs); // 지연
            isDebouncing = false;
        }
    }
}