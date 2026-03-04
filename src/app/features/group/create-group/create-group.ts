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
import { CreateGroupRequest } from '../models/group.model';

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

  constructor(
    private fb: FormBuilder,
    private groupService: GroupService,
    private router: Router
  ) {}

  ngOnInit(): void {

    // init form
    this.groupForm = this.fb.group({
      groupName: ['', [Validators.required, Validators.minLength(3)]],
      targetMembers: [2, [Validators.required, Validators.min(2), Validators.max(5)]]
    });

    // check user already in group
    const userId = 1; // TODO: replace when login ready

    this.groupService.getUserGroup(userId).subscribe({
      next: (res) => {
        this.isInGroup = !!res;
      },
      error: () => {
        this.isInGroup = false;
      }
    });
  }

  onSubmit(): void {

    this.errorMessage = '';
    this.successMessage = '';

    if (this.groupForm.invalid || this.isInGroup) return;

    const payload: CreateGroupRequest = {
      groupName: this.groupForm.value.groupName!,
      targetMembers: this.groupForm.value.targetMembers!
    };

    this.loading = true;

    this.groupService.createGroup(payload).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMessage = res.message;

        this.router.navigate(['/groups', res.groupId]);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage =
          err.error?.message || 'Create group failed';
      }
    });
  }
}