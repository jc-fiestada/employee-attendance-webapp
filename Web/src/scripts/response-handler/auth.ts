import { Token } from "../models/Token";

export class AuthService{
    public async signIn(username: string, password: string) : Promise<Response>{
        const response = await fetch("/admin/signin", {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify({
                username : username,
                password : password
            })
        });
        return response;
    }
    public async pageAccessAuth(token: Token) : Promise<boolean>{
        const response = await fetch("/authenticate/page-access", {
            method : "GET",
            headers : {"Authorization" : `Bearer ${token.token}`}
        });
        
        if (response.status === 200){
            return true;
        }
        return false;
    }
}