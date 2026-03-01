import { Toast } from "./shared/toast";
import { AuthService } from "./response-handler/auth";
import { Token } from "./models/Token";

const form = <HTMLFormElement> document.getElementById("signinForm");
const username = <HTMLInputElement> document.getElementById("username");
const password = <HTMLInputElement> document.getElementById("password");
const service : AuthService = new AuthService();


// ill arrange this later, for now ill just write it here


form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const usernameValue = username.value;
    const passwordValue = password.value;

    const response : Response = await service.signIn(usernameValue, passwordValue);

    if (response.status === 401){
        Toast.show({message: "Invalid Admin Credentials", type: "error"})
        return;
    }

    if (response.status === 500){
        Toast.show({message: "Server Error: Something Went Wrong", type: "error"})
        return;
    }

    if (!response.ok) {
        Toast.show({message: "Sign In Failed - Something Went Wrong", type: "error"})
        return;
    }

    const data : Token = await response.json();
    if (data.token === null){
        Toast.show({message: "Server sent an invalid JWT", type : "error"});
        return;
    }
    localStorage.setItem("token", data.token);
    window.location.href = "./main.html";
});
