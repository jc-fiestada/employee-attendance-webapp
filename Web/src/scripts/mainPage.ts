const deleteModal = document.getElementById("deleteModal")!;
const updateModal = document.getElementById("updateModal")!;

document.querySelectorAll(".delete-btn").forEach(btn => {
    btn.addEventListener("click", () => {
        deleteModal.style.display = "flex";
    });
});

document.querySelectorAll(".update-btn").forEach(btn => {
    btn.addEventListener("click", () => {
        updateModal.style.display = "flex";
    });
});

document.querySelectorAll(".closeModal").forEach(btn => {
    btn.addEventListener("click", () => {
        deleteModal.style.display = "none";
        updateModal.style.display = "none";
    });
});
