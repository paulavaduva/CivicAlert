import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DepartmentComponent } from './components/department/departments';
import { RegisterStaffComponent } from './components/register-staff/register-staff';
import { HomeComponent } from './components/home/home';
import { UserManagementComponent } from './components/user-management/user-management';
import { roleGuard } from './guards/role.guard';
import { IssueManagementComponent } from './components/issue-management/issue-management';
import { IssueReportComponent } from './components/issue-report/issue-report';
import { MyIssuesComponent } from './components/my-issues/my-issues';


export const routes: Routes = [
    { path: 'home', component: HomeComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'departments', component: DepartmentComponent },
    { path: 'register-staff', component: RegisterStaffComponent, canActivate: [roleGuard(['Admin'])] },
    { path: 'user-management', component: UserManagementComponent, canActivate: [roleGuard(['Admin'])] },
    { path: 'issue-management', component: IssueManagementComponent, canActivate: [roleGuard(['Admin', 'Dispatcher', 'HOD', 'TeamLeader'])]},
    { path: 'issue-report', component: IssueReportComponent },
    { path: 'my-issues', component: MyIssuesComponent },
    { path: '', redirectTo: 'home', pathMatch: 'full' } 
];