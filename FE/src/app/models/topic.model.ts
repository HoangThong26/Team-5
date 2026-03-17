export interface TopicSubmitRequest {
  groupId: number;
  title: string;
  description: string;
}

export interface TopicDto {
  versionId: number; // ID từ bảng TopicVersions
  topicId: number;
  groupId: number;
  groupName?: string;
  title: string;
  description: string;
  status: string;
  submittedAt?: Date;
}

export interface TopicApprovalRequest {
  versionId: number; // Thêm trường này
  topicId: number;
  status: string;
  reviewComment: string;
}