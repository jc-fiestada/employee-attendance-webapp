import { Token } from "../models/Token";
import { Toast } from "../shared/toast";

export class Tools{
    public static parseToken(tokenString : string | null) : Token | null{
        if (tokenString !== null){
            const token : Token = { token: tokenString };
            return token;
        }
        return null;
    }

    public static isTokenNull(token: Token | null) : boolean{
        if (token === null){
            window.location.href = "401.html"
            Toast.show({message : "Unauthorized Access", type : "error"});
            return true;
        }
        return false;
    }
}