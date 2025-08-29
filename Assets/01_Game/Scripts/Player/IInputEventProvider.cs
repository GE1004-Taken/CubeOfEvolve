using R3;
using UnityEngine;

public interface IInputEventProvider
{
    public ReadOnlyReactiveProperty<Vector2> Move {  get; }
    public ReadOnlyReactiveProperty<bool> Pause { get; }
    public ReadOnlyReactiveProperty<bool> Create { get; }
    public ReadOnlyReactiveProperty<bool> Skill { get; }
    public ReadOnlyReactiveProperty<float> Zoom { get; }
    public ReadOnlyReactiveProperty<bool> MoveCamera { get; }
    public ReadOnlyReactiveProperty<bool> Shop { get; }
    public ReadOnlyReactiveProperty<bool> Build { get; }

    public ReadOnlyReactiveProperty<bool> Remove { get; }
}
