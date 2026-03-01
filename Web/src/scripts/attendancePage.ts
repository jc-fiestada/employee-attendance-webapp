import { ResponseHandler } from "./shared/response";
import { Attendance } from "./models/Attendance";
import { AuthService } from "./response-handler/auth";
import { Token } from "./models/Token";
import { Tools } from "./services/tools";
import { CrudResponseHandler } from "./response-handler/crud-ops";


const attendanceContainer = <HTMLDivElement> document.getElementById("attendance-container");
const service = new CrudResponseHandler();


document.addEventListener("DOMContentLoaded", async () => {
    attendanceContainer.innerHTML = "";

    const tokenString = localStorage.getItem("token");
    const token : Token | null = Tools.parseToken(tokenString);
    if (Tools.isTokenNull(token)) return;
    if(!await new AuthService().pageAccessAuth(token!)){
        window.location.href = "401.html"
        return;
    };


    const response = await service.selectAllAttendance(token!);

    const responseHandler = new ResponseHandler();
    
    if (!response.ok) {
        const message = await response.text();
        console.log(`ERROR: ${message}`);
        responseHandler.responseNotif("Attendance Retrieval", response.status, message);
        return;
    }

    const attendance : Attendance[] = await response.json();

    attendance.forEach(row => {

        const dateAndTime = new Date(row.dateAndTime);

        attendanceContainer.innerHTML += `
            <div class="table-row attendance-row">
            <div><img src="/resources/${row.code}.jpeg" />
            </div>
                <div>${row.name.toUpperCase()}</div>
                <div>${dateAndTime.toLocaleString()}</div>
            </div>`
    });
});


