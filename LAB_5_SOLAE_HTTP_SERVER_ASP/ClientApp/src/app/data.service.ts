import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Solae } from './solae';

@Injectable()
export class DataService {
    private url = "/api/solaes";

    constructor(private http: HttpClient) {

    }

    getSolaes() {
        return this.http.get(this.url);
    }

    getSolae(id: number) {
        return this.http.get(this.url + '/' + id);
    }

    createSolae(solae: Solae) {
        return this.http.post(this.url, solae);
    }

    updateSolae(solae: Solae) {
        return this.http.put(this.url, solae);
    }

    deleteSolae(id: number) {
        return this.http.delete(this.url + '/' + id);
    }
}