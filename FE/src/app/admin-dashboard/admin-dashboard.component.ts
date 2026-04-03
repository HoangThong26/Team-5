import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { AdminService } from '../services/admin.service';
import { GradeService, FinalGrade } from '../services/grade.service';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CouncilService, Staff } from '../services/council.service';
import { DefenseService } from '../services/defense.service';
import {
  CreateDefenseScheduleRequest,
  DefenseCommitteeDto,
  DefenseRegistrationItemDto,
  UpdateDefenseScheduleRequest
} from '../models/defense-registration.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [FormsModule],
  templateUrl:  './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {

  // User list
  users: any[] = [];
  isLoadingUsers = false;
  isExporting: boolean = false;

  // --- CÁC BIẾN VÀ HÀM PHÂN TRANG ---
  currentPage = 1;
  pageSize = 10;

  get pagedUsers() {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    return this.users.slice(startIndex, startIndex + this.pageSize);
  }

  get totalPages() {
    return Math.ceil(this.users.length / this.pageSize) || 1;
  }

  get pageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  nextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  previousPage() {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }
  
   get scheduledDefenseCount(): number {
    return (this.defenseRegistrations || []).filter(r => !!r.startTime).length;
  }
  // ------------------------------------

  // Search
  searchKeyword = '';
  isSearching = false;

  // Create user form
  showCreateForm = false;
  newEmail = '';
  newFullName = '';
  newPhone = '';
  newRole = 'Student';
  newPassword = '';
  isCreating = false;

  // Import Excel form
  showImportForm = false;
  selectedFile: File | null = null;
  isImporting = false;

  // Messages
  successMessage = '';
  errorMessage = '';

  // Group Management
  groups: any[] = [];
  isLoadingGroups = false;
  viewMode: 'users' | 'groups' | 'timeline' | 'councils' | 'defense' | 'grades' = 'users';

  // Thêm các biến quản lý điểm
  grades: FinalGrade[] = [];
  isLoadingGrades = false;
  
    // Defense schedule management
  defenseRegistrations: DefenseRegistrationItemDto[] = [];
  defenseCommittees: DefenseCommitteeDto[] = [];
  isLoadingDefense = false;
  isSavingDefense = false;
  defenseScheduleDraft: Record<number, {
    date: string;
    startTime: string;
    endTime: string;
    room: string;
    councilId: number;
  }> = {};

  // Council Management
  availableStaffs: Staff[] = [];
  createdCouncils: DefenseCommitteeDto[] = [];
  isLoadingStaffs = false;
  isLoadingCouncils = false;
  isCreatingCouncil = false;
  
  // Council Form
  newCouncilName = '';
  selectedStaffIds: number[] = [];
  
  // Council Staff Search & Pagination
  staffSearchKeyword = '';
  staffCurrentPage = 1;
  staffPageSize = 5;

  // Timeline Setup
  projectStartDate: string = '';
  isSettingTimeline = false;

  // --- THÊM BIẾN LƯU DANH SÁCH MENTOR ---
  mentors: any[] = []; 

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router,
    private gradeService: GradeService,
    private councilService: CouncilService,
    private defenseService: DefenseService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.setInitialViewFromRoute();
    this.loadUsers();
    this.loadAllStatsOnInit();
  }

  private setInitialViewFromRoute() {
    const requestedView = this.route.snapshot.data['viewMode'];
    if (requestedView === 'defense') {
      this.viewMode = 'defense';
      this.loadDefenseScheduleData();
    }
  }

  /** Load groups & mentors silently on startup so stat cards have correct data */
  private loadAllStatsOnInit() {
    this.adminService.getAllGroups().subscribe({
      next: (res) => { this.groups = res; },
      error: () => {}
    });
    this.adminService.getAllUsers().subscribe({
      next: (res: any[]) => {
        if (this.users.length === 0) this.users = res;
        this.mentors = res.filter(u => (u?.role || '').toLowerCase() === 'mentor');
      },
      error: () => {}
    });
  }

  private extractError(err: any, fallback: string): string {
    const error = err?.error;
    if (typeof error === 'string') return error;
    if (error?.errors) {
      const messages = Object.values(error.errors).flat();
      return (messages as string[]).join(' ');
    }
    if (error?.message) return error.message;
    if (error?.title) return error.title;
    return fallback;
  }

  loadUsers() {
    this.isLoadingUsers = true;
    this.adminService.getAllUsers().subscribe({
      next: (res) => {
        this.users = res;
        this.isLoadingUsers = false;
        this.currentPage = 1; // Reset về trang 1 khi load data mới
      },
      error: (err) => {
        this.isLoadingUsers = false;
        this.errorMessage = 'Unable to load user list.';
      }
    });
  }

  // --- QUẢN LÝ CREATE FORM ---
  toggleCreateForm() {
    this.showCreateForm = !this.showCreateForm;
    this.successMessage = '';
    this.errorMessage = '';
    // Đóng form Import nếu đang mở
    if (this.showCreateForm) {
      this.showImportForm = false; 
    }
  }

  createUser() {
    this.successMessage = '';
    this.errorMessage = '';

    if (!this.newEmail.trim() || !this.newFullName.trim() || !this.newPassword.trim()) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isCreating = true;

    this.adminService.createUser({
      email: this.newEmail,
      fullName: this.newFullName,
      phone: this.newPhone,
      role: this.newRole,
      password: this.newPassword
    }).subscribe({
      next: (res: any) => {
        this.isCreating = false;
        this.successMessage = res?.message || 'User created successfully!';
        this.resetForm();
        this.loadUsers();
      },
      error: (err) => {
        this.isCreating = false;
        this.errorMessage = this.extractError(err, 'Failed to create user!');
      }
    });
  }

  resetForm() {
    this.newEmail = '';
    this.newFullName = '';
    this.newPhone = '';
    this.newRole = 'Student';
    this.newPassword = '';
  }

  // --- QUẢN LÝ IMPORT EXCEL FORM ---
  toggleImportForm() {
    this.showImportForm = !this.showImportForm;
    this.successMessage = '';
    this.errorMessage = '';
    // Đóng form Create nếu đang mở
    if (this.showImportForm) {
      this.showCreateForm = false;
    }
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select an Excel file first.';
      return;
    }

    this.isImporting = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.adminService.importUsers(this.selectedFile).subscribe({
      next: (res: any) => {
        this.isImporting = false;
        this.successMessage = res?.message || 'Users imported successfully!';
        this.selectedFile = null;
        this.showImportForm = false; // Đóng form sau khi thành công
        this.loadUsers(); // Tự động làm mới danh sách
      },
      error: (err) => {
        this.isImporting = false;
        this.errorMessage = this.extractError(err, 'Failed to import users!');
      }
    });
  }

  // --- QUẢN LÝ TÌM KIẾM & LOGOUT ---
  searchUsers() {
    const keyword = this.searchKeyword.trim();
    if (!keyword) {
      this.loadUsers();
      return;
    }

    this.isSearching = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.adminService.searchUsers(keyword).subscribe({
      next: (res) => {
        this.users = res;
        this.isSearching = false;
        this.successMessage = `Found ${res.length} results for "${keyword}".`;
        this.currentPage = 1; // Reset về trang 1 khi có kết quả tìm kiếm
      },
      error: (err) => {
        this.isSearching = false;
        this.errorMessage = this.extractError(err, 'Search failed!');
      }
    });
  }

  clearSearch() {
    this.searchKeyword = '';
    this.successMessage = '';
    this.errorMessage = '';
    this.loadUsers();
  }

  logout() {
    this.authService.logout().subscribe({
      next: () => this.router.navigate(['/login']),
      error: () => {
        this.authService.clearToken();
        this.router.navigate(['/login']);
      }
    });
  }

  downloadStudentList() {
    this.isExporting = true;
    this.errorMessage = ''; 
    this.successMessage = ''; 

    this.adminService.exportStudents().subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        
        const a = document.createElement('a');
        a.href = url;
      
        const today = new Date();
        const dd = String(today.getDate()).padStart(2, '0');
        const mm = String(today.getMonth() + 1).padStart(2, '0'); 
        const yyyy = today.getFullYear();
        
        a.download = `StudentList_${dd}${mm}${yyyy}.xlsx`; 
      
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url); 
        
        this.isExporting = false;
      },
      error: (error) => {
        console.error('Error exporting Excel file:', error);
        this.errorMessage = 'An error occurred while exporting the student list!';
        this.isExporting = false;
      }
    });
  }

  switchView(mode: 'users' | 'groups' | 'timeline' | 'councils' | 'defense' | 'grades') {
    this.viewMode = mode;
    this.successMessage = '';
    this.errorMessage = '';
    
    if (mode === 'users') {
      if (this.users.length === 0) {
        this.loadUsers();
      }
    } else if (mode === 'groups') {
      this.loadGroups();
      this.loadMentors(); 
    } else if (mode === 'timeline') {
      // Có thể load ngày hiện tại từ API nếu cần
    } else if (mode === 'grades') {
    this.loadAllGrades();
    } else if (mode === 'councils') {
      this.staffCurrentPage = 1;
      this.staffSearchKeyword = '';
      this.loadAvailableStaffs();
      this.loadCreatedCouncils();
    } else if (mode === 'defense') {
      this.loadDefenseScheduleData();
    }
  }

  loadDefenseScheduleData() {
    this.isLoadingDefense = true;
    this.errorMessage = '';
    this.successMessage = '';

    const ensureUsers = this.users.length > 0
      ? Promise.resolve()
      : new Promise<void>((resolve) => {
          this.adminService.getAllUsers().subscribe({
            next: (res: any[]) => {
              this.users = res || [];
              resolve();
            },
            error: () => resolve()
          });
        });

    ensureUsers.then(() => {
      forkJoin({
        registrations: this.defenseService.getRegistrations(),
        committees: this.defenseService.getCommittees()
      }).subscribe({
        next: (data) => {
          this.defenseRegistrations = data.registrations || [];
          this.defenseCommittees = (data.committees || []).filter(c => (c.members || []).length >= 3);
          this.prepareScheduleDrafts();
          this.isLoadingDefense = false;
        },
        error: (err) => {
          this.isLoadingDefense = false;
          this.errorMessage = this.extractError(err, 'Could not load defense data.');
        }
      });
    });
  }

  // Các hàm xử lý nghiệp vụ điểm
  loadAllGrades() {
    this.isLoadingGrades = true;
    this.gradeService.getAllGrades().subscribe({
      next: (res) => {
        this.grades = res;
        this.isLoadingGrades = false;
      },
      error: (err) => {
        this.isLoadingGrades = false;
        this.errorMessage = 'Could not load grade list.';
      }
    });
  }

  handleCalculateGrade(groupId: number) {
    this.isLoadingGrades = true;
    this.gradeService.calculateGrade(groupId).subscribe({
      next: (res) => {
        this.successMessage = `Calculated score: ${res.score}`;
        this.loadAllGrades(); // Refresh danh sách
      },
      error: (err) => {
        this.isLoadingGrades = false;
        this.errorMessage = this.extractError(err, 'Failed to calculate grade. Ensure all scores are submitted.');
      }
    });
  }

  handlePublishGrade(groupId: number) {
    if (confirm('Are you sure you want to publish this grade? Students will be able to see it.')) {
      this.gradeService.publishGrade(groupId).subscribe({
        next: () => {
          this.successMessage = 'Grade published successfully!';
          this.loadAllGrades();
        },
        error: (err) => this.errorMessage = this.extractError(err, 'Failed to publish grade.')
      });
    }
  }

  loadGroups() {
    this.isLoadingGroups = true;
    this.adminService.getAllGroups().subscribe({
      next: (res) => {
        this.groups = res;
        this.isLoadingGroups = false;
      },
      error: (err) => {
        this.isLoadingGroups = false;
        this.errorMessage = 'Could not load group list.';
      }
    });
  }

  loadMentors() {
    if (this.users.length > 0) {
      this.mentors = this.users.filter(u => u.role === 'Mentor');
    } else {
      this.adminService.getAllUsers().subscribe({
        next: (res: any[]) => {
          this.users = res;
          this.mentors = res.filter(u => u.role === 'Mentor');
        },
        error: (err) => console.error('Could not load mentor list', err)
      });
    }
  }

  handleAssignMentor(groupId: number, mentorId: number) {
  if (!mentorId) {
    this.errorMessage = 'Please select a mentor first.';
    return; 
  }

  this.isLoadingGroups = true;
  this.successMessage = '';
  this.errorMessage = '';

  this.adminService.assignMentor(groupId, mentorId).subscribe({
    next: (res: any) => {
      this.isLoadingGroups = false; 
      this.successMessage = res?.message || 'Mentor assigned successfully!';
      this.loadGroups(); 
    },
    error: (err) => {
      this.isLoadingGroups = false;
      this.errorMessage = this.extractError(err, 'Failed to assign mentor.');
      console.error('Lỗi từ API:', err);
    }
  });
}

  handleKickMentor(groupId: number) {
    if (confirm('Are you sure you want to remove the mentor from this group?')) {
      this.adminService.kickMentor(groupId).subscribe({
        next: (res: any) => {
          this.successMessage = res.message;
          this.loadGroups();
        },
        error: (err) => this.errorMessage = this.extractError(err, 'Failed to kick mentor.')
      });
    }
  }
  handleDeleteGroup(groupId: number) {
    if (confirm('WARNING: This will delete the group and all its assignments. Proceed?')) {
      this.adminService.deleteGroup(groupId).subscribe({
        next: (res: any) => {
          this.successMessage = res.message;
          this.loadGroups();
        },
        error: (err) => this.errorMessage = this.extractError(err, 'Failed to delete group.')
      });
    }
  }

  handleSetupTimeline() {
    if (!this.projectStartDate) {
      this.errorMessage = 'Please select a start date.';
      return;
    }

    this.isSettingTimeline = true;
    this.successMessage = '';
    this.errorMessage = '';

    this.adminService.setupTimeline(this.projectStartDate).subscribe({
      next: (res: any) => {
        this.isSettingTimeline = false;
        this.successMessage = res?.message || 'Project timeline has been set successfully!';
      },
      error: (err) => {
        this.isSettingTimeline = false;
        this.errorMessage = this.extractError(err, 'Failed to setup timeline.');
      }
    });
  }

  getRoleGradient(role: string): string {
    switch ((role || '').toLowerCase()) {
      case 'admin':   return 'linear-gradient(135deg,#ef4444,#b91c1c)';
      case 'mentor':  return 'linear-gradient(135deg,#7c3aed,#a855f7)';
      case 'council': return 'linear-gradient(135deg,#d97706,#f59e0b)';
      default:        return 'linear-gradient(135deg,#2563eb,#3b82f6)';
    }
  }

  /** Returns full names of members beyond index 3, for the +N bubble tooltip */
  getExtraNames(members: any[]): string {
    if (!members || members.length <= 4) return '';
    return members.slice(4).map((m: any) => m.fullName || 'Unknown').join(', ');
  }
  // --- COUNCIL METHODS ---

  loadAvailableStaffs() {
    this.isLoadingStaffs = true;
    this.councilService.getAvailableStaffs().subscribe({
      next: (res) => {
        this.availableStaffs = res.data || [];
        this.isLoadingStaffs = false;
      },
      error: (err) => {
        this.isLoadingStaffs = false;
        this.errorMessage = 'Could not load available staff.';
      }
    });
  }

  loadCreatedCouncils() {
    this.isLoadingCouncils = true;
    this.defenseService.getCommittees().subscribe({
      next: (res) => {
        this.createdCouncils = res || [];
        this.isLoadingCouncils = false;
      },
      error: () => {
        this.isLoadingCouncils = false;
        this.errorMessage = 'Could not load created councils.';
      }
    });
  }

  toggleStaffSelection(staffId: number) {
    const index = this.selectedStaffIds.indexOf(staffId);
    if (index > -1) {
      this.selectedStaffIds.splice(index, 1);
    } else {
      this.selectedStaffIds.push(staffId);
    }
  }

  createCouncil() {
    if (!this.newCouncilName) {
      this.errorMessage = 'Please fill in required fields (Name).';
      return;
    }

    if (this.selectedStaffIds.length === 0) {
      this.errorMessage = 'Please select at least one staff member.';
      return;
    }

    this.isCreatingCouncil = true;
    this.successMessage = '';
    this.errorMessage = '';

    const payload = {
      name: this.newCouncilName,
      memberIds: this.selectedStaffIds
    };

    this.councilService.createFullCouncil(payload).subscribe({
      next: (res: any) => {
        this.isCreatingCouncil = false;
        if (res.success) {
          this.successMessage = res.message || 'Council created successfully!';
          this.resetCouncilForm();
          this.loadCreatedCouncils();
        } else {
          this.errorMessage = res.message || 'Failed to create council.';
        }
      },
      error: (err) => {
        this.isCreatingCouncil = false;
        this.errorMessage = this.extractError(err, 'Failed to create council.');
      }
    });
  }

  resetCouncilForm() {
    this.newCouncilName = '';
    this.selectedStaffIds = [];
    this.staffSearchKeyword = '';
    this.staffCurrentPage = 1;
  }

  get pagedStaffs(): Staff[] {
    const start = (this.staffCurrentPage - 1) * this.staffPageSize;
    return this.availableStaffs.slice(start, start + this.staffPageSize);
  }

  get totalStaffPages(): number {
    return Math.ceil(this.availableStaffs.length / this.staffPageSize) || 1;
  }

  get staffPageNumbers(): number[] {
    return Array.from({ length: this.totalStaffPages }, (_, i) => i + 1);
  }

  goToStaffPage(p: number) {
    if (p >= 1 && p <= this.totalStaffPages) this.staffCurrentPage = p;
  }

  searchStaffs() {
    if (!this.staffSearchKeyword.trim()) {
      this.loadAvailableStaffs();
      return;
    }
    this.isLoadingStaffs = true;
    this.councilService.searchStaff(this.staffSearchKeyword).subscribe({
      next: (res) => {
        this.availableStaffs = res.data || [];
        this.isLoadingStaffs = false;
        this.staffCurrentPage = 1;
      },
      error: (err) => {
        this.isLoadingStaffs = false;
        this.errorMessage = 'Staff search failed.';
      }
    });
  }
   prepareScheduleDrafts() {
    for (const registration of this.defenseRegistrations) {
      if (this.defenseScheduleDraft[registration.defenseId]) {
        continue;
      }

      const start = registration.startTime ? new Date(registration.startTime) : null;
      const end = registration.endTime ? new Date(registration.endTime) : null;

      this.defenseScheduleDraft[registration.defenseId] = {
        date: this.toDateInputValue(start),
        startTime: this.toTimeInputValue(start),
        endTime: this.toTimeInputValue(end),
        room: registration.room || '',
        councilId: registration.councilId || 0
      };
    }
  }

  hasExistingSchedule(item: DefenseRegistrationItemDto): boolean {
    return !!(item.startTime || item.room || item.councilId);
  }

  getCommitteeLabel(councilId?: number): string {
    if (!councilId) {
      return 'Not assigned';
    }

    const committee = this.defenseCommittees.find(c => c.councilId === councilId);
    return committee ? committee.councilName : `Council #${councilId}`;
  }

  getCommitteeOptionLabel(committee: DefenseCommitteeDto): string {
    const memberCount = committee.members?.length || 0;
    const memberText = memberCount === 1 ? 'member' : 'members';
    return `${committee.councilName} (${memberCount} ${memberText})`;
  }

  saveDefenseSchedule(item: DefenseRegistrationItemDto) {
    const draft = this.defenseScheduleDraft[item.defenseId];
    if (!draft) {
      this.errorMessage = 'Schedule draft not found.';
      return;
    }

    if (!draft.date || !draft.startTime || !draft.room.trim() || !draft.councilId) {
      this.errorMessage = 'Please provide Date, Time, Room and a valid Council.';
      return;
    }

    const startTime = this.toIsoDateTime(draft.date, draft.startTime);
    const endTime = draft.endTime ? this.toIsoDateTime(draft.date, draft.endTime) : null;

    this.isSavingDefense = true;
    this.successMessage = '';
    this.errorMessage = '';

    if (this.hasExistingSchedule(item)) {
      const payload: UpdateDefenseScheduleRequest = {
        councilId: draft.councilId,
        room: draft.room.trim(),
        startTime,
        endTime
      };

      this.defenseService.updateSchedule(item.defenseId, payload).subscribe({
        next: (res) => {
          this.isSavingDefense = false;
          this.successMessage = res?.message || 'Defense schedule updated successfully.';
          this.refreshDefenseRegistrations();
        },
        error: (err) => {
          this.isSavingDefense = false;
          this.errorMessage = this.extractError(err, 'Failed to update defense schedule.');
        }
      });
      return;
    }

    const payload: CreateDefenseScheduleRequest = {
      defenseId: item.defenseId,
      councilId: draft.councilId,
      room: draft.room.trim(),
      startTime,
      endTime
    };

    this.defenseService.createSchedule(payload).subscribe({
      next: (res) => {
        this.isSavingDefense = false;
        this.successMessage = res?.message || 'Defense schedule saved successfully.';
        this.refreshDefenseRegistrations();
      },
      error: (err) => {
        this.isSavingDefense = false;
        this.errorMessage = this.extractError(err, 'Failed to save defense schedule.');
      }
    });
  }

  private refreshDefenseRegistrations() {
    this.defenseService.getRegistrations().subscribe({
      next: (res) => {
        this.defenseRegistrations = res || [];
        this.defenseScheduleDraft = {};
        this.prepareScheduleDrafts();
      },
      error: (err) => {
        this.errorMessage = this.extractError(err, 'Could not refresh defense registrations.');
      }
    });
  }

  private toDateInputValue(value: Date | null): string {
    if (!value) {
      return '';
    }

    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private toTimeInputValue(value: Date | null): string {
    if (!value) {
      return '';
    }

    const hours = String(value.getHours()).padStart(2, '0');
    const minutes = String(value.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  private toIsoDateTime(dateValue: string, timeValue: string): string {
    return `${dateValue}T${timeValue}`;
  }

  formatDateTime(value?: string): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
      return value;
    }

    const dd = String(date.getDate()).padStart(2, '0');
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const yyyy = date.getFullYear();
    const hh = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    return `${dd}/${mm}/${yyyy} ${hh}:${min}`;
  }
}

