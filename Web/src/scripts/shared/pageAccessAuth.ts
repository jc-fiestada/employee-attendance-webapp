export class PageAccessAuth{
    public static async AdminAuth(){
        const token = localStorage.getItem("token");
        const response = await fetch("/authenticate/page-access", {
            method : "GET",
            headers : {"Authorization" : `Bearer ${token}`}
        });

        if (response.status === 401){
            window.location.href = "401.html"
        }
    } 
}