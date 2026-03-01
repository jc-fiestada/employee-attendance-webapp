import { Toast } from "./shared/toast";
import { ResponseHandler } from "./shared/response";
import { AuthService } from "./response-handler/auth";
import { CrudResponseHandler } from "./response-handler/crud-ops";
import { Tools } from "./services/tools";
import { Token } from "./models/Token";

const service = new CrudResponseHandler();

const employeeForm = <HTMLFormElement> document.getElementById("employeeForm");
const submitBtn = <HTMLButtonElement> document.getElementById("submit-employee-btn");

const sex = <HTMLInputElement> document.getElementById("sex");
const department = <HTMLInputElement> document.getElementById("department");
const fullName = <HTMLInputElement> document.getElementById("fullName");
const gmail = <HTMLInputElement> document.getElementById("userGmail");

const uploadBtn = <HTMLButtonElement> document.getElementById("uploadBtn");
const photoInput = <HTMLInputElement> document.getElementById("photoInput");
const preview = <HTMLDivElement> document.getElementById("photoPreview");


document.addEventListener("DOMContentLoaded", async () => {
    const tokenString = localStorage.getItem("token");
    const token : Token | null = Tools.parseToken(tokenString);
    if (Tools.isTokenNull(token)) return;

    if(!await new AuthService().pageAccessAuth(token!)){
        window.location.href = "401.html"
        return;
    };
});

// sends employee data including image
employeeForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    submitBtn.disabled = true;

    if (photoInput.files == null || photoInput.files.length === 0) {
        Toast.show({message: "Image must be included", type: "error"});
        submitBtn.disabled = false;
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

    const tokenString = localStorage.getItem("token");
    const token : Token | null = Tools.parseToken(tokenString);
    if (Tools.isTokenNull(token)) return;

    Toast.show({message : "Data is being sent and processed - please wait...", type : "ok"});
    const response = await service.insertEmployee(token!, fullNameValue, sexValue, gmailValue, departmentValue, image);
    if (response === null){
        Toast.show({message: "Image is missing", type : "error"});
        return;
    }
    const responseHandler = new ResponseHandler();
    if (!response.ok) {
        const message = await response.text();
        console.log(`ERROR: ${message}`);
        responseHandler.responseNotif("Insert", response.status, message);
        submitBtn.disabled = false;
        return;
    }

    Toast.show({message : "Employee ID has been sent", type : "ok"})
    submitBtn.disabled = false;
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

