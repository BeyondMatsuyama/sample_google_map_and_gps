using UnityEngine;

/// <summary>
/// カーソル制御
/// </summary>
public class Cursor : MonoBehaviour
{
    public bool     IsEnabled { get; set; }

    /// <summary>
    /// 起動
    /// </summary>
    void Start()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// 更新
    /// </summary>
    void Update()
    {
        if (IsEnabled)
        {
            // カーソルの向きを合わせる
            this.transform.localEulerAngles = new Vector3(0, 0, 360 - Input.compass.trueHeading);
        }
    }
}