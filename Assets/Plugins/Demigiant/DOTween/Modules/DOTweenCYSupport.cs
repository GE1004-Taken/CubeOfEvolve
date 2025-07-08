// DOTweenCYSupport.cs
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

public static class DOTweenCYSupport
{
    public static UniTask ToUniTask(this Tween tween, CancellationToken cancellationToken = default)
    {
        var promise = new UniTaskCompletionSource();

        tween.OnComplete(() => promise.TrySetResult())
             .OnKill(() => promise.TrySetCanceled());

        if (cancellationToken != default)
        {
            cancellationToken.Register(() =>
            {
                if (tween.IsActive() && tween.IsPlaying())
                {
                    tween.Kill();
                }
            });
        }

        return promise.Task;
    }

    public static UniTask<T> ToUniTask<T>(this Tween tween, T result, CancellationToken cancellationToken = default)
    {
        var promise = new UniTaskCompletionSource<T>();

        tween.OnComplete(() => promise.TrySetResult(result))
             .OnKill(() => promise.TrySetCanceled());

        if (cancellationToken != default)
        {
            cancellationToken.Register(() =>
            {
                if (tween.IsActive() && tween.IsPlaying())
                {
                    tween.Kill();
                }
            });
        }

        return promise.Task;
    }
}
