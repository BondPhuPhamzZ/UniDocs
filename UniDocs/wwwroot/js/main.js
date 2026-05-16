document.addEventListener("DOMContentLoaded", function () {
    // Hiển thị Modal tài liệu khi nhấn vào card tài liệu đó
  const previewLinks = document.querySelectorAll(".stretched-link"); // Lấy tất cả các thẻ <a> chứa tên tài liệu
  const previewModal = new bootstrap.Modal(document.getElementById('previewModal'));

  previewLinks.forEach(link => {
      link.addEventListener("click", function(e) {
          e.preventDefault(); // Chặn việc load sang trang khác
          previewModal.show();
      });
  });

  // Logic thả tim (Tạm thời FE tĩnh)
  const btnFav = document.getElementById("btnToggleFavorite");
  if(btnFav) {
      btnFav.addEventListener("click", function() {
          this.classList.toggle("btn-favorite-active");
          if(this.classList.contains("btn-favorite-active")) {
              this.innerHTML = '<i class="bi bi-heart-fill me-2"></i> Đã lưu';
          } else {
              this.innerHTML = '<i class="bi bi-heart me-2"></i> Thêm vào Yêu thích';
          }
      });
  }


  // Khởi tạo Modal của Bootstrap -> Hiển thị tóm tắt của AI khi bấm vào nút "Tóm tắt AI"
  const aiModalElement = document.getElementById("aiModal");
  const aiModal = new bootstrap.Modal(aiModalElement);
  const aiModalBody = document.getElementById("aiModalBody");

  // Bắt sự kiện click vào các nút AI Tóm tắt
  const aiButtons = document.querySelectorAll(".btn-ai-summary");

  aiButtons.forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();

      // Lấy tên tài liệu
      const docTitle = this.getAttribute("data-title");

      // Hiện Modal với trạng thái đang tải
      aiModalBody.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-success mb-3" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mb-0">Đang gửi yêu cầu tới Gemini AI để tóm tắt <strong>${docTitle}</strong>...</p>
                </div>
            `;
      aiModal.show();

      // ===== GIẢ LẬP DELAY gọi API mất 2 giây =====
      setTimeout(() => {
        // Trả về kết quả
        aiModalBody.innerHTML = `
                    <h6 class="fw-bold mb-3 border-bottom pb-2">Kết quả phân tích:</h6>
                    <p class="mb-2">Tài liệu <strong>${docTitle}</strong> chứa các nội dung chính sau:</p>
                    <ul>
                        <li>Tổng hợp các dạng bài tập trọng tâm thường gặp trong đề thi.</li>
                        <li>Có kèm theo đáp án chi tiết và giải thích các bước.</li>
                        <li>Phù hợp để sinh viên tự ôn tập nhanh trước kỳ thi.</li>
                    </ul>
                    <div class="alert alert-info mt-3 small mb-0">
                        <i class="bi bi-info-circle"></i> Đây là tóm tắt tự động bởi AI. Bạn nên tải file về để xem chi tiết nhé.
                    </div>
                `;
      }, 2000);
    });
  });

  // ===== Thanh tìm kiếm ở Trang chủ (Nhập tên và nhấn button Tìm kiếm) =====
  const btnSearch = document.getElementById("btnSearch");
  const searchInput = document.getElementById("searchInput");

  btnSearch.addEventListener("click", function (e) {
    e.preventDefault();
    const query = searchInput.value.trim();
    if (query) {
      alert(`Sẽ chuyển hướng tới trang tìm kiếm với từ khóa: "${query}"`);
    } else {
      searchInput.focus();
    }
  });
});
