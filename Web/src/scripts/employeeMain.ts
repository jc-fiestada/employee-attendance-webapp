import { Toast } from "./shared/toast";
import { ResponseHandler } from "./shared/response";
import QrScanner from "qr-scanner";

let qrCode : string | null = null;


const qrVideo = <HTMLVideoElement> document.getElementById("qr-video");
const videoError = <HTMLDivElement> document.getElementById("video-error");

const scanner = new QrScanner(
    qrVideo,
    async result => {
        console.log("QR TRIGGERED");
        if (result === qrCode) return;
        qrCode = result;

        const token = localStorage.getItem("token")
        
        const response = await fetch("/record/employee-attendance", {
            method : "POST",
            headers : {
                "Authorization" : `Bearer ${token}`,
                "Content-Type" : "application/json"
            },
            body : JSON.stringify({
                code : result
            })
        });

        const responseHandler = new ResponseHandler();

        if (!response.ok) {

            const message = await response.text()
            console.log(`ERROR: ${message}`);
            responseHandler.responseNotif("Attendance", response.status, message);
        }
        
        Toast.show(({message: "Attendance Recorded", type: "ok"}));
    });

document.addEventListener("DOMContentLoaded", async () => {
    await scanner.start().catch(err => {
        Toast.show(({message: "No Camera Detected", type: "error"}));;
    });
})

