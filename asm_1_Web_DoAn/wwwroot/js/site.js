// Thêm món ăn vào combo
function addFoodToCombo(comboId) {
    const foodId = prompt("Nhập ID món ăn cần thêm vào combo:");
    if (!foodId) return;

    fetch(`/api/banhang/${comboId}/add-food/${foodId}`, { method: "POST" })
        .then(response => response.text())
        .then(message => alert(message))
        .catch(error => console.error(error));
}

// Tính lại giá combo
function calculateComboPrice(comboId) {
    fetch(`/api/banhang/${comboId}/calculate-price`)
        .then(response => response.json())
        .then(price => alert(`Giá combo mới: ${price} VND`))
        .catch(error => console.error(error));
}

// Mua combo
function buyCombo(comboId) {
    fetch(`/api/banhang/${comboId}/buy`, { method: "POST" })
        .then(response => response.text())
        .then(message => alert(message))
        .catch(error => console.error(error));
}

// Xem chi tiết món ăn
function viewFoodDetails(foodId) {
    alert(`Xem chi tiết món ăn: ID ${foodId}`);
    // Có thể điều hướng sang trang chi tiết món ăn
}
