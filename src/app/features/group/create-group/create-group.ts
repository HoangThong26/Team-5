import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators,
  FormGroup
} from '@angular/forms';
import { Router } from '@angular/router';
import { GroupService } from '../services/group.service';
import { forkJoin, of } from 'rxjs'; // Thêm 'of' vào đây
import { catchError } from 'rxjs/operators'; // Thêm dòng này để xử lý lỗi

@Component({
  selector: 'app-create-group',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-group.html',
  styleUrl: './create-group.css'
})
export class CreateGroup implements OnInit {
  groupForm!: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';
  isInGroup = false;
  invitedEmails: string[] = [];

  constructor(
    private fb: FormBuilder,
    private groupService: GroupService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.checkUserGroup();
  }

  private initForm(): void {
    this.groupForm = this.fb.group({
      groupName: ['', [Validators.required, Validators.minLength(3)]],
      targetMembers: [2, [Validators.required, Validators.min(2), Validators.max(5)]]
    });
  }

private checkUserGroup(): void {
    const userId = 1; // Hãy đảm bảo lấy đúng ID
    // Kiểm tra xem Service có đang cộng thêm ID vào URL không
    this.groupService.getUserGroup(userId).subscribe({
        next: (res) => this.isInGroup = !!res,
        error: (err) => {
            // Nếu lỗi 404, có thể hiểu là User chưa có nhóm (tức là không sao)
            if (err.status === 404) this.isInGroup = false; 
            else this.isInGroup = false;
        }
    });
}

  // Hàm xử lý nút "Add" trên giao diện của bạn
  addMember(email: string): void {
  const trimmedEmail = email.trim(); // Sử dụng trực tiếp chuỗi string
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  
  if (trimmedEmail && emailPattern.test(trimmedEmail) && !this.invitedEmails.includes(trimmedEmail)) {
    this.invitedEmails.push(trimmedEmail);
  } else if (trimmedEmail && !emailPattern.test(trimmedEmail)) {
    alert("Invalid email format!");
  } else if (this.invitedEmails.includes(trimmedEmail)) {
    alert("This email is already added.");
  }
}

  removeMember(index: number): void {
    this.invitedEmails.splice(index, 1);
  }

  onSubmit(): void {
    if (this.groupForm.invalid || this.isInGroup) return;

    this.loading = true;
    this.errorMessage = '';
    
    // 1. Tạo Group trước
    const createPayload = {
      groupName: this.groupForm.value.groupName,
      targetMembers: this.groupForm.value.targetMembers
    };

    this.groupService.createGroup(createPayload).subscribe({
      next: (res) => {
        const newGroupId = res.groupId;

        // 2. Nếu có danh sách email, thực hiện gửi lời mời
        if (this.invitedEmails.length > 0 && newGroupId) {
         const inviteRequests = this.invitedEmails.map(email => 
          this.groupService.inviteMember({ groupId: newGroupId, inviteeEmail: email }).pipe(
            catchError(err => {
              console.error(`Invite failed for ${email}`, err);
              return of(null); // Trả về null để forkJoin vẫn hoàn tất
            })
          )
        );

          // Đợi tất cả lời mời được gửi đi (hoặc lỗi) rồi mới chuyển hướng
          forkJoin(inviteRequests).subscribe({
            next: () => {
              this.loading = false;
              this.router.navigate(['/groups', newGroupId]);
            },
            error: (inviteErr) => {
              // Dù mời lỗi (ví dụ email không tồn tại), vẫn chuyển hướng vào trang chi tiết
              console.error("Some invitations failed", inviteErr);
              this.loading = false;
              this.router.navigate(['/groups', newGroupId]);
            }
          });
        } else {
          // Nếu không có ai để mời, chuyển hướng luôn
          this.loading = false;
          this.router.navigate(['/groups', newGroupId]);
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || "Create group failed. Please try again.";
      }
    });
  }
}