import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private hubConnection: signalR.HubConnection | undefined;
  
  // Dùng Subject để đẩy data sang cho Dashboard
  private messageSubject = new Subject<any>();

  constructor() { }

  public connect(userId: string) {
    // 1. Cấu hình kết nối tới Hub của C# (Đổi lại domain localhost:7084 cho đúng với Backend của bạn nhé)
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7084/ws/notifications?userId=${userId}`) 
      .withAutomaticReconnect()
      .build();

    // 2. Khởi động kết nối
    this.hubConnection.start()
      .then(() => console.log(' SignalR Connected Successfully!'))
      .catch(err => console.error(' Error while starting SignalR: ', err));

    // 3. Lắng nghe ĐÚNG KÊNH "ReceiveNotification" mà C# đang phát sóng
    this.hubConnection.on('ReceiveNotification', (data) => {
      console.log('SignalR received data:', data);
      // Đẩy data sang cho Component
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