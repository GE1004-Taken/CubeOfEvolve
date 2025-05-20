using R3;
using UnityEngine;

public interface IInputEventProvider
{
    public ReadOnlyReactiveProperty<Vector2> Move {  get; }
    public ReadOnlyReactiveProperty<bool> Pause { get; }
    public ReadOnlyReactiveProperty<bool> Create { get; }
    public ReadOnlyReactiveProperty<bool> Skill { get; }
}
