const uploadBtn = <HTMLButtonElement> document.getElementById("uploadBtn");
const photoInput = <HTMLInputElement> document.getElementById("photoInput");
const preview = <HTMLDivElement> document.getElementById("photoPreview");

uploadBtn.addEventListener("click", () => photoInput.click());

photoInput.addEventListener("change", () => {
    const file = photoInput.files?.[0];
    if (!file) return;

    const reader = new FileReader();

    reader.onload = e => {
        preview.innerHTML = `<img src="${e.target?.result}" />`;
    };

    reader.readAsDataURL(file);
});

