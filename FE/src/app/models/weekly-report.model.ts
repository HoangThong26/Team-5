export interface WeeklyReportRequest {
    groupId: number;
    content: string;
    githubLink?: string;
    fileUrl?: string;
    weekId?: number; 
}

export interface WeeklyReportDto {
  reportId: number;
  groupId: number;
  weekNumber: number;
  content: string;
  submittedAt: Date;
  githubLink?: string;
  fileUrl?: string;
  Status?: string;
  status?: string;
  weekId?: number;
  groupName?: string;
  GroupName?: string;
  name?: string;
  group?: {
    name?: string;
    groupName?: string;
  };
  // Evaluation fields (populated when status is 'Reviewed')
  score?: number;
  comment?: string;
  Score?: number;
  Comment?: string;
}

export interface WeeklyReportHistoryDto {
  reportId: number;
  weekId?: number;
  content?: string;
  status?: string;
  submittedAt?: Date;
  githubLink?: string;
  fileUrl?: string;

  mentorComment?: string;
  score?: number;     
  isPass: boolean;    
  reviewedAt?: Date;
  mentorName?: string;
}
