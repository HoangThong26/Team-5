import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private hubConnection: signalR.HubConnection | undefined;
  
  // Dùng Subject để đẩy data sang cho Dashboard
  private messageSubject = new Subject<any>();

  constructor(private authService: AuthService) { }

  public connect(userId?: string) {
    // 1. Cấu hình kết nối tới Hub với Access Token để xác thực
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7084/ws/notifications`, {
        accessTokenFactory: () => this.authService.getAccessToken() || ''
      }) 
      .withAutomaticReconnect()
      .build();

    // 2. Khởi động kết nối
    this.hubConnection.start()
      .then(() => console.log(`✅ SignalR Connected Successfully!${userId ? ' (User: ' + userId + ')' : ''}`))
      .catch(err => console.error('❌ Error while starting SignalR: ', err));

    // 3. Lắng nghe Hub - ReceiveNotification
    this.hubConnection.on('ReceiveNotification', (data) => {
      console.log('📬 SignalR received:', data);
      this.messageSubject.next(data); 
    });
  }

  public getMessages(): Observable<any> {
    return this.messageSubject.asObservable();
  }

  public disconnect() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}