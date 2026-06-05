document.addEventListener("DOMContentLoaded", function () {
    const previewModalElement = document.getElementById("previewModal");
    if (previewModalElement) {
        const previewModal = new bootstrap.Modal(previewModalElement);
        document.querySelectorAll(".stretched-link").forEach((link) => {
            link.addEventListener("click", function (e) {
                e.preventDefault();
                previewModal.show();
            });
        });
    }

    const btnFav = document.getElementById("btnToggleFavorite");
    if (btnFav) {
        btnFav.addEventListener("click", function () {
            this.classList.toggle("btn-favorite-active");
            this.innerHTML = this.classList.contains("btn-favorite-active")
                ? '<i class="bi bi-heart-fill me-2"></i> Đã lưu'
                : '<i class="bi bi-heart me-2"></i> Thêm vào Yêu thích';
        });
    }

    const aiModalElement = document.getElementById("aiModal");
    const aiModalBody = document.getElementById("aiModalBody");
    const aiButtons = document.querySelectorAll(".btn-ai-summary");
    if (aiModalElement && aiModalBody && aiButtons.length > 0) {
        const aiModal = new bootstrap.Modal(aiModalElement);

        aiButtons.forEach((button) => {
            button.addEventListener("click", function (e) {
                e.preventDefault();
                const docTitle = this.getAttribute("data-title") || "tài liệu này";

                aiModalBody.innerHTML = `
                    <div class="text-center py-4">
                        <div class="spinner-border text-success mb-3" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p class="mb-0">Đang gửi yêu cầu tới Gemini AI để tóm tắt <strong>${docTitle}</strong>...</p>
                    </div>
                `;
                aiModal.show();
            });
        });
    }

    const btnSearch = document.getElementById("btnSearch");
    const searchInput = document.getElementById("searchInput");
    if (btnSearch && searchInput) {
        btnSearch.addEventListener("click", function (e) {
            e.preventDefault();
            const query = searchInput.value.trim();
            if (query) {
                window.location.href = `/?query=${encodeURIComponent(query)}`;
            } else {
                searchInput.focus();
            }
        });
    }
});
