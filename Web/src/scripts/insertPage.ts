import { Toast } from "./shared/toast";

const employeeForm = <HTMLFormElement> document.getElementById("employeeForm");

const sex = <HTMLInputElement> document.getElementById("sex");
const department = <HTMLInputElement> document.getElementById("department");
const fullName = <HTMLInputElement> document.getElementById("fullName");
const gmail = <HTMLInputElement> document.getElementById("userGmail");

const uploadBtn = <HTMLButtonElement> document.getElementById("uploadBtn");
const photoInput = <HTMLInputElement> document.getElementById("photoInput");
const preview = <HTMLDivElement> document.getElementById("photoPreview");


//test first update later

employeeForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    if (photoInput.files == null || photoInput.files.length === 0) {
        Toast.show({message: "Image must be included", type: "error"});
        return;
    }

    const fullNameValue = fullName.value;
    const sexValue = sex.value;
    const gmailValue = gmail.value;
    const departmentValue = department.value;
    const image = photoInput.files[0]!;

    const payload = {
        "name" : fullNameValue,
        "sex" : sexValue,
        "gmail" : gmailValue,
        "department" : departmentValue
    }

    const formData = new FormData();
    formData.append("employee", JSON.stringify(payload));
    formData.append("img", image);

    const token = localStorage.getItem("token");

    const response = await fetch("/insert/employee", {
        method : "POST",
        headers : {"Authorization" : `Bearer ${token}`},
        body : formData
    });

    if (!response.ok) {
        const message = await response.text()
        Toast.show({message : message, type : "error"})
        return;
    }

    const pdfBlob = await response.blob();

    const pdfUrl = window.URL.createObjectURL(pdfBlob);

    window.open(pdfUrl, "_blank");

    Toast.show({message : "It works bruh", type : "ok"})


});


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

