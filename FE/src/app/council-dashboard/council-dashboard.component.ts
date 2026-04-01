import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DefenseService } from '../services/defense.service';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { AssignedDefense, EvaluationDetails } from '../models/evaluation.model';
import { DefenseEvaluationRequest } from '../models/defense.model';

@Component({
  selector: 'app-council-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './council-dashboard.component.html',
  styleUrl: './council-dashboard.component.css'
})
export class CouncilDashboardComponent implements OnInit {
  // Sử dụng Interface thay cho any[]
  assignedDefenses: AssignedDefense[] = [];
  selectedDefense: AssignedDefense | null = null;
  
  showEvaluationForm = false;
  errorMessage = '';
  successMessage = '';

  // Evaluation Fields
  presentationScore: number = 0;
  demoScore: number = 0;
  qaScore: number = 0;
  finalScore: number = 0;
  evaluationComment: string = '';

  constructor(
    private defenseService: DefenseService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadAssignedDefenses();
  }

  countByStatus(status: string): number {
    return this.assignedDefenses.filter(d => d.status === status).length;
  }

  loadAssignedDefenses() {
    this.defenseService.getAssignedDefenses().subscribe({
      next: (data) => this.assignedDefenses = data,
      error: (err) => console.error('Error fetching defenses:', err)
    });
  }

  openEvaluation(defense: AssignedDefense) {
    this.selectedDefense = defense;
    const userId = this.authService.getUserId();

    if (userId) {
    this.defenseService.getEvaluationDetails(defense.defenseId, Number(userId)).subscribe({
      next: (data: EvaluationDetails) => {
        // Gán dữ liệu từ BE trả về vào các biến ngModel của Form
        this.presentationScore = data.presentationScore || 0;
        this.demoScore = data.demoScore || 0;
        this.qaScore = data.qaScore || 0;
        this.evaluationComment = data.comment || '';
        this.finalScore = data.score; 
        
        this.showEvaluationForm = true;
      },
      error: (err) => {
        console.log('Chưa có điểm cũ, bắt đầu chấm mới.');
        this.resetForm();
        this.showEvaluationForm = true;
      }
    });
  }
  }

  calculateFinalScore() {
    const p = Number(this.presentationScore || 0);
    const d = Number(this.demoScore || 0);
    const q = Number(this.qaScore || 0);
    this.finalScore = Number(((p * 0.3) + (d * 0.5) + (q * 0.2)).toFixed(2));
  }

  submitDefenseEvaluation() {
    this.errorMessage = ''; // Reset lỗi cũ
    if (!this.selectedDefense) return;

    // 1. Kiểm tra Required Fields (Thiếu tiêu chí)
    if (this.presentationScore === null || this.demoScore === null || this.qaScore === null ||
        this.presentationScore === undefined || this.demoScore === undefined || this.qaScore === undefined) {
        this.errorMessage = "Required evaluation fields missing."; 
        return;
    }

    // 2. Kiểm tra Logic điểm hợp lệ (0-10)
    const scores = [this.presentationScore, this.demoScore, this.qaScore];
    const isInvalid = scores.some(score => score < 0 || score > 10);

    if (isInvalid) {
        this.errorMessage = "All scores must be between 0 and 10.";
        return;
    }

    const userId = this.authService.getUserId();
    if (!userId) {
        this.errorMessage = "User session invalid. Please log in again.";
        return;
    }

    const payload: DefenseEvaluationRequest = {
        defenseId: this.selectedDefense.defenseId,
        councilMemberUserId: Number(userId),
        presentationScore: this.presentationScore,
        demoScore: this.demoScore,
        qaScore: this.qaScore,
        finalScore: this.finalScore,
        comment: this.evaluationComment
    };

    this.defenseService.submitEvaluation(payload).subscribe({
        next: () => {
            this.successMessage = `Evaluation for ${this.selectedDefense?.groupName} saved successfully!`;
            this.showEvaluationForm = false;
            this.loadAssignedDefenses();
            setTimeout(() => this.successMessage = '', 3000);
        },
        error: (err) => {
            this.errorMessage = err.error?.message || "Failed to save evaluation.";
        }
    });
}

  resetForm() {
    this.presentationScore = 0;
    this.demoScore = 0;
    this.qaScore = 0;
    this.finalScore = 0;
    this.evaluationComment = '';
    this.errorMessage = '';
  }

  onLogout() {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => {
        this.authService.clearToken();
        this.router.navigate(['/login']);
      }
    });
  }
}