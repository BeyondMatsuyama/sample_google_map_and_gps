using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    /// <summary>
    /// Google API Key & Base URL
    /// </summary>
    private string GoogleApiKey = "";  // 設定してください
    private string BaseUrl      = @"https://maps.googleapis.com/maps/api/staticmap?";

    /// <summary>
    /// 緯度（経度）１度の距離（m）
    /// </summary>
    private const float Lat2Meter = 111319.491f;

    /// <summary>
    /// マップ更新の閾値
    /// </summary>
    private const float ThresholdDistance = 10f;

    /// <summary>
    /// マップ更新時間
    /// </summary>
    private const float UpdateMapTime = 5f;

    /// <summary>
    /// ダウンロードするマップイメージのサイズ
    /// </summary>
    private const int MapImageSize  = 640;

    /// <summary>
    /// 画面に表示するマップスプライトのサイズ
    /// </summary>
    private const int MapSpriteSize = 960;

    [SerializeField] GameObject loading;        // ダウンロード確認用オブジェクト
    [SerializeField] Text       txtLocation;    // 座標
    [SerializeField] Text       txtDistance;    // 距離
    [SerializeField] Image      mapImage;       // マップ Image
    [SerializeField] Cursor     cursor;         // カーソル

    /// <summary>
    /// 起動時処理
    /// </summary>
    void Start()
    {
        // ローディング表示を非表示にしておく
        loading.SetActive(false);
        updateDistance(0f);

        // GPS 初期化
        Input.location.Start();
        Input.compass.enabled = true;

        // マップ取得
        StartCoroutine(updateMap());
    }

    /// <summary>
    /// マップ更新
    /// </summary>
    /// <returns></returns>
    private IEnumerator updateMap()
    {
        // GPS が許可されていない
        if (!Input.location.isEnabledByUser) yield break;

        // サービスの状態が起動中になるまで待機
        while (Input.location.status != LocationServiceStatus.Running)
        {
            yield return new WaitForSeconds(2f);
        }

        // カーソルをアクティブに
        cursor.IsEnabled = true;
             
        LocationInfo curr;
        LocationInfo prev = new LocationInfo();
        while(true)
        {
            // 現在位置
            curr = Input.location.lastData;
            txtLocation.text = string.Format("緯度：{0:0.000000}, 経度：{1:0.000000}", curr.latitude, curr.longitude);

            // 一定以上移動している
            if(getDistanceFromLocation(curr, prev) >= ThresholdDistance)
            {
                // マップ見込み
                yield return StartCoroutine(downloading(curr));
                prev = curr;
            }

            // 待機
            yield return new WaitForSeconds(UpdateMapTime);
        }
    }

    /// <summary>
    /// マップ画像ダウンロード
    /// </summary>
    /// <param name="curr">現在の座標</param>
    /// <returns>コルーチン</returns>
    private IEnumerator downloading(LocationInfo curr)
    {
        loading.SetActive(true);

        // ベース URL
        string url = BaseUrl;
        // 中心座標
        url += "center=" + curr.latitude + "," + curr.longitude;
        // ズーム
        url += "&zoom=" + 18;   // デフォルト 0 なので、適当なサイズに設定
        // 画像サイズ（640x640が上限）
        url += "&size=" + MapImageSize + "x" + MapImageSize;
        // API Key
        url += "&key=" + GoogleApiKey;

        // 地図画像をダウンロード
        url = UnityWebRequest.UnEscapeURL(url);
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        // テクスチャ生成
        if(req.error == null) yield return StartCoroutine(updateSprite(req.downloadHandler.data));

        updateDistance(0f);
        loading.SetActive(false);
    }

    /// <summary>
    /// スプライトの更新
    /// </summary>
    /// <param name="data">マップ画像データ</param>
    /// <returns>コルーチン</returns>
    private IEnumerator updateSprite(byte[] data)
    {
        // テクスチャ生成
        Texture2D tex = new Texture2D(MapSpriteSize, MapSpriteSize);
        tex.LoadImage(data);
        if (tex == null) yield break;
        // スプライト（インスタンス）を明示的に開放
        if (mapImage.sprite != null)
        {
            Destroy(mapImage.sprite);
            yield return null;
            mapImage.sprite = null;
            yield return null;
        }
        // スプライト（インスタンス）を動的に生成
        mapImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }

    /// <summary>
    /// 2点間の距離を取得する
    /// </summary>
    /// <param name="curr">現在の座標</param>
    /// <param name="prev">直前の座標</param>
    /// <returns>距離（メートル）</returns>
    private float getDistanceFromLocation(LocationInfo curr, LocationInfo prev)
    {
        Vector3 cv = new Vector3((float)curr.longitude, 0, (float)curr.latitude);
        Vector3 pv = new Vector3((float)prev.longitude, 0, (float)prev.latitude);
        float dist = Vector3.Distance(cv, pv) * Lat2Meter;
        updateDistance(dist);        
        return dist;        
    }

    /// <summary>
    /// 距離表示
    /// </summary>
    /// <param name="dist">距離</param>
    private void updateDistance(float dist)
    {
        txtDistance.text = string.Format("距離：{0:0.0000} m", dist);
    }

}
