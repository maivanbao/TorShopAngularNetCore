import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../@core/utils/services/auth.service';
import { Router } from '@angular/router';
import { UserLogin } from '../../../@core/utils/models/masterdata/UserLogin.model';

@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  private postStream$: Subscription;
  public user: UserLogin = new UserLogin();
  
  constructor(private authService: AuthService,private router: Router) { }

  ngOnInit() {
  }
  public login(): void {
    // if (this.postStream$) { this.postStream$.unsubscribe }
    this.authService.login$(this.user).subscribe(
        result => {
            if (result.state == 1) {
                console.log("Login successful!");
                this.router.navigate(["pages"]);
            } else {
                alert(result.msg);
            }
        }
    )
  }

  // ngOnDestroy() {
  //   if(this.postStream$){this.postStream$.unsubscribe()}
  // }

}
