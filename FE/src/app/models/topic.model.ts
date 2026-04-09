export interface TopicSubmitRequest {
  groupId: number;
  title: string;
  description: string;
}

export interface TopicDto {
  versionId: number; 
  topicId: number;
  groupId: number;
  groupName?: string;
  title: string;
  description: string;
  status: string;
  submittedAt?: Date;
  reviewComment?: string;
}

export interface TopicApprovalRequest {
  versionId: number; 
  topicId: number;
  status: string;
  reviewComment: string;
}