import { Token } from "../models/Token";

export class CrudResponseHandler
{
    public async deleteEmployee(token : Token, id : number) : Promise<Response>{
        const response = await fetch("/delete/employee", {
            method : "POST",
            headers : {
                "Authorization" : `Bearer ${token.token}`,
                "Content-Type" : "application/json"
            },
            body : JSON.stringify({
                id : id
        })});
        return response;
    }

    public async insertEmployee(token : Token, name: string, sex : string, gmail : string, department : string, imgFile : File | null) : Promise<Response | null> {
        const employee = {
            "name" : name,
            "sex" : sex,
            "gmail" : gmail,
            "department" : department
        };
        if (imgFile === null) {
            return null;
        }
        const formData = new FormData();
        formData.append("employee", JSON.stringify(employee));
        formData.append("img", imgFile);
        const response = await fetch("/insert/employee", {
            method : "POST",
            headers : {"Authorization" : `Bearer ${token.token}`},
            body : formData
        });
        return response;
    }
    public async insertAttendance(code : string) : Promise<Response>{
        const response = await fetch("/record/employee-attendance", {
                method : "POST",
                headers : {
                    "Content-Type" : "application/json"
                },
                body : JSON.stringify({
                    code : code
                })
        });
        return response;
    } 
    public async selectAllAttendance(token : Token) : Promise<Response> {
        const response = await fetch("/select/employee-attendance", {
                method : "GET",
                headers : {
                    "Authorization" : `Bearer ${token.token}`,
                    "Content-Type" : "application/json",
        }}); 
        return response;
    }
    public async selectAllEmployee(token : Token) : Promise<Response>{
        const response = await fetch("/select-all/employee", {
            method : "GET",
            headers : {
                "Authorization" : `Bearer ${token.token}`,
        }});
        return response;
    }
    public async selectFilteredEmployee(token: Token, field : string, value : string) : Promise<Response>{
        const response = await fetch("/select/filtered-employee", {
            method : "POST",
            headers : {
                "Authorization" : `Bearer ${token.token}`,
                "Content-Type" : "application/json"
            },
            body : JSON.stringify({
                "column" : field,
                "value" : value
        })});
        return response;
    }
    public async updateEmployee(token: Token, columnName : string, updateValue : string, employeeId : number): Promise<Response>{
        const response = await fetch("/update/employee", {
            method : "POST",
            headers : {
                "Authorization" : `Bearer ${token.token}`,
                "Content-Type" : "application/json"
            },
            body : JSON.stringify({
                column : columnName,
                value : updateValue,
                employeeId : employeeId
            })
        });
        return response;
    }

    // ts is not a crud, move this to util

    public async sendEmployeeViaGmail(token: Token, id: number): Promise<Response>{
        const response = await fetch("/upload/employee-id", {
            method : "POST",
            headers : {
                "Authorization" : `Bearer ${token.token}`,
                "Content-Type" : "application/json"
            },
            body : JSON.stringify({
                id : id
            })
        });
        return response;
    }
}