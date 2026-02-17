import { Toast } from "./shared/toast";

const form = <HTMLFormElement> document.getElementById("signinForm");
const username = <HTMLInputElement> document.getElementById("username");
const password = <HTMLInputElement> document.getElementById("password");


// ill arrange this later, for now ill just write it here
interface Token{
    token : string
}

form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const usernameValue = username.value;
    const passwordValue = password.value;

    const response = await fetch("/admin/signin", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            username : usernameValue,
            password : passwordValue
        })
    });

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
    
    localStorage.setItem("token", data.token);

    window.location.href = "./main.html";
});
