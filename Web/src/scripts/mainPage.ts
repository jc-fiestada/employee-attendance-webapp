import { Employees } from "./interface/Employee";
import { Toast } from "./shared/toast";
import { ResponseHandler } from "./shared/response";
import { PageAccessAuth } from "./shared/pageAccessAuth";



// note : make sure to refactor some of these code and turn it into reusable methods
// note : move these refactored methods to shared scripts
// i hate ts

// note : disable buttons when processing to prevent spam

// kinda too tired to refactor ts 


let currentlySelectedId : number | null = null;

const deleteModal = <HTMLDivElement> document.getElementById("deleteModal")!;
const updateModal = <HTMLDivElement> document.getElementById("updateModal")!;

const confirmUpdateBtn = <HTMLButtonElement> document.getElementById("confirmUpdate")!;
const confirmDeleteBtn = <HTMLButtonElement> document.getElementById("confirmDelete")!;
const confirmUploadBtn = <HTMLButtonElement> document.getElementById("confirmUpload")!;

const employeeContainer = <HTMLDivElement> document.getElementById("employee-container");

const updateField = <HTMLInputElement> document.getElementById("updateField");
const updateValue = <HTMLInputElement> document.getElementById("updateValue");

const updateSex = document.getElementById("updateSex") as HTMLSelectElement;
const updateDepartment = document.getElementById("updateDepartment") as HTMLSelectElement;

// load table on dom load
document.addEventListener("DOMContentLoaded", async () => {
    await PageAccessAuth.AdminAuth();
    await updateTable() 
    Toast.show({message: "Welcome Admin", type: "ok"});
});

// delete user
confirmDeleteBtn.addEventListener("click", async () => {

    const token = localStorage.getItem("token");
    const response = await fetch("/delete/employee", {
        method : "POST",
        headers : {
            "Authorization" : `Bearer ${token}`,
            "Content-Type" : "application/json"
        },
        body : JSON.stringify({
            id : currentlySelectedId
    })});

    closeModal();

    const responseHandler = new ResponseHandler();
    
    if (!response.ok) {
        const message = await response.text();
        console.log(`ERROR: ${message}`);
        responseHandler.responseNotif("Delete", response.status, message);
    }

    Toast.show({message: "Employee has been successfully deleted", type: "ok"});
    await updateTable();
});

// update user
confirmUpdateBtn.addEventListener("click", async () => {
    const token = localStorage.getItem("token");

    const response = await fetch("/update/employee", {
        method : "POST",
        headers : {
            "Authorization" : `Bearer ${token}`,
            "Content-Type" : "application/json"
        },
        body : JSON.stringify({
            column : updateField.value,
            value : getUpdateValue(),
            employeeId : currentlySelectedId
        })
    });

    closeModal();

    const responseHandler = new ResponseHandler();

    if (!response.ok) {
        const message = await response.text();
        console.log(`ERROR: ${message}`);
        responseHandler.responseNotif("Update", response.status, message);
    }

    Toast.show({message: "yey", type: "ok"});
    await updateTable();
});

// open modal
document.addEventListener("click", (e) =>{
    const element = <HTMLElement> e.target;
    const button = element.closest("button");

    if (!button) return;

    const actionButton = button.closest(".delete-btn, .update-btn, .upload-btn");
    if (!actionButton) return;

    const userRow = <HTMLDivElement> actionButton.closest(".table-row");

    const id = userRow.dataset.id;

    if (!id) {
        console.error("Employee row missing id");
        return;
    }

    currentlySelectedId = parseInt(id);

    if (actionButton!.classList.contains("delete-btn")){
        deleteModal.style.display = "flex";
        return;
    }

    if (actionButton!.classList.contains("update-btn")){
        updateModal.style.display = "flex";
        return;
    }
    
});


// close Modal
document.addEventListener("click", (e) => {
    const element = <HTMLElement> e.target;

    if (element.classList.contains("closeModal")){
        closeModal();
    }
});


// methods
function closeModal(){
    deleteModal.style.display = "none";
    updateModal.style.display = "none";

    currentlySelectedId = null;
}

async function updateTable(){
    employeeContainer.innerHTML = "";

    const token = localStorage.getItem("token");

    const response = await fetch("/select-all/employee", {
        method : "GET",
        headers : {
            "Authorization" : `Bearer ${token}`,
        }});
    
    if (!response.ok){
        if (response.status === 404){
            Toast.show({message: "No employee exist's in the database yet", type: "warning"});
            return;
        }
        Toast.show({message: "Failed to load table", type: "error"});
        return;
    }

    const employees : Employees[] = await response.json();
    console.log(employees);

    employees.forEach(employee => {
        employeeContainer.innerHTML += `
        <div class="table-row" data-id="${employee.id}">
            <div><img src="/resources/${employee.code}.jpeg" /></div>
            <div>${employee.name}</div>
            <div>${employee.sex}</div>
            <div>${employee.gmail}</div>
            <div>${employee.department}</div>
            <div>${employee.id}</div>
            <div class="actions">
                <button class="upload-btn">Upload</button>
                <button class="update-btn">Update</button>
                <button class="delete-btn">Delete</button>
            </div>
        </div>`;
    });
}

function getUpdateValue(): string {

    if (updateField.value === "sex") {
        return updateSex.value;
    }

    if (updateField.value === "department") {
        return updateDepartment.value;
    }

    return updateValue.value;
}



// change input based on currently selected update option

updateField.addEventListener("change", () => {
    updateValue.style.display = "none";
    updateSex.style.display = "none";
    updateDepartment.style.display = "none";

    if (updateField.value === "sex") {
        updateSex.style.display = "block";
        return;
    }

    if (updateField.value === "department") {
        updateDepartment.style.display = "block";
        return;
    }
    updateValue.style.display = "block";
});
