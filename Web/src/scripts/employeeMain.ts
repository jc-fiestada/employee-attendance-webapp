import { Toast } from "./shared/toast";
import { ResponseHandler } from "./shared/response";
import QrScanner from "qr-scanner";
import { CrudResponseHandler } from "./response-handler/crud-ops";

const service = new CrudResponseHandler();

let qrCode : string | null = null;

const qrVideo = <HTMLVideoElement> document.getElementById("qr-video");

const scanner = new QrScanner(
    qrVideo,
    async result => {
        console.log("QR TRIGGERED");
        console.log(`Code: ${result}`);
        if (result === qrCode) return;
        qrCode = result;
        
        const response = await service.insertAttendance(qrCode);
        const responseHandler = new ResponseHandler();

        if (!response.ok) {

            const message = await response.text()
            console.log(`ERROR: ${message}`);
            responseHandler.responseNotif("Attendance", response.status, message);
            return;
        }
        
        Toast.show(({message: "Attendance Recorded", type: "ok"}));
    });

document.addEventListener("DOMContentLoaded", async () => {
    await scanner.start().catch(err => {
        console.log(err);
        Toast.show(({message: "No Camera Detected", type: "error"}));;
    });
})

