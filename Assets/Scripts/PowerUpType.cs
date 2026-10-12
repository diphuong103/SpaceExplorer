using UnityEngine;

public enum PowerUpType
{
    [InspectorName("Không có")]
    None = 0,
    [InspectorName("Khác / Tăng tốc")]
    WarpDrive = 1,
    [InspectorName("Khác / Hồi năng lượng")]
    Energy = 2,
    [InspectorName("Khác / Hồi máu")]
    Heal = 3,
    [InspectorName("Phòng thủ / Khiên")]
    Shield = 4,
    [InspectorName("Cũ / Làm chậm")]
    Slow = 5,
    [InspectorName("Tấn công / Bắn đạn chùm")]
    NovaSpread = 6,
    [InspectorName("Điểm / Nhân đôi điểm")]
    QuantumScore = 7,
    [InspectorName("Cũ / Đạn tự tìm mục tiêu")]
    ViperHoming = 8,
    [InspectorName("Tấn công / Bắn laser")]
    HyperionBeam = 9,
    [InspectorName("Cũ / Xóa chướng ngại vật")]
    Wipe = 10
}
