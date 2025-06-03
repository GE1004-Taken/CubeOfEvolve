using R3;
using UnityEngine;

public class WeaponLevelManager : MonoBehaviour
{
    public static WeaponLevelManager Instance;
    // ---------------------------- SerializeField
    [SerializeField] private WeaponDataBase _weaponData;

    // ---------------------------- ReactiveProperty
    public ReadOnlyReactiveProperty<int>[] PlayerWeaponLevels => _playerWeaponLevels;
    private ReactiveProperty<int>[] _playerWeaponLevels;
    public ReadOnlyReactiveProperty<int>[] EnemyWeaponLevels => _enemyWeaponLevels;
    private ReactiveProperty<int>[] _enemyWeaponLevels;

    // ---------------------------- UnityMassage
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        _playerWeaponLevels = new ReactiveProperty<int>[_weaponData.WeaponDataList.Count];
        _enemyWeaponLevels = new ReactiveProperty<int>[_weaponData.WeaponDataList.Count];

        for (int i = 0; i < _weaponData.WeaponDataList.Count; i++)
        {
            _playerWeaponLevels[i] = new ReactiveProperty<int>(_weaponData.WeaponDataList[i].Level.CurrentValue);
            _enemyWeaponLevels[i] = new ReactiveProperty<int>(1);
        }
    }

    // ---------------------------- UnityMassage
    /// <summary>
    /// 武器のレベルアップ
    /// </summary>
    /// <param name="index">武器のID</param>
    public void WeaponLevelUp(int index)
    {
        _playerWeaponLevels[index].Value++;
    }
}
