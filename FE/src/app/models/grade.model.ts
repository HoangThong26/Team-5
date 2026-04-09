export interface FinalGrade {
  groupId: number;
  averageScore?: number; 
  gradeLetter?: string;
  isPublished: boolean;
  publishedAt?: Date | string;
  groupName?: string; 
  topicName?: string;
  group?: {
    groupName: string;
    topic?: { topicName: string };
  };
}

export interface CalculateGradeResponse {
  message: string;
  score: number;
}