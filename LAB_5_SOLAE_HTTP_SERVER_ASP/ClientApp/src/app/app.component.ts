import { Component, OnInit } from '@angular/core';
import { DataService } from './data.service';
import { Solae } from './solae';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    providers: [DataService]
})

export class AppComponent implements OnInit {
    solae: Solae = new Solae();
    solaes: Solae[];
    tableMode: boolean = true;

    constructor(private dataService: DataService) { }

    ngOnInit() {
        this.loadSolaes();
    }

    loadSolaes() {
        this.dataService.getSolaes().subscribe((data: Solae[]) => this.solaes = data);
    }

    save() {
        if (this.solae.id == null) {
            this.dataService.createSolae(this.solae).subscribe((data: Solae) => this.solaes.push(data));
        } else {
            this.dataService.updateSolae(this.solae).subscribe(data => this.loadSolaes());
        }

        this.cancel();
    }

    editSolae(s: Solae) {
        this.solae = s;
    }

    cancel() {
        this.solae = new Solae();
        this.tableMode = true;
    }

    delete(s: Solae) {
        this.dataService.deleteSolae(s.id).subscribe(data => this.loadSolaes());
    }

    add() {
        this.cancel();
        this.tableMode = false;
    }
}