import { Component, OnInit, ChangeDetectorRef } from '@angular/core';import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GroupService } from '../services/group.service';
import { GroupDetail } from '../models/group-detail.model';
import { switchMap, catchError } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { of } from 'rxjs';


@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './group-detail.html',
  styleUrl: './group-detail.css'
})
export class GroupDetailComponent implements OnInit {
  group: GroupDetail | null = null; // Khởi tạo null an toàn
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private groupService: GroupService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) {
          this.error = 'Invalid Group ID';
          this.loading = false;
          return of(null); // Trả về Observable rỗng thay vì throw Error để tránh dừng luồng
        }
        this.loading = true;
        return this.groupService.getGroupDetail(id).pipe(
          catchError((err: HttpErrorResponse) => {
            this.error = err.error?.message || 'Cannot load group details';
            this.loading = false;
            return of(null);
          })
        );
      })
    ).subscribe(res => {
      if (res) {
        this.group = res;
        this.error = '';
      }
      this.loading = false;
      this.cdr.markForCheck();
    });
  }

  // Tải lại dữ liệu mà không làm thay đổi trạng thái loading toàn bộ trang (tăng UX)
  private loadGroupDetails() {
    if (!this.group?.groupId) return;
    
    this.groupService.getGroupDetail(this.group.groupId.toString()).subscribe({
      next: (res: GroupDetail) => {
        this.group = res;
      },
      error: (err: HttpErrorResponse) => console.error('Refresh failed', err)
    });
  }

  inviteMember(email: string) {
    if (!email.trim() || !this.group) return;

    this.groupService.inviteMember({ 
      groupId: this.group.groupId, 
      inviteeEmail: email.trim() 
    }).subscribe({
      next: () => {
        alert("Invitation sent successfully!");
        this.loadGroupDetails();
      },
      error: (err: HttpErrorResponse) => {
        const msg = err.error?.message || "Failed to send invitation";
        alert(msg);
      }
    });
  }
}