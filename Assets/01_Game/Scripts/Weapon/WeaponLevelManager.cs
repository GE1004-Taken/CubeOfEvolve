using R3;
using UnityEngine;

public class WeaponLevelManager : MonoBehaviour
{
    public static WeaponLevelManager Instance;
    // ---------------------------- SerializeField
    [SerializeField] private WeaponDataBase _weaponData;

    // ---------------------------- ReactiveProperty
    public ReadOnlyReactiveProperty<int[]> PlayerWeaponLevels => _playerWeaponLevels;
    private ReactiveProperty<int[]> _playerWeaponLevels = new();
    public ReadOnlyReactiveProperty<int[]> EnemyWeaponLevels => _enemyWeaponLevels;
    private ReactiveProperty<int[]> _enemyWeaponLevels = new();

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

        _playerWeaponLevels.Value = new int[_weaponData.weaponDataList.Count];
        _enemyWeaponLevels.Value = new int[_weaponData.weaponDataList.Count];

        for (int i = 0; i < _weaponData.weaponDataList.Count; i++)
        {
            _playerWeaponLevels.Value[i] = _weaponData.weaponDataList[i].Level.CurrentValue;
            _enemyWeaponLevels.Value[i] = 1;
        }
    }

    // ---------------------------- UnityMassage
    /// <summary>
    /// 武器のレベルアップ
    /// </summary>
    /// <param name="index">武器のID</param>
    public void WeaponLevelUp(int index)
    {
        _playerWeaponLevels.Value[index]++;
    }
}
