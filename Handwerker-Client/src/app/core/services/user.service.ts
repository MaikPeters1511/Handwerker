import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {User} from "../interfaces/user/IUser";
import {IKcRole} from "../interfaces/user/IKcRole";
import {CreateUserFormModel} from '../interfaces/form/ICreateUserFormModel';
import {UpdateUserFormModel} from '../interfaces/form/IEditUserFormModel';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private apiUrl = '/api/users';

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  createUser(user: any) {
    return this.http.post(this.apiUrl, user);
  }
  updateUser(userId : string, user:any) {
    return this.http.put(this.apiUrl + '/' + userId, user);
  }

  deleteUser(id: string) {
    return this.http.delete(this.apiUrl + '/' + id);
  }

  getRoles(): Observable<IKcRole[]> {
    return this.http.get<IKcRole[]>(`${this.apiUrl}/roles`);
  }

  getUserRoles(userId: string): Observable<IKcRole[]> {
    return this.http.get<IKcRole[]>(`${this.apiUrl}/${userId}/roles`);
  }

  getUserAvailableRoles(userId: string): Observable<IKcRole[]> {
    return this.http.get<IKcRole[]>(`${this.apiUrl}/${userId}/roles/available`);
  }

  createUserRoleMappings(userId: string, roles: IKcRole[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/${userId}/roles/create`, roles);
  }

  deleteUserRoleMappings(userId: string, roles: IKcRole[]): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${userId}/roles/delete`, { body: roles });
  }
}
