import { Component } from '@angular/core';
import { AuthService } from '../../@core/utils/services/auth.service';
import { Router } from '@angular/router';
import { concat } from 'rxjs/internal/operators/concat';

@Component({
  selector: 'ngx-dashboard',
  styleUrls: ['./dashboard.component.scss'],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  characters: string[];

  constructor(private authService: AuthService, private router: Router) { }
  ngOnInit() {
      if (this.authService.checkLogin()) {
         console.log("Dashboard Work!")
          // this.authService.authGet$("/api/Values/GetStaff").subscribe(
          //     characters => this.characters = characters
          // );
      } else {
          this.router.navigate(["auth"]);
      }
  }
}
