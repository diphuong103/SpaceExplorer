using UnityEngine;

public class StageTwoParallax : MonoBehaviour
{
    [SerializeField, Min(0f), Tooltip("Biên độ dịch chuyển nhẹ của nền Level 2.")]
    private float backgroundDriftAmplitude = 0.08f;
    [SerializeField, Min(0f), Tooltip("Tần số dao động của nền Level 2.")]
    private float backgroundDriftFrequency = 0.25f;
    [SerializeField, Min(0f), Tooltip("Hệ số tốc độ trôi của trạm bên trái.")]
    private float leftStationSpeedMultiplier = 0.15f;
    [SerializeField, Min(0f), Tooltip("Hệ số tốc độ trôi của trạm bên phải.")]
    private float rightStationSpeedMultiplier = 0.3f;

    private Camera stageCamera;
    private Transform backgroundTransform;
    private Transform leftStationTransform;
    private Transform rightStationTransform;
    private float backgroundWidth;
    private float backgroundHeight;
    private float backgroundSpriteScale;
    private float baseWorldSpeed;
    private float leftStationHalfWidth;
    private float rightStationHalfWidth;

    // Tạo các lớp nền Level 2 và trả về false nếu thiếu camera hoặc sprite hợp lệ.
    public bool Initialize(
        Camera targetCamera,
        Sprite backgroundSprite,
        Sprite leftStationSprite,
        Sprite rightStationSprite,
        float worldSpeed)
    {
        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogError("Stage 2 parallax requires an orthographic camera.", this);
            return false;
        }

        if (backgroundSprite == null || leftStationSprite == null || rightStationSprite == null)
        {
            Debug.LogError("Assign the Level 2 background and both station sprites.", this);
            return false;
        }

        stageCamera = targetCamera;
        baseWorldSpeed = Mathf.Max(0f, worldSpeed);
        backgroundHeight = stageCamera.orthographicSize * 2f;
        backgroundWidth = backgroundHeight * stageCamera.aspect;
        backgroundSpriteScale = Mathf.Max(
            backgroundHeight / backgroundSprite.bounds.size.y,
            backgroundWidth / backgroundSprite.bounds.size.x);

        backgroundTransform = CreateSpriteLayer(
            "Space Background",
            backgroundSprite,
            0);
        FitBackground(backgroundTransform);

        leftStationTransform = CreateStationLayer(
            "Left Space Station",
            leftStationSprite,
            1,
            out leftStationHalfWidth);
        rightStationTransform = CreateStationLayer(
            "Right Space Station",
            rightStationSprite,
            2,
            out rightStationHalfWidth);

        PositionStationsAtViewportEdges();
        return true;
    }

    // Cập nhật dao động nền và chuyển động tuần hoàn của hai trạm không gian.
    private void Update()
    {
        if (stageCamera == null)
        {
            return;
        }

        // Dao động rất nhẹ giúp lớp nền có chiều sâu mà không để lộ mép ảnh.
        Vector3 cameraPosition = stageCamera.transform.position;
        float drift = Mathf.Sin(Time.time * backgroundDriftFrequency) * backgroundDriftAmplitude;
        backgroundTransform.position = new Vector3(
            cameraPosition.x + drift,
            cameraPosition.y,
            0f);

        MoveStation(leftStationTransform, leftStationHalfWidth, leftStationSpeedMultiplier);
        MoveStation(rightStationTransform, rightStationHalfWidth, rightStationSpeedMultiplier);
    }

    // Tạo một lớp SpriteRenderer con với sorting order được chỉ định.
    private Transform CreateSpriteLayer(string layerName, Sprite sprite, int sortingOrder)
    {
        GameObject layerObject = new GameObject(layerName);
        layerObject.transform.SetParent(transform, false);

        SpriteRenderer spriteRenderer = layerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = "Background";
        spriteRenderer.sortingOrder = sortingOrder;
        return layerObject.transform;
    }

    // Tạo lớp trạm, áp dụng tỷ lệ nền và trả về nửa chiều rộng của trạm.
    private Transform CreateStationLayer(
        string layerName,
        Sprite sprite,
        int sortingOrder,
        out float halfWidth)
    {
        Transform stationTransform = CreateSpriteLayer(
            layerName,
            sprite,
            sortingOrder);
        stationTransform.localScale = Vector3.one * backgroundSpriteScale;
        halfWidth = sprite.bounds.size.x * stationTransform.localScale.x * 0.5f;
        return stationTransform;
    }

    // Phóng nền đủ phủ toàn bộ khung nhìn, có dư để tránh lộ viền.
    private void FitBackground(Transform target)
    {
        target.localScale = new Vector3(
            backgroundSpriteScale * 1.04f,
            backgroundSpriteScale * 1.04f,
            1f);
    }

    // Đặt hai trạm tại hai mép trái/phải của khung nhìn camera.
    private void PositionStationsAtViewportEdges()
    {
        float cameraX = stageCamera.transform.position.x;
        float cameraY = stageCamera.transform.position.y;
        float halfViewportWidth = backgroundWidth * 0.5f;

        leftStationTransform.position = new Vector3(
            cameraX - halfViewportWidth + leftStationHalfWidth * 0.45f,
            cameraY,
            0f);
        rightStationTransform.position = new Vector3(
            cameraX + halfViewportWidth - rightStationHalfWidth * 0.45f,
            cameraY,
            0f);
    }

    // Di chuyển trạm sang trái và đưa nó về ngoài mép phải khi đi khỏi màn hình.
    private void MoveStation(Transform station, float halfWidth, float speedMultiplier)
    {
        float cameraX = stageCamera.transform.position.x;
        float cameraY = stageCamera.transform.position.y;
        float leftEdge = cameraX - backgroundWidth * 0.5f;
        float rightEdge = cameraX + backgroundWidth * 0.5f;

        Vector3 position = station.position;
        position.x -= baseWorldSpeed * speedMultiplier * Time.deltaTime;
        position.y = cameraY;

        if (position.x + halfWidth < leftEdge)
        {
            position.x = rightEdge + halfWidth;
        }

        station.position = position;
    }
}
