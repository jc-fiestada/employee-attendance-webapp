import { ResponseHandler } from "./shared/response";
import { Attendance } from "./interface/Attendance";
import { PageAccessAuth } from "./shared/pageAccessAuth";

const attendanceContainer = <HTMLDivElement> document.getElementById("attendance-container");


document.addEventListener("DOMContentLoaded", async () => {
    attendanceContainer.innerHTML = "";

    await PageAccessAuth.AdminAuth();

    const token = localStorage.getItem("token");

    const response = await fetch("/select/employee-attendance", {
        method : "GET",
        headers : {
            "Authorization" : `Bearer ${token}`,
            "Content-Type" : "application/json",
    }})    

    const responseHandler = new ResponseHandler();
    
    if (!response.ok) {
        const message = await response.text();
        console.log(`ERROR: ${message}`);
        responseHandler.responseNotif("Attendance Retrieval", response.status, message);
    }

    const attendance : Attendance[] = await response.json();

    attendance.forEach(row => {
        attendanceContainer.innerHTML += `
            <div class="table-row attendance-row">
            <div><img src="/resources/${row.code}.jpeg" />
            </div>
                <div>${row.name}</div>
                <div>${row.dateAndTime}</div>
            </div>`
    });
});


