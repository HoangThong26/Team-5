import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { GroupService } from '../services/group.service';
import { GroupDetail } from '../models/group-detail.model';

@Component({
  selector: 'app-group-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './group-detail.html',
  styleUrl: './group-detail.css'
})
export class GroupDetailComponent implements OnInit {

  group?: GroupDetail;
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private groupService: GroupService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) return;

    this.groupService.getGroupDetail(id).subscribe({
    next: (res: GroupDetail) => {
      this.group = res;
      this.loading = false;
    },
    error: (err: any) => {
      this.error = 'Cannot load group';
      this.loading = false;
    }
  });
  }
}