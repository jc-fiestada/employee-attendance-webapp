import { Toast, ToastType } from "./toast";

export class ResponseHandler{
    public responseNotif(actionType : string, statusCode: number, message : string){
        if (statusCode === 401){
            window.location.href = "401.html";
            return;
        }

        if (statusCode === 422){
            Toast.show({"message" : `${message}`, "type" : "error"});
            return;
        }
        
        if (statusCode === 400){
            Toast.show({"message" : `${actionType} Failed - Client request data might be corrupted/missing`, "type" : "error"});
            return;
        }

        if (statusCode === 404){
            Toast.show({"message" : `${message}`, "type" : "error"});
            return;
        }
        
        if (statusCode === 409){
            Toast.show({"message" : `${message}`, "type" : "warning"});
            return;
        }
        
        if(statusCode === 500){
            Toast.show({"message" : "Internal Server Error - Something went wrong", "type" : "error"});
            return;
        }
        
    }
} 